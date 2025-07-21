use axum::{
    extract::{
        ws::{Message, WebSocket, WebSocketUpgrade},
        Extension,
        connect_info::ConnectInfo
    },
    routing::get,
    Router,
    http::StatusCode,
    error_handling::HandleErrorLayer,
    response::IntoResponse,
};
use axum_extra::TypedHeader;
use tokio::sync::RwLock;
use tower::{BoxError, ServiceBuilder};
use tower_http::trace::TraceLayer;
use tracing_subscriber::{layer::SubscriberExt, util::SubscriberInitExt};
use std::{
    net::SocketAddr,
    time::Duration,
    collections::HashMap,
    sync::Arc
};
use serde_json::json;
use futures_util::{
    StreamExt,
    SinkExt
};

//
// 接続情報はサーバープロセス内だけで保持する
// 接続情報はスレッド間で共有するため、ArcとRwLockを使用する
// Arc<T>: スレッド間で安全に共有できる参照カウント付きスマートポインタ
// RwLock: 書き込みロック(待たせる)
//

// クライアント向け送信チャネル型
type Tx = tokio::sync::mpsc::UnboundedSender<Message>;

// 接続中クライアントID -> クライアント向け送信チャネル型
type Clients = HashMap<String, Tx>;

// ルーム構造体
#[derive(Default)]
struct Room {
    clients: Clients,
    player_idx: u8,
    match_time_sec: usize,
    idx_on_duty: usize,
}
// ルームを番号 -> ルーム構造体型
type Rooms = Arc<RwLock<HashMap<u8, Room>>>;

type NumOfRooms = u8;   // ルーム数用
type NumOfClients = u8; // クライアント数用

const MAX_NUM_ROOMS: NumOfRooms = 255;   // 最大ルーム数
const MAX_NUM_CLIENTS: NumOfClients = 2; // 最大クライアント数(1ルーム内)
const MAX_NUM_PLAYER_INDEX: u8 = 255;    // 最大プレイヤー番号(1ルーム内)
const ROUTINE_INTERVAL_MS:u64 = 100;     // 定期処理間隔(ミリ秒)
const MATCH_TIME_SEC:usize = 300;        // 試合時間(秒)

#[tokio::main]
async fn main() {
    let rooms: Rooms = Arc::new(RwLock::new(HashMap::new()));

    // tracingはイベントやパフォーマンスの計測データを収集するフレームワーク
    // tracing_subscriberはそれらのデータを受け取り、フィルタリングやフォーマット、出力を行う
    // registryはログデータの収集、分配を行う
    tracing_subscriber::registry()
        .with(
            // 環境変数RUST_LOGのログレベルによってログをフィルターする
            // 環境変数が無ければ今のクレート、tower_httpのログレベルにdebugを設定する
            tracing_subscriber::EnvFilter::try_from_default_env().unwrap_or_else(|_| {
                format!("{}=debug,tower_http=debug", env!("CARGO_CRATE_NAME")).into()
            }),
        )
        // layerでログのフィルタリング、フォーマット変更、外部システムへの送信を設定する
        .with(tracing_subscriber::fmt::layer())
        // 設定したログをアプリケーションに適用する
        .init();

    // Router定義
    let app = Router::new()
        // .routeでエンドポイントとハンドラー関数を紐づける
        .route("/ws/", get(handler_ws))
        // .layerで通信処理過程に処理を追加できる
        // Extensionでハンドラー内からデータにアクセスできるようにする
        .layer(Extension(rooms.clone()))
        .layer(ServiceBuilder::new().layer(HandleErrorLayer::new(|error: BoxError| async move {
            if error.is::<tower::timeout::error::Elapsed>() {
                Ok(StatusCode::REQUEST_TIMEOUT)
            } else {
                Err((
                    StatusCode::INTERNAL_SERVER_ERROR,
                    format!("Unhandled internal error: {error}"),
                ))
            }
        }))
        // 指定秒数でタイムアウトエラーをトリガーする
        .timeout(Duration::from_secs(10))
        .layer(TraceLayer::new_for_http())
        // Routerに設定を適用
        .into_inner()
    );

    // 指定ポートにTCPリスナーをバインドする
    // バインドされたTcpListenerインスタンスが返される
    let listener = tokio::net::TcpListener::bind("127.0.0.1:3000").await.unwrap();

    // サーバーのアドレス、ポートを出力
    tracing::debug!("Listening on {}", listener.local_addr().unwrap());

    // 定期処理開始
    start_duty_loop(rooms.clone());

    // TcpListenerとRouterのインスタンスを渡してHTTPサーバーを起動
    axum::serve(
        listener,
        app.into_make_service_with_connect_info::<SocketAddr>(),
    )
    .await
    .unwrap();
}

