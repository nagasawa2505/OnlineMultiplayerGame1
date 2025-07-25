using UnityEngine;

// アイテムクラス
public class Item : SynchronizedObject
{
    protected int itemId;
    protected int prefabId;
    protected bool isUpdated;
    protected bool isMoving;

    protected float positionWeight = 0.5f;
    protected float rotationWeight = 0.25f;

    protected float moveSqrThreshold = 0.75f;
    protected float delayResetSyncState = 5f;
    protected float timerResetSyncState;

    protected Rigidbody rbody;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        rbody = GetComponent<Rigidbody>();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        // 動いてるフラグ更新
        isMoving = rbody.velocity.sqrMagnitude > moveSqrThreshold;
        if (!isMoving)
        {
            if (timerResetSyncState > 0)
            {
                timerResetSyncState -= Time.fixedDeltaTime;
            }
            else
            {
                ResetSyncState();
            }
        }

        // 受信した内容をセット
        if (isUpdated)
        {
            rbody.MovePosition(Vector3.Lerp(transform.position, receivedPosition, positionWeight));
            rbody.MoveRotation(transform.localRotation = Quaternion.Slerp(transform.localRotation, receivedRotation, rotationWeight));
        }
        isUpdated = false;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
    }

    // 同期状態を元に戻す
    protected virtual void ResetSyncState()
    {
        syncState = SyncState.Bidirectional;
    }

    protected virtual void ResetTimer()
    {
        timerResetSyncState = delayResetSyncState;
    }

    public virtual void SetSyncState(SyncState state)
    {
        ResetTimer();
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
