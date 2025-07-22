using UnityEngine;

// アイテムクラス
public class Item : SynchronizedObject
{
    protected int itemId;
    protected int prefabId;
    protected bool isUpdated;
    protected bool isMoving;

    protected float positionWeight = 0.75f;
    protected float rotationWeight = 0.75f;

    protected float moveSqrThreshold = 0.01f;
    protected float delayResetSyncState = 0f;
    protected float timerResetSyncState;

    protected Player closePlayer;
    protected Rigidbody rbody;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        rbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        // 動いてるフラグ更新
        UpdateIsMoving();

        // 同期状態を元に戻す
        ResetSyncState();

        // 受信内容をセット
        ApplyReceivedData();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        // すでにプレイヤーセット済みなら終了
        if (other == closePlayer)
        {
            return;
        }

        // プレイヤー取得
        Player player = other.GetComponent<Player>();
        if (player == null)
        {
            return;
        }

        // 同期状態を変更
        if (player.IsMyself())
        {
            SetSyncState(SyncState.SendOnly);
        }
        else
        {
            SetSyncState(SyncState.ReceiveOnly);
        }

        // 最寄りのプレイヤーをセット
        closePlayer = player;
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);

        // 最寄りのプレイヤーを解除
        closePlayer = null;

        // 同期状態を元に戻す
        timerResetSyncState = delayResetSyncState;
    }

    // 動いてるフラグ更新
    protected virtual void UpdateIsMoving()
    {
        isMoving = rbody.velocity.sqrMagnitude > moveSqrThreshold;
    }

    // 受信内容が更新されてたらセット
    protected virtual void ApplyReceivedData()
    {
        if (isUpdated)
        {
            rbody.MovePosition(Vector3.Lerp(transform.position, receivedPosition, positionWeight));
            rbody.MoveRotation(transform.localRotation = Quaternion.Slerp(transform.localRotation, receivedRotation, rotationWeight));
        }
        isUpdated = false;
    }

    // 同期状態を元に戻す
    protected virtual void ResetSyncState()
    {
        if (syncState == SyncState.Bidirectional)
        {
            return;
        }

        if (!isMoving && timerResetSyncState <= 0f)
        {
            syncState = SyncState.Bidirectional;
            timerResetSyncState = delayResetSyncState;

            return;
        }

        if (timerResetSyncState > 0)
        {
            timerResetSyncState -= Time.fixedDeltaTime;
        }
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
