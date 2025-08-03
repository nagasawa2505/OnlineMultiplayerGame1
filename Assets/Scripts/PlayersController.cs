using System.Collections.Generic;
using UnityEngine;

// プレイヤー管理クラス
public static class PlayersController
{
    static Dictionary<string, Player> players = new();
    static MyPlayer myPlayer;

    // プレイヤー生成
    public static Player SpawnPlayer(string clientId)
    {
        Player player;

        // 引数チェック
        if (string.IsNullOrEmpty(clientId))
        {
            Debug.Log("クライアントID取得失敗");
            return null;
        }

        // 通信用クライアントIDチェック
        string thisClientId = GameController.GetClientId();
        if (string.IsNullOrEmpty(thisClientId))
        {
            Debug.Log("クライアントID未割り当て");
            return null;
        }

        // Prefab取得
        int teamNum = GameController.GetTeamNumber(clientId);
        GameObject playerPrefab = PrefabStrage.GetPlayer(teamNum);
        if (playerPrefab == null)
        {
            Debug.Log("Prefab取得失敗");
            return null;
        }

        // チームごとにプレイヤーの位置と向きを調整
        Quaternion playerRotation;
        float playerPosZ;
        if (teamNum % 2 == 0)
        {
            playerRotation = Quaternion.Euler(0, 180, 0);
            playerPosZ = 45;
        }
        else
        {
            playerRotation = Quaternion.identity;
            playerPosZ = -45;
        }

        // プレイヤー生成
        GameObject playerObj = Object.Instantiate(playerPrefab, new Vector3(
            UnityEngine.Random.Range(-5, 5), GameController.spawnAxisY, playerPosZ), playerRotation);
        if (playerObj == null)
        {
            Debug.Log("プレイヤー生成失敗");
            return null;
        }

        // 操作対象のプレイヤー
        if (clientId == thisClientId)
        {
            player = playerObj.AddComponent<MyPlayer>();
            myPlayer = (MyPlayer)player;
        }
        else
        {
            player = playerObj.AddComponent<OtherPlayer>();
        }

        // クライアントIDをセット
        player.SetClientId(clientId);

        // 管理対象に追加
        players.Add(clientId, player);

        return player;
    }

    // 送信内容をセット
    public static void SetSendData(DataContainer dc)
    {
        // 位置をセット
        Vector3 position = myPlayer.transform.position;
        if (myPlayer.IsSentPositionChanged(ref position))
        {
            dc.SetPlayerPosition(position);
        }

        // 回転をセット
        Quaternion rotation = myPlayer.transform.localRotation;
        if (myPlayer.IsSentRotationChanged(ref rotation))
        {
            dc.SetPlayerRotation(rotation);
        }

        // イベント、対象アイテムIDをセット
        PlayerEvent evt = myPlayer.GetPlayerEvent();
        if (evt == PlayerEvent.None)
        {
            dc.SetPlayerEventAndItem(evt, 0);
        }
        else
        {
            Item eventItem = myPlayer.GetEventItem();
            if (eventItem == null)
            {
                dc.SetPlayerEventAndItem(evt, 0);
            }
            else
            {
                int itemId = eventItem.GetItemId();
                dc.SetPlayerEventAndItem(evt, itemId);
                dc.AddItemId(itemId);
                dc.AddItemPrefabId(eventItem.GetPrefabId());
                dc.AddItemPosition(eventItem.transform.position);
                dc.AddItemRotation(eventItem.transform.localRotation);
            }
        }

        // 送信時の情報を保存しておく
        myPlayer.SetLastSentPosition(position);
        myPlayer.SetLastSentRotation(rotation);
    }

    // 受信内容をセット
    public static void SetReceivedData(DataContainer dc)
    {
        OtherPlayer player;
        string clientId = dc.GetClientId();
        Vector3 position = dc.GetPlayerPosition();
        Quaternion rotation = dc.GetPlayerRotation();
        (PlayerEvent evt, int itemId) = dc.GetPlayerEventAndItem();

        // 既存のプレイヤー
        if (players.TryGetValue(clientId, out Player existing))
        {
            player = (OtherPlayer)existing;
        }
        // 新しいプレイヤー
        else
        {
            // ルームのプレイヤーとして追加
            player = (OtherPlayer)SpawnPlayer(clientId);
        }

        // 位置をセット
        if (position != Vector3.zero)
        {
            player.SetReceivedPosition(position);
        }

        // 回転をセット
        if (rotation != Quaternion.identity)
        {
            player.SetReceivedRotation(rotation);
        }

        // イベントをセット
        Item item = ItemsController.GetItem(itemId);
        player.SetReceivedPlayerEvent(evt, item);
        if (item != null)
        {
            item.SetSyncState(SyncState.ReceiveOnly);
        }

        // 通信切断検知タイマー更新
        player.ResetExitTimer();
    }

    // 通信が切れたプレイヤーを破棄
    public static void RemovePlayer(Player player)
    {
        // ルームに1人だけ
        if (players.Count == 1)
        {
            return;
        }

        // 持ち物は道連れにしない
        player.DropItem();

        // 管理対象から除外
        players.Remove(player.GetClientId());
    }

    // 操作対象のプレイヤーを返す
    public static Player GetMyPlayer()
    {
        return myPlayer;
    }

    // プレイヤー数を返す
    public static int GetPlayerCount()
    {
        return players.Count;
    }

    public static void Clear()
    {
        foreach (var (clientId, player) in players)
        {
            if (player != null)
            {
                Object.Destroy(player);
            }
        }
        myPlayer = null;
        players.Clear();
    }
}
