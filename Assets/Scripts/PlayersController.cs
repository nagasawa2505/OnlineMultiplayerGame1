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
        // 引数チェック
        if (string.IsNullOrEmpty(clientId))
        {
            MyDebug.Log("クライアントID取得失敗");
            return null;
        }

        // 通信用クライアントIDチェック
        string thisClientId = WebSocketClient.GetClientId();
        if (string.IsNullOrEmpty(thisClientId))
        {
            MyDebug.Log("クライアントID未割り当て");
            return null;
        }

        // Prefab取得
        GameObject playerPrefab = PrefabStrage.GetDefaultPlayer();
        if (playerPrefab == null)
        {
            MyDebug.Log("Prefab取得失敗");
            return null;
        }

        // プレイヤー生成
        GameObject playerObj = Object.Instantiate(playerPrefab, new Vector3(0, GameController.spawnAxisY, 1), Quaternion.identity);
        if (playerObj == null)
        {
            MyDebug.Log("プレイヤー生成失敗");
            return null;
        }

        Player player;
        bool isMyself = clientId == thisClientId;

        // 操作対象のプレイヤー
        if (isMyself)
        {
            player = playerObj.AddComponent<MyPlayer>();
            myPlayer = (MyPlayer)player;
            MyDebug.SetText(0, $"player spawned cid={clientId} time={Time.time}", true);
        }
        else
        {
            player = playerObj.AddComponent<OtherPlayer>();
        }

        // 操作対象かをセット
        player.SetIsMyself(isMyself);

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
        Item eventItem = myPlayer.GetEventItem();
        if (eventItem != null) {
            dc.SetPlayerEventAndItem(myPlayer.GetPlayerEvent(), eventItem.GetItemId());

            // アイテム情報セット
            dc.AddItemId(eventItem.GetItemId());
            dc.AddItemPrefabId(eventItem.GetPrefabId());
            dc.AddItemPosition(eventItem.transform.position);
            dc.AddItemRotation(eventItem.transform.localRotation);
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
        player.SetPlayerEvent(evt, ItemsController.GetItem(itemId));

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
}
