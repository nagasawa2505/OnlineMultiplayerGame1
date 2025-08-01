#if UNITY_WEBGL
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

#else
﻿using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

#endif

// WebSocket通信制御クラス
public static partial class WebSocketClient
{
    static bool isOnDuty;

    // 送信内容をセット
    static void SetSendData(DataContainer dc)
    {
        // クライアントIDセット
        dc.SetClientId(GameController.GetClientId());

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
}

#if UNITY_WEBGL
// WebGL用
public static partial class WebSocketClient
{
    const string JavaScriptWebSocketGameObjectName = "JavaScriptWebSocketController";
    const string JavaScriptWebSocketCallbackMethodName = "OnWebSocketMessage";

    static bool isConnected;

    [DllImport("__Internal")]
    private static extern void WebSocketConnect(string url, string gameObjectName, string callbackMethodName);

    [DllImport("__Internal")]
    private static extern void WebSocketSend(string msg);

    [DllImport("__Internal")]
    private static extern void WebSocketClose();

    // 双方向通信開始
    public static void OpenConnection(int playerCount)
    {
        if (isConnected)
        {
            return;
        }

        // サーバーに接続
        WebSocketConnect(
            Config.WebsocketEndpoint + $"/{playerCount}",
            JavaScriptWebSocketGameObjectName,
            JavaScriptWebSocketCallbackMethodName
        );
    }

    // 双方向通信終了
    public static void CloseConnection()
    {
        if (!isConnected)
        {
            return;
        }

        WebSocketClose();
        isConnected = false;
    }

    // 通信してるかを返す
    public static bool IsConnected()
    {
        return isConnected;
    }

    // JavaScriptでWebSocket通信を開始したとき
    public static void OnOpenWebSocket()
    {
        isConnected = true;
    }

    // JavaScriptでWebSocket通信を受信したとき
    public static void OnReceiveWebSocket(string data)
    {
        if (!isConnected || string.IsNullOrEmpty(data))
        {
            return;
        }

        // FromJsonはエラーハンドリングできないので要注意
        DataContainer dc = JsonUtility.FromJson<DataContainer>(data);
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
                GameController.SpawnMyPlayer(dc.GetClientId());
                break;

            // 持ち回り当番の通信
            case DataType.Duty:
                isOnDuty = true;
                break;

            // 試合開始
            case DataType.Start:
                GameController.StartGame();
                break;
        }
    }

    // JavaScriptでWebSocket通信を送信する
    public static void Send()
    {
        if (!isConnected)
        {
            return;
        }

        if (PlayersController.GetMyPlayer() == null)
        {
            return;
        }

        // 送信データ作成
        DataContainer dc = new DataContainer();
        SetSendData(dc);

        // JSON作成
        string json = JsonUtility.ToJson(dc);
        byte[] buffer = Encoding.UTF8.GetBytes(json);

        // 送信
        WebSocketSend(json);
    }
}

#else
// WebGL以外用
public static partial class WebSocketClient
{
    static ClientWebSocket ws;
    static CancellationTokenSource cancel;

    const int sendIntervalMs = 50; // 20Hz
    const int TimeoutMs = 10000;

    // 双方向通信開始
    public static async void OpenConnection(int playerCount)
    {
        // 通信中なら終了
        if (ws != null)
        {
            return;
        }

        ws = new ClientWebSocket();
        cancel = new CancellationTokenSource();

        // サーバーに接続
        await ws.ConnectAsync(new Uri(Config.WebsocketEndpoint + $"/{playerCount}"), cancel.Token);

        // 送信ループ
        _ = SendLoop().ContinueWith(t =>
        {
            Debug.Log((t.Exception?.Flatten().InnerException).ToString());
        }, TaskContinuationOptions.OnlyOnFaulted);

        // 受信ループ
        _ = ReceiveLoop().ContinueWith(t =>
        {
            Debug.Log((t.Exception?.Flatten().InnerException).ToString());
        }, TaskContinuationOptions.OnlyOnFaulted);
    }

    // 双方向通信終了
    public static async void CloseConnection()
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
            Debug.Log(e.ToString());
        }
        finally
        {
            if (ws != null)
            {
                ws.Dispose();
                ws = null;
                cancel = null;
            }
        }
    }

    // 送信ループ
    static async Task SendLoop()
    {
        // プレイヤー生成を待つ
        int timer = 0;
        while (PlayersController.GetMyPlayer() == null && timer <= TimeoutMs)
        {
            await Task.Delay(100);
            timer += 100;
        }
        if (PlayersController.GetMyPlayer() == null)
        {
            throw new Exception("プレイヤー生成タイムアウト");
        }

        while (ws != null && ws.State == WebSocketState.Open)
        {
            // 送信データ作成
            DataContainer dc = new DataContainer();
            SetSendData(dc);

            // JSON作成
            string json = JsonUtility.ToJson(dc);
            byte[] buffer = Encoding.UTF8.GetBytes(json);

            // 送信
            await ws.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, cancel.Token);

            // 送信間隔を調整
            await Task.Delay(sendIntervalMs);
        }
    }

    // 受信ループ
    static async Task ReceiveLoop()
    {
        byte[] buffer = new byte[2048];

        while (ws != null && ws.State == WebSocketState.Open)
        {
            // 受信
            var segment = new ArraySegment<byte>(buffer);
            WebSocketReceiveResult result = await ws.ReceiveAsync(segment, cancel.Token);

            // クローズ要求されたら終了
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "server closed", cancel.Token);
                throw new Exception("サーバーからクローズ要求");
            }

            // FromJsonはエラーハンドリングできないので要注意
            string received = Encoding.UTF8.GetString(buffer, 0, result.Count);
            DataContainer dc = JsonUtility.FromJson<DataContainer>(received);

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
                    GameController.SpawnMyPlayer(dc.GetClientId());
                    break;

                // 持ち回り当番の通信
                case DataType.Duty:
                    isOnDuty = true;
                    break;

                // 試合開始
                case DataType.Start:
                    GameController.StartGame();
                    break;
            }
        }
    }
}
#endif
