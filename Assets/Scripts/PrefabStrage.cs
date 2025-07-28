using UnityEngine;

// Prefab管理クラス
public class PrefabStrage : MonoBehaviour
{
    static PrefabStrage self;

    public GameObject[] players;
    public GameObject[] items;

    // エディタで触りたいのでMonoBehaviourを継承する
    private void Awake()
    {
        if (self != null && self != this)
        {
            Destroy(gameObject);
            return;
        }
        self = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        if (players.Length == 0)
        {
            MyDebug.Log("プレイヤー未登録");
        }

        if (items.Length == 0)
        {
            MyDebug.Log("アイテム未登録");
        }
    }

    public static GameObject GetPlayer(int teamNumber)
    {
        return self.players[teamNumber - 1];
    }

    public static GameObject GetPlayerByIndex(int index)
    {
        return self.players[index];
    }

    public static GameObject GetItemByIndex(int index)
    {
        return self.items[index];
    }
}
