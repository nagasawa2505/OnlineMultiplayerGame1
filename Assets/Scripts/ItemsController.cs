using System.Collections.Generic;
using UnityEngine;

// アイテム管理クラス
public static class ItemsController
{
    static int lastItemId;
    static Dictionary<int, Item> items = new();

    // アイテム生成
    public static void SpawnFieldItem(int prefabIndex, Vector3 position, Vector3 rotation, Transform transform)
    {
        // 生成対象のPrefabを取得
        GameObject prefab = PrefabStrage.GetItemByIndex(prefabIndex);
        if (prefab == null)
        {
            MyDebug.Log("Prefab取得失敗");
            return;
        }

        // アイテム生成
        GameObject obj = Object.Instantiate(prefab, position, Quaternion.Euler(rotation), transform);
        if (obj == null)
        {
            MyDebug.Log("アイテム生成失敗");
            return;
        }

        // コンポーネント取得
        Item item = obj.GetComponent<Item>();
        if (item == null)
        {
            MyDebug.Log("コンポーネント取得失敗");
            return;
        }

        // Prefabの番号をセット
        item.SetPrefabId(prefabIndex);

        // IDを更新してセット
        item.SetItemId(++lastItemId);

        // 管理対象に追加
        items.Add(lastItemId, item);
    }

    // 全アイテム破棄
    public static void Clear()
    {
        foreach (var (itemId, item) in items)
        {
            if (item != null)
            {
                Object.Destroy(item);
            }
        }
        items.Clear();
        lastItemId = 0;
    }

    // 送信内容をセット
    public static void SetSendData(DataContainer dc)
    {
        dc.itmid = new();
        dc.itmidx = new();
        dc.itmpos = new();
        dc.itmrot = new();

        foreach (var (itemId, item) in items)
        {
            if (item == null)
            {
                MyDebug.Log("アイテム破棄済み");
                continue;
            }

            // 送信対象外ならとばす
            SyncState state = item.GetSyncState();
            if (state == SyncState.ReceiveOnly || state == SyncState.None)
            {
                continue;
            }

            Vector3 position = item.transform.position;
            Quaternion rotation = item.transform.localRotation;

            if (item.IsSentPositionChanged(ref position) || item.IsSentRotationChanged(ref rotation))
            {
                dc.itmid.Add(itemId);
                dc.itmidx.Add(item.GetPrefabId());
                dc.itmpos.Add(item.transform.position);
                dc.itmrot.Add(new Vector4(rotation.x, rotation.y, rotation.z, rotation.w));

                item.SetLastSentPosition(position);
                item.SetLastSentRotation(rotation);
            }
        }
    }

    // 受信内容をセット
    public static void SetReceivedData(DataContainer dc)
    {
        for (int i = 0; i < dc.itmid.Count; i++)
        {
            int itemId = dc.itmid[i];
            int index = dc.itmidx[i];
            Vector3 position = dc.itmpos[i];
            Vector4 rotation = dc.itmrot[i];

            if (items.TryGetValue(itemId, out Item item))
            {
                // 受信対象外ならとばす
                SyncState state = item.GetSyncState();
                if (state == SyncState.SendOnly || state == SyncState.None)
                {
                    item.SetIsUpdated(false);
                    continue;
                }

                // 位置をセット
                if (position != Vector3.zero)
                {
                    item.SetReceivedPosition(position);
                }

                // 回転をセット
                if (rotation != Vector4.zero)
                {
                    item.SetReceivedRotation(rotation);
                }

                // 更新フラグを立てる
                item.SetIsUpdated(true);
            }
            else
            {
                // 管理対象外なら新たに生成
                SpawnFieldItem(index, position, rotation, GameController.GetTransform());
            }
        }
    }

    // アイテムを返す
    public static Item GetItem(int id)
    {
        return items[id];
    }
}
