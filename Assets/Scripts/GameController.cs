using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;

public enum GameState
{
    Title,
    Wait,
    Start,
    End,
}

public enum GameResult
{
    None,
    Win,
    Lose,
    Draw
}

// アプリケーション管理クラス
public partial class GameController : MonoBehaviour
{
    static GameController self;

    [SerializeField]
    GameState gameState;

    [SerializeField]
    string clientId;

    [SerializeField]
    int roomNumber;

    [SerializeField]
    int playerNumber;

    [SerializeField]
    int teamNumber;

    int roomCapacity;

    public static float spawnAxisY = 5f;

    void Awake()
    {
        if (self != null && self != this)
        {
            Destroy(gameObject);
            return;
        }
        self = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (this == self)
        {
            // WebSocket接続終了
            WebSocketClient.CloseConnection();
        }
    }

    private void OnApplicationQuit()
    {
        // WebSocket接続終了
        WebSocketClient.CloseConnection();
    }

    // シーン毎にアイテムを生成
    static void MakeScene()
    {
        string activeSceneName = SceneManager.GetActiveScene().name;
        int maxAxisX = 50;
        int maxAxisZ = 30;

        switch (activeSceneName)
        {
            case "Title":
                {
                    break;
                }
            case "Scene1":
                {
                    ItemsController.SpawnItem(0, new Vector3(UnityEngine.Random.Range(-maxAxisX, maxAxisX), spawnAxisY, UnityEngine.Random.Range(-maxAxisZ, maxAxisZ)), Quaternion.identity, GetTransform());
                    ItemsController.SpawnItem(0, new Vector3(UnityEngine.Random.Range(-maxAxisX, maxAxisX), spawnAxisY, UnityEngine.Random.Range(-maxAxisZ, maxAxisZ)), Quaternion.identity, GetTransform());
                    ItemsController.SpawnItem(0, new Vector3(UnityEngine.Random.Range(-maxAxisX, maxAxisX), spawnAxisY, UnityEngine.Random.Range(-maxAxisZ, maxAxisZ)), Quaternion.identity, GetTransform());
                    ItemsController.SpawnItem(1, new Vector3(UnityEngine.Random.Range(-maxAxisX, maxAxisX), spawnAxisY, UnityEngine.Random.Range(-maxAxisZ, maxAxisZ)), Quaternion.identity, GetTransform());
                    ItemsController.SpawnItem(1, new Vector3(UnityEngine.Random.Range(-maxAxisX, maxAxisX), spawnAxisY, UnityEngine.Random.Range(-maxAxisZ, maxAxisZ)), Quaternion.identity, GetTransform());
                    ItemsController.SpawnItem(1, new Vector3(UnityEngine.Random.Range(-maxAxisX, maxAxisX), spawnAxisY, UnityEngine.Random.Range(-maxAxisZ, maxAxisZ)), Quaternion.identity, GetTransform());
                    ItemsController.SpawnItem(1, new Vector3(UnityEngine.Random.Range(-maxAxisX, maxAxisX), spawnAxisY, UnityEngine.Random.Range(-maxAxisZ, maxAxisZ)), Quaternion.identity, GetTransform());
                    ItemsController.SpawnItem(1, new Vector3(UnityEngine.Random.Range(-maxAxisX, maxAxisX), spawnAxisY, UnityEngine.Random.Range(-maxAxisZ, maxAxisZ)), Quaternion.identity, GetTransform());
                    ItemsController.SpawnItem(1, new Vector3(UnityEngine.Random.Range(-maxAxisX, maxAxisX), spawnAxisY, UnityEngine.Random.Range(-maxAxisZ, maxAxisZ)), Quaternion.identity, GetTransform());
                    ItemsController.SpawnItem(2, new Vector3(0, spawnAxisY, 0), Quaternion.identity, GetTransform());

                    break;
                }
            default:
                {
                    break;
                }
        }
    }

    // 参加人数を選択したとき
    static void StartScene(int playerCount, string sceneName)
    {
        try
        {
            // WebSocket通信開始
            WebSocketClient.OpenConnection(playerCount);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            Application.Quit();
        }

        // シーン移動
        SetGameState(GameState.Wait);
        SceneManager.LoadScene(sceneName);
    }

    public static void StartScene2P()
    {
        self.roomCapacity = 2;
        StartScene(2, "Scene1");
    }

    public static void StartScene4P()
    {
        self.roomCapacity = 4;
        StartScene(4, "Scene1");
    }

    public static void StartScene6P()
    {
        self.roomCapacity = 6;
        StartScene(6, "Scene1");
    }

    // 初回通信に成功したとき
    public static void SpawnMyPlayer(string cid)
    {
        self.clientId = cid;

        string[] s = cid.Split('-');
        self.roomNumber = int.Parse(s[0]);
        self.playerNumber = int.Parse(s[1]);
        self.teamNumber = GetTeamNumber(cid);

        // この端末のプレイヤーを生成
        Player newPlayer = PlayersController.SpawnPlayer(self.clientId);
        if (newPlayer == null)
        {
            throw new Exception("プレイヤー生成失敗");
        }
    }

    // 参加人数が集まったとき
    public static void StartGame()
    {
        self.Invoke("SetGameStateStart", 3f);

        // 1人目に準備してもらう
        if (self.playerNumber == 1)
        {
            // アイテム生成
            MakeScene();

            // すべてのアイテム情報を送信する
            ItemsController.OwnAllItems(PlayersController.GetMyPlayer());
        }
    }

    // 試合が終了したとき
    public static void EndGame()
    {
        SetGameState(GameState.End);

        // WebSocket接続終了
        WebSocketClient.CloseConnection();
    }

    static void ClearSceneData()
    {
        // プレイヤー初期化
        PlayersController.Clear();

        // アイテム掃除
        ItemsController.Clear();

        foreach (Transform child in self.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public static void BackToTitle()
    {
        ClearSceneData();

        SetGameState(GameState.Title);

        SceneManager.LoadScene("Title");
    }

    public static int GetTeamNumber(string clientId)
    {
        string[] s = clientId.Split('-');
        return int.Parse(s[1]) % 2 == 1 ? 1 : 2;
    }

    public static Transform GetTransform()
    {
        return self.transform;
    }

    public static GameState GetGameState()
    {
        return self.gameState;
    }

    public static string GetClientId()
    {
        return self.clientId;
    }

    public static int GetRoomNumber()
    {
        return self.roomNumber;
    }

    public static int GetTeamNumber()
    {
        return self.teamNumber;
    }

    public static int GetRoomCapacity()
    {
        return self.roomCapacity;
    }

    public static void SetGameState(GameState state)
    {
        self.gameState = state;
    }

    void SetGameStateStart()
    {
        self.gameState = GameState.Start;
    }
}

public partial class GameController : MonoBehaviour
{
#if UNITY_WEBGL
    [DllImport("__Internal")]
    static extern int IsMobile();
#endif

    public static bool IsMobileDevice()
    {
#if UNITY_WEBGL
        return IsMobile() == 1;
#else
        return Application.isMobilePlatform;
#endif
    }
}
