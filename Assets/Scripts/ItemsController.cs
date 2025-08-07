using System.Collections.Generic;
using UnityEngine;

// アイテム管理クラス
public static class ItemsController
{
    static int lastItemId;
    static Dictionary<int, Item> items = new();

    // アイテム生成
    public static void SpawnItem(int prefabIndex, Vector3 position, Quaternion rotation, Transform transform, int itemId = 0)
    {
        // 生成対象のPrefabを取得
        GameObject prefab = PrefabStrage.GetItemByIndex(prefabIndex);
        if (prefab == null)
        {
            Debug.Log("Prefab取得失敗");
            return;
        }

        // アイテム生成
        GameObject obj = Object.Instantiate(prefab, position, rotation, transform);
        if (obj == null)
        {
            Debug.Log("アイテム生成失敗");
            return;
        }

        // コンポーネント取得
        Item item = obj.GetComponent<Item>();
        if (item == null)
        {
            Debug.Log("コンポーネント取得失敗");
            return;
        }

        // Prefabの番号をセット
        item.SetPrefabId(prefabIndex);

        if (itemId == 0)
        {
            // IDを更新してセット
            lastItemId++;
            item.SetItemId(lastItemId);
            items.Add(lastItemId, item);
        }
        else
        {
            // 受信したIDをセット
            item.SetItemId(itemId);
            items.Add(itemId, item);
        }   
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
        foreach (var (itemId, item) in items)
        {
            if (item == null)
            {
                continue;
            }

            // 自分の担当分以外はとばす
            if (!item.GetOwner().IsMyself())
            {
                continue;
            }

            // 送信内容セット
            Vector3 position = item.transform.position;
            Quaternion rotation = item.transform.localRotation;
            if (item.IsSentPositionChanged(ref position) || item.IsSentRotationChanged(ref rotation))
            {
                dc.AddItemId(itemId);
                dc.AddItemPrefabId(item.GetPrefabId());
                dc.AddItemPosition(item.transform.position);
                dc.AddItemRotation(rotation);
            }
        }
    }

    // 受信内容をセット
    public static void SetReceivedData(DataContainer dc)
    {
        string clientId = dc.GetClientId();
        Player sender = PlayersController.GetPlayer(clientId);
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
                // アイテムの情報送信者を更新
                if (sender != null && !item.IsFixedState())
                {
                    item.SetOwner(sender);
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
                SpawnItem(prefabId, position, rotation, GameController.GetTransform(), itemId);
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

    // 全アイテムの同期状態を変更する
    public static void OwnAllItems(Player player)
    {
        if (player == null)
        {
            return;
        }

        foreach (Item item in items.Values)
        {
            item.SetOwner(player);
        }
    }
}
