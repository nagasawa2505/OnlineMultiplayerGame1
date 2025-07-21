using UnityEngine;

// アイテムクラス
public class Item : SynchronizedObject
{
    protected int itemId;
    protected int prefabId;
    protected bool isUpdated;
    protected Player closePlayer;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        if (other == closePlayer)
        {
            return;
        }

        Player player = other.GetComponent<Player>();
        if (player == null)
        {
            return;
        }

        if (player.IsMyself())
        {
            SetSyncState(SyncState.SendOnly);
        }
        else
        {
            SetSyncState(SyncState.ReceiveOnly);
        }
        closePlayer = player;
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);

        closePlayer = null;
        SetSyncState(SyncState.Bidirectional);
    }

    public virtual void SetSyncState(SyncState state)
    {
        syncState = state;
    }

    public virtual SyncState GetSyncState()
    {
        return syncState;
    }

    public virtual void SetIsUpdated(bool either)
    {
        isUpdated = either;
    }

    public virtual void SetItemId(int id)
    {
        if (itemId == 0)
        {
            itemId = id;
        }
    }

    public virtual int GetItemId()
    {
        return itemId;
    }

    public virtual void SetPrefabId(int id)
    {
        if (prefabId == 0)
        {
            prefabId = id;
        }
    }

    public virtual int GetPrefabId()
    {
        return prefabId;
    }
}