// WebSocketエンドポイントへのアクセスを処理
async fn handler_ws(
    ws: WebSocketUpgrade,
    Extension(rooms): Extension<Rooms>,
    ConnectInfo(addr): ConnectInfo<SocketAddr>,
    user_agent: Option<TypedHeader<headers::UserAgent>>,
) -> impl IntoResponse {

    // 接続元の情報を出力
    let user_agent = if let Some(TypedHeader(user_agent)) = user_agent {
        user_agent.to_string()
    } else {
        String::from("Unknown browser")
    };
    tracing::debug!("`{}` at {} connected.", user_agent, addr);

    // WebSocket接続が確立したらクライアント毎の通信処理に入る
    ws.on_upgrade(move |socket| client_connection(socket, rooms))
}

// WebSocketクライアントとの接続管理
// 各クライアント毎に1回ずつ呼ばれ、中で継続して通信を処理する
async fn client_connection(
    socket: WebSocket,
    rooms: Rooms
) {
    // WebSocketを読み書きに分離
    let (mut sender, mut receiver) = socket.split();

    // WebSocketへの送信用チャネルを作成
    let (tx, mut rx) = tokio::sync::mpsc::unbounded_channel::<Message>();

    let mut room_id = 0;               // この通信に割り当てるルーム番号
    let mut client_id = String::new(); // この通信に割り当てるクライアント識別子

    // 空いてるルームを探して割り当てる
    // IDに0は使わないことにする
    {
        let mut rooms_writable = rooms.write().await;
    
        for n in 1..=MAX_NUM_ROOMS {
            // 既存のルーム
            if let Some(room) = rooms_writable.get_mut(&n) {
                // 現在のクライアント数を取得
                let room_members_len = room.clients.len();
                // 満員なら次のルームへ
                if room_members_len >= MAX_NUM_CLIENTS.into() {
                    continue;
                }
                // ルームIDをセット
                room_id = n;
                // ルーム内のプレイヤー番号を更新
                room.player_idx = if room.player_idx < MAX_NUM_PLAYER_INDEX {
                    room.player_idx + 1
                } else {
                    1
                };
                // クライアントIDを作成
                client_id = format!("{}-{}", room_id, room.player_idx);
                // ルームにクライアントを追加
                room.clients.insert(client_id.clone(), tx);

                break;
            } else {
                // ルーム数が上限なら通信中断
                if n == MAX_NUM_ROOMS {
                    tracing::debug!("Number of rooms exceeded!");
                    return;
                }
                // ルームIDをセット
                room_id = n;
                // ルーム生成
                let mut new_room = Room::default();
                // ルーム内のプレイヤー番号を初期化
                new_room.player_idx = 1;
                // クライアントIDを作成
                client_id = format!("{}-{}", room_id, new_room.player_idx);
                // ルームにクライアントを追加
                new_room.clients.insert(client_id.clone(), tx);
                // 新しいルームを追加
                rooms_writable.insert(room_id, new_room);

                break;
            }
        }
    }

    // 非同期タスク間通信を別スレッドで実行
    // tx.sendを受け取ってsender.sendでクライアントに送信する
    // tx(clone含む)が全て無くなるとrx.recv()はNoneを返す
    tokio::spawn(async move {
        while let Some(msg) = rx.recv().await {
            // クライアントが切断したら終了
            if sender.send(msg).await.is_err() {
                break;
            }
        }
    });

    // クライアントに割り当てた情報を送信
    if let Some(room) = rooms.read().await.get(&room_id) {
        if let Some(tx) = room.clients.get(&client_id) {
            let _ = tx.send(Message::Text(json!({
                "ctrl": 1,        // 識別子割り当て用の通信と明示
                "cid": client_id, // 割り当てるクライアント識別子
            }).to_string().into()));

            tracing::debug!("{} joined room {}", client_id, room_id);
        }
    }

    //
    // クライアントから受信したメッセージを同じルームの他クライアントに転送
    // receiver.next()が接続終了(None)やエラー(Err)を切り分けて返している
    //
    // use axum::extract::ws::Message;
    // pub enum Message {
    // Text(String),                       // クライアントからのテキストメッセージ
    // Binary(Vec<u8>),                    // バイナリメッセージ
    // Ping(Vec<u8>),                      // WebSocket ping
    // Pong(Vec<u8>),                      // WebSocket pong
    // Close(Option<CloseFrame<'static>>), // 切断要求(理由あり、または無し)
    // }
    //
    while let Some(Ok(msg)) = receiver.next().await {
        match msg {
            Message::Text(text) => {
                println!("received from {} [{}]", client_id, text.to_string());

                let rooms_read = rooms.read().await;
                
                // 現在のルームに参加するクライアントを取得
                if let Some(room) = rooms_read.get(&room_id) {
                    for (member_id, tx) in &room.clients {
                        // 自分以外のクライアント
                        if *member_id != client_id {
                            // サーバーが受信した内容をそのまま送信
                            let _ = tx.send(Message::Text(text.clone()));

                            println!("**** {} -> {} ****", client_id, member_id);
                        }
                    }
                }
            }
            // Closeが送られてこない場合もある
            Message::Close(Some(frame)) => {
                println!("disconnect from {} (code: {}, reason: {})", client_id, frame.code, frame.reason);
                break;
            }
            Message::Close(None) => {
                println!("disconnect from {} client (no reason)", client_id);
                break;
            }
            _ => {}
        }
    }

    // 接続終了
    tracing::debug!("{} left room {}", client_id, room_id);

    {
        // ルームからクライアントを削除
        let mut is_room_empty = false;
        let mut rooms_guard = rooms.write().await;
        if let Some(room) = rooms_guard.get_mut(&room_id) {
            room.clients.retain(|id, _tx| id != &client_id);

            // ルームにクライアントがいなくなったらルームを削除
            // 要再検討
            if room.clients.is_empty() {
                is_room_empty = true;
            }
        }
        if is_room_empty {
            rooms_guard.remove(&room_id);
        }
    }
}

// 定期的にサーバーからクライアントに処理を依頼する
fn start_duty_loop(rooms: Rooms) {
    tokio::spawn(async move {
        loop {
            {
                let mut rooms_guard = rooms.write().await;

                for (_room_id, room) in rooms_guard.iter_mut() {
                    let client_ids: Vec<String> = room.clients.keys().cloned().collect();

                    if client_ids.is_empty() {
                        continue;
                    }

                    // 当番のクライアントIDを取得
                    let idx = room.idx_on_duty % client_ids.len();
                    let client_id = &client_ids[idx];

                    // クライアントに処理依頼を送信
                    if let Some(tx) = room.clients.get(client_id) {
                        let _ = tx.send(Message::Text(json!({
                            "ctrl": 2, // 共通処理の当番と明示
                        }).to_string().into()));

                        println!("//// {} is on duty! ////", client_id);
                    }

                    // クライアントの当番インデックスを更新
                    room.idx_on_duty = (room.idx_on_duty + 1) % client_ids.len();
                }
            }

            // 実行間隔を調整
            tokio::time::sleep(std::time::Duration::from_millis(ROUTINE_INTERVAL_MS)).await;
        }
    });
}
