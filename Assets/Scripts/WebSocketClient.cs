using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

// WebSocket通信制御クラス
public static class WebSocketClient
{
    static ClientWebSocket ws;
    static CancellationTokenSource cancel;

    const string Host = Config.Host;
    const string Port = Config.Port;
    const int ReceiveBufSize = 1024;
    const int IntervalMs = 20;
    const int TimeoutSec = 10;

    static string clientId;
    static bool isOnDuty;

    // 双方向通信開始
    public static async void StartConnection()
    {
        // 通信中なら終了
        if (ws != null)
        {
            return;
        }

        ws = new ClientWebSocket();
        cancel = new CancellationTokenSource();

        // サーバーに接続
        await ws.ConnectAsync(new Uri($"ws://{Host}:{Port}/ws/"), cancel.Token);

        // 送信ループ
        _ = SendLoop().ContinueWith(t =>
        {
            MyDebug.Log((t.Exception?.Flatten().InnerException).ToString());
        }, TaskContinuationOptions.OnlyOnFaulted);

        // 受信ループ
        _ = ReceiveLoop().ContinueWith(t =>
        {
            MyDebug.Log((t.Exception?.Flatten().InnerException).ToString());
        }, TaskContinuationOptions.OnlyOnFaulted);
    }

    // 双方向通信終了
    public static async void EndConnection()
   {
        if (ws == null)
        {
            return;
        }

        try
        {
            // 通信処理停止
            cancel.Cancel();

            // 通信が切断されてなければ切断
            if (ws.State == WebSocketState.Open || ws.State == WebSocketState.CloseReceived)
            {
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "client closed", CancellationToken.None);
            }
        }
        catch (Exception e)
        {
            MyDebug.Log(e.ToString());
        }
        finally
        {
            ws.Dispose();
        }
    }

    // 送信ループ
    static async Task SendLoop()
    {
        while (ws.State == WebSocketState.Open)
        {
            int timer = 0;
            int waitMs = TimeoutSec * 1000;

            // プレイヤー生成を待つ
            while (PlayersController.GetMyPlayer() == null && timer <= waitMs)
            {
                await Task.Delay(IntervalMs);
                timer += IntervalMs;
            }

            if (PlayersController.GetMyPlayer() == null)
            {
                throw new Exception(MyDebug.Log("プレイヤー生成タイムアウト"));
            }

            // 送信データ作成
            DataContainer dc = new DataContainer();
            SetSendData(dc);

            // JSON作成
            string json = JsonUtility.ToJson(dc);
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            MyDebug.SetText(2, json);

            // 送信
            await ws.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, cancel.Token);

            // 送信間隔を調整
            await Task.Delay(IntervalMs);
        }
    }

    // 受信ループ
    static async Task ReceiveLoop()
    {
        byte[] buffer = new byte[ReceiveBufSize];

        while (ws.State == WebSocketState.Open)
        {
            // 受信
            var segment = new ArraySegment<byte>(buffer);
            WebSocketReceiveResult result = await ws.ReceiveAsync(segment, cancel.Token);

            // クローズ要求されたら終了
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "server closed", cancel.Token);
                throw new Exception(MyDebug.Log("サーバーからクローズ要求"));
            }

            // FromJsonはエラーハンドリングできないので要注意
            string received = Encoding.UTF8.GetString(buffer, 0, result.Count);
            DataContainer dc = JsonUtility.FromJson<DataContainer>(received);
            MyDebug.SetText(1, received);

            DataType type = dc.GetDataType();
            switch (type)
            {
                // 通常の通信
                case DataType.Normal:
                    // 受信内容を反映
                    SetReceivedData(dc);
                    break;

                // 初回の通信
                case DataType.Init:
                    // サーバーから割り当てられた識別子をセット
                    clientId = dc.GetClientId();

                    // この端末のプレイヤーを生成
                    Player newPlayer = PlayersController.SpawnPlayer(clientId);
                    if (newPlayer == null)
                    {
                        throw new Exception(MyDebug.Log("プレイヤー生成失敗"));
                    }
                    break;

                // 持ち回り当番の通信
                case DataType.Duty:
                    isOnDuty = true;
                    break;
            }
        }
    }

    // 送信内容をセット
    static void SetSendData(DataContainer dc)
    {
        // クライアントIDセット
        dc.SetClientId(clientId);

        // プレイヤー情報セット
        PlayersController.SetSendData(dc);

        // アイテム情報セット
        ItemsController.SetSendData(dc, false);

        // 持ち回り当番
        if (isOnDuty)
        {
            // 迷子アイテム情報セット
            ItemsController.SetSendData(dc, true);

            isOnDuty = false;
        }
    }

    // 受信内容をセット
    static void SetReceivedData(DataContainer dc)
    {
        // プレイヤー情報セット
        PlayersController.SetReceivedData(dc);

        // アイテム情報セット
        ItemsController.SetReceivedData(dc);
    }

    // このクライアントの通信用IDを返す
    public static string GetClientId()
    {
        return clientId;
    }
}
