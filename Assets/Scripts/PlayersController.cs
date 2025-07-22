using System.Collections.Generic;
using UnityEngine;

// プレイヤー管理クラス
public static class PlayersController
{
    static Dictionary<string, Player> players = new();
    static Player myPlayer;
    const float TimeoutSecExit = 10f;

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

        // コンポーネント取得
        Player player = playerObj.GetComponent<Player>();

        // この端末のプレイヤーならセット
        bool isMyself = clientId == thisClientId;
        if (isMyself)
        {
            myPlayer = player;
            MyDebug.SetText(0, $"player spawned cid={clientId} time={Time.time}", true);
        }

        // この端末のプレイヤーかをセット
        player.SetIsMyself(isMyself);

        // クライアントIDをセット
        player.SetClientId(clientId);

        // タイマーセット
        player.SetExitTimer(TimeoutSecExit);

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
            dc.pos = position;
        }

        // 回転をセット
        Quaternion rotation = myPlayer.transform.localRotation;
        if (myPlayer.IsSentRotationChanged(ref rotation))
        {
            dc.rot = new Vector4(rotation.x, rotation.y, rotation.z, rotation.w);
        }

        // イベントをセット
        dc.evt = myPlayer.GetEvent();

        // 持ち物があればセット
        CarryableItem carryable = myPlayer.GetCarryingItem();
        dc.has = carryable != null ? carryable.GetItemId() : 0;

        // 接してる物があればセット
        Collider otherCollider = myPlayer.GetOtherCollder();
        if (otherCollider != null)
        {
            Item item = otherCollider.GetComponent<Item>();
            if (item != null)
            {
                dc.itmid = new();
                dc.itmidx = new();
                dc.itmpos = new();
                dc.itmrot = new();

                Vector3 itemPos = item.transform.position;
                Quaternion itemRot = item.transform.localRotation;

                dc.itmid.Add(item.GetItemId());
                dc.itmidx.Add(item.GetPrefabId());
                dc.itmpos.Add(itemPos);
                dc.itmrot.Add(new Vector4(itemRot.x, itemRot.y, itemRot.z, itemRot.w));
            }
        }

        // 送信時の情報を保存しておく
        myPlayer.SetLastSentPosition(position);
        myPlayer.SetLastSentRotation(rotation);
    }

    // 受信内容をセット
    public static void SetReceivedData(DataContainer dc)
    {
        Player target;

        // 既存のプレイヤー
        if (players.TryGetValue(dc.cid, out Player player))
        {
            target = player;
        }
        // 新しいプレイヤー
        else
        {
            // ルームのプレイヤーとして追加
            target = SpawnPlayer(dc.cid);
        }

        // 位置をセット
        if (dc.pos != Vector3.zero)
        {
            target.SetReceivedPosition(dc.pos);
        }

        // 回転をセット
        if (dc.rot != Vector4.zero)
        {
            target.SetReceivedRotation(dc.rot);
        }

        // イベントをセット
        target.SetEvent(dc.evt);

        // 持ち物をセット
        if (dc.has == 0)
        {
            target.DropItem();
        }
        else
        {
            target.HoldItem((CarryableItem)ItemsController.GetItem(dc.has));
        }

        // 通信切断検知タイマー更新
        target.SetExitTimer(TimeoutSecExit);
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

    // この端末のプレイヤーを返す
    public static Player GetMyPlayer()
    {
        return myPlayer;
    }
}
