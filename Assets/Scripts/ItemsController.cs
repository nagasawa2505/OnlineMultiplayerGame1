using System.Collections.Generic;
using UnityEngine;

// アイテム管理クラス
public static class ItemsController
{
    static int lastItemId;
    static Dictionary<int, Item> items = new();

    // アイテム生成
    public static void SpawnItem(int prefabIndex, Vector3 position, Quaternion rotation, Transform transform)
    {
        // 生成対象のPrefabを取得
        GameObject prefab = PrefabStrage.GetItemByIndex(prefabIndex);
        if (prefab == null)
        {
            MyDebug.Log("Prefab取得失敗");
            return;
        }

        // アイテム生成
        GameObject obj = Object.Instantiate(prefab, position, rotation, transform);
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
    public static void SetSendData(DataContainer dc, bool isOnDuty)
    {
        foreach (var (itemId, item) in items)
        {
            if (item == null)
            {
                MyDebug.Log("アイテム破棄済み");
                continue;
            }

            // 同期状態を取得
            SyncState state = item.GetSyncState();

            // 当番なら担当なし以外はとばす
            if (isOnDuty)
            {
                if (state != SyncState.Bidirectional)
                {
                    continue;
                }
            }
            // 自分の担当分以外はとばす
            else
            {
                if (state != SyncState.SendOnly)
                {
                    continue;
                }
            }

            // 送信内容セット
            Vector3 position = item.transform.position;
            Quaternion rotation = item.transform.localRotation;
            dc.AddItemId(itemId);
            dc.AddItemPrefabId(item.GetPrefabId());
            dc.AddItemPosition(item.transform.position);
            dc.AddItemRotation(rotation);
        }
    }

    // 受信内容をセット
    public static void SetReceivedData(DataContainer dc)
    {
        List<int> ids = dc.GetItemIds();
        List<int> prefabIds = dc.GetItemPrefabIds();
        List<Vector3> positions = dc.GetItemPositions();
        List<Quaternion> rotations = dc.GetItemRotations();

        for (int i = 0; i < ids.Count; i++)
        {
            int itemId = ids[i];
            int prefabId = prefabIds[i];
            Vector3 position = positions[i];
            Quaternion rotation = rotations[i];

            if (items.TryGetValue(itemId, out Item item))
            {
                // 受信対象外ならとばす
                SyncState state = item.GetSyncState();
                if (state == SyncState.SendOnly || state == SyncState.None)
                {
                    item.SetIsUpdated(false);
                    continue;
                }

                // 位置と回転をセット
                if (position != Vector3.zero)
                {
                    item.SetReceivedPosition(position);
                    item.SetReceivedRotation(rotation);
                }

                // 更新フラグを立てる
                item.SetIsUpdated(true);
            }
            else
            {
                // 管理対象外なら新たに生成
                SpawnItem(prefabId, position, rotation, GameController.GetTransform());
            }
        }
    }

    // アイテムを返す
    public static Item GetItem(int itemId)
    {
        if (items.TryGetValue(itemId, out Item item))
        {
            return item;
        }

        return null;
    }
}
