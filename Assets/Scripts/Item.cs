using UnityEngine;

// アイテムクラス
public class Item : SynchronizedObject
{
    protected int itemId;
    protected int prefabId;

    public int points;
    public float groundCheckDistance;

    protected bool isUpdated;
    protected bool isMoving;

    [SerializeField]
    protected bool isGrounded;

    protected float positionWeight = 0.75f;
    protected float rotationWeight = 0.75f;
    protected float moveSqrThreshold;

    protected Rigidbody rbody;

    protected override void Awake()
    {
        rbody = GetComponent<Rigidbody>();
        moveSqrThreshold = positionThreshold * positionThreshold;

        SetSyncState(SyncState.ReceiveOnly);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        // 動いてるフラグ更新
        isMoving = rbody.velocity.sqrMagnitude > moveSqrThreshold;

        // 接地フラグ更新
        isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, groundCheckDistance + 0.1f);

        // 受信した内容をセット
        if (isUpdated)
        {
            transform.position = Vector3.Lerp(transform.position, receivedPosition, positionWeight);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, receivedRotation, rotationWeight);
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

    protected override void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player == null)
        {
            return;
        }

        if (!isGrounded)
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
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
    }

    public override void SetSyncState(SyncState state)
    {
        base.SetSyncState(state);

        switch (state)
        {
            case SyncState.ReceiveOnly:
                rbody.isKinematic = true;
                break;

            case SyncState.SendOnly:
                rbody.isKinematic = false;
                break;

            default:
                rbody.isKinematic = false;
                break;
        }
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
