using System;
using UnityEngine;
using UnityEngine.SceneManagement;

// アプリケーション管理クラス
public class GameController : MonoBehaviour
{
    static GameController self;

    public static string activeSceneName;

    public static float spawnAxisY = 20f;

    void Awake()
    {
        if (self != null && self != this)
        {
            Destroy(gameObject);
            return;
        }
        self = this;
        DontDestroyOnLoad(gameObject);

        // 現在のシーン名をセット
        activeSceneName = SceneManager.GetActiveScene().name;

        try
        {
            // WebSocket通信開始
            WebSocketClient.StartConnection();
        }
        catch (Exception e)
        {
            MyDebug.Log(e.Message);
            Application.Quit();
        }

        // アイテム掃除
        ItemsController.Clear();
    }

    void Start()
    {
        MakeSceneField();
    }

    void FixedUpdate()
    {

    }

    void Update()
    {
        
    }
    void LateUpdate()
    {

    }

    private void OnDestroy()
    {

    }

    void OnApplicationQuit()
    {
        // WebSocket接続終了
        WebSocketClient.EndConnection();
    }

    // アプリケーションのフォーカス取得、喪失時
    void OnApplicationFocus(bool focus)
    {

    }

    void MakeSceneField()
    {
        switch (activeSceneName)
        {
            case "title":
                {
                    break;
                }
            case "Scene1":
                {
                    ItemsController.SpawnFieldItem(0, new Vector3(0, spawnAxisY, 5), Vector3.zero, transform);
                    //ItemsController.SpawnFieldItem(0, new Vector3(0, spawnAxisY, 5), Vector3.zero, transform);
                    break;
                }
            case "Scene2":
                {
                    break;
                }
            case "Scene3":
                {
                    break;
                }
            default:
                {
                    break;
                }
        }
    }

    public static Transform GetTransform()
    {
        return self.transform;
    }
}
