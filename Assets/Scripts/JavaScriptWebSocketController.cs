using UnityEngine;

// JavaScriptのWebSocket通信を仲介するクラス(WebGL用)
public class JavaScriptWebSocketController : MonoBehaviour
{
#if UNITY_WEBGL

    // JavaScriptから受け取ったJSONのパース用
    struct JavaScriptMessage
    {
        public string evt;
        public string data;
    }

    const float sendInterval = 0.05f; // 20Hz
    float sendTimer;

    // Update is called once per frame
    void Update()
    {
        if (!WebSocketClient.IsConnected())
        {
            return;
        }

        sendTimer += Time.deltaTime;
        if (sendTimer >= sendInterval)
        {
            WebSocketClient.Send();
            sendTimer = 0;
        }
    }

    // JavaScriptからデータを受け取る
    public void OnWebSocketMessage(string msg)
    {
        JavaScriptMessage jsMsg = JsonUtility.FromJson<JavaScriptMessage>(msg);
        switch (jsMsg.evt)
        {
            case "open":
                WebSocketClient.OnOpenWebSocket();
                break;

            case "receive":
                WebSocketClient.OnReceiveWebSocket(jsMsg.data);
                break;

            case "close":
                WebSocketClient.CloseConnection();
                break;

            case "error":
                break;

            default:
                break;
        }
    }
#endif
}
