using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public enum DataType : byte
{
    Normal,
    Init,
    Duty,
    Start,
}

// JsonUtility
// メンバがprivateだと値が初期値になる
// メンバの変数名がJSONのキーになる
[Serializable]
public class DataContainer
{
    public DataType    k1; // データの用途
    public string      k2; // クライアントID
    public ulong       k3; // プレイヤーの位置(16B * 3 + 空16B)
    public ulong       k4; // プレイヤーの回転(16B * 4)
    public ushort      k5; // プレイヤーの状態、対象アイテムID(8B * 2)
    public List<int>   k6; // アイテムID(複数)
    public List<int>   k7; // アイテムPrefabID(複数)
    public List<ulong> k8; // アイテムの位置(16B * 3 + 空16B)
    public List<ulong> k9; // アイテムの回転(16B * 4)

    public DataContainer()
    {
        k6 = new();
        k7 = new();
        k8 = new();
        k9 = new();
    }

    public void SetDataType(DataType type)
    {
       k1 = type;
    }

    public DataType GetDataType()
    {
        return k1;
    }

    public void SetClientId(string id)
    {
        k2 = id;
    }

    public string GetClientId()
    {
        return k2;
    }

    public void SetPlayerPosition(Vector3 pos)
    {
        k3 = ConvertPosition(pos);
    }

    public Vector3 GetPlayerPosition()
    {
        return RestorePosition(k3);
    }

    public void SetPlayerRotation(Quaternion rot)
    {
        k4 = ConvertRotation(rot);
    }

    public Quaternion GetPlayerRotation()
    {
        return RestoreRotation(k4);
    }

    public void SetPlayerEventAndItem(PlayerEvent evt, int itemId)
    {
        ushort ue = (ushort)evt;
        ushort ui = (ushort)itemId;

        k5 = (ushort)(ue << 8 | ui);
    }

    public (PlayerEvent evt, int itemId) GetPlayerEventAndItem()
    {
        ushort uEvt = (ushort)((k5 >> 8) & 0xFF);
        ushort uItemId = (ushort)(k5 & 0xFF);

        byte bEvt = (byte)uEvt;
        byte bItemId = (byte)uItemId;

        return ((PlayerEvent)bEvt, (int)bItemId);
    }

    public void AddItemId(int itemId)
    {
        k6.Add(itemId);
    }

    public List<int> GetItemIds()
    {
        return k6;
    }

    public void AddItemPrefabId(int prefabId)
    {
        k7.Add(prefabId);
    }

    public List<int> GetItemPrefabIds()
    {
        return k7;
    }

    public void AddItemPosition(Vector3 pos)
    {
        k8.Add(ConvertPosition(pos));
    }

    public List<Vector3> GetItemPositions()
    {
        List<Vector3> positions = new();

        foreach (ulong bPos in k8)
        {
            Vector3 pos = RestorePosition(bPos);
            positions.Add(pos);
        }

        return positions;
    }

    public void AddItemRotation(Quaternion rot)
    {
        k9.Add(ConvertRotation(rot));
    }

    public List<Quaternion> GetItemRotations()
    {
        List<Quaternion> rotations = new();

        foreach (ulong bRot in k9)
        {
            Quaternion rot = RestoreRotation(bRot);
            rotations.Add(rot);
        }

        return rotations;
    }

    private ulong ConvertPosition(Vector3 pos)
    {
        half hx = (half)pos.x;
        half hy = (half)pos.y;
        half hz = (half)pos.z;
        short pad = 0;

        ulong ux = (ulong)hx.value;
        ulong uy = (ulong)hy.value;
        ulong uz = (ulong)hz.value;
        ulong uPad = (ulong)pad;

        // 上位から詰める
        return (ux << 48) | (uy << 32) | (uz << 16) | uPad;
    }

    private Vector3 RestorePosition(ulong bPos)
    {
        float x = (float)new half { value = (ushort)((bPos >> 48) & 0xFFFF) };
        float y = (float)new half { value = (ushort)((bPos >> 32) & 0xFFFF) };
        float z = (float)new half { value = (ushort)((bPos >> 16) & 0xFFFF) };

        return new Vector3(x, y, z);
    }

    private ulong ConvertRotation(Quaternion rot)
    {
        half hx = (half)rot.x;
        half hy = (half)rot.y;
        half hz = (half)rot.z;
        half hw = (half)rot.w;

        ulong ux = (ulong)hx.value;
        ulong uy = (ulong)hy.value;
        ulong uz = (ulong)hz.value;
        ulong uw = (ulong)hw.value;

        // 上位から詰める
        return (ux << 48) | (uy << 32) | (uz << 16) | uw;
    }

    private Quaternion RestoreRotation(ulong bRot)
    {
        float x = (float)new half { value = (ushort)((bRot >> 48) & 0xFFFF) };
        float y = (float)new half { value = (ushort)((bRot >> 32) & 0xFFFF) };
        float z = (float)new half { value = (ushort)((bRot >> 16) & 0xFFFF) };
        float w = (float)new half { value = (ushort)(bRot & 0xFFFF) };

        return new Quaternion(x, y, z, w).normalized;
    }
}
