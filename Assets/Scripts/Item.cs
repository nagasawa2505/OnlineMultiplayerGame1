using UnityEngine;

// アイテムクラス
public class Item : SynchronizedObject
{
    [SerializeField]
    Player owner;

    [SerializeField]
    protected bool isFixedState;

    [SerializeField]
    protected bool isGrounded;

    protected int itemId;
    protected int prefabId;

    public int points;
    public float groundCheckDistance;

    protected bool isUpdated;
    protected bool isMoving;

    protected bool isPosted;
    protected float timerAfterPosted;
    protected const float DestroySecAfterPosted = 30f;

    protected float positionWeight = 0.75f;
    protected float rotationWeight = 0.75f;
    protected float moveSqrThreshold;

    protected Rigidbody rbody;

    protected override void Awake()
    {
        rbody = GetComponent<Rigidbody>();
        moveSqrThreshold = positionThreshold * positionThreshold;
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

        // ゴール配置済み
        if (isPosted)
        {
            timerAfterPosted += Time.fixedDeltaTime;
            if (timerAfterPosted >= DestroySecAfterPosted)
            {
                Die();
            }
        }
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
        if (isFixedState)
        {
            return;
        }

        if (!isGrounded)
        {
            return;
        }

        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            SetOwner(player);
            return;
        }

        Item item = other.GetComponent<Item>();
        if (item != null && !item.IsFixedState())
        {
            item.SetOwner(GetOwner());
        }
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
    }

    public virtual void SetOwner(Player player)
    {
        if (player != null)
        {
            owner = player;
        }
    }

    public virtual Player GetOwner()
    {
        return owner;
    }

    public virtual bool IsFixedState()
    {
        return isFixedState;
    }

    public virtual void Posted()
    {
        isPosted = true;
    }

    public virtual void Unposted()
    {
        isPosted = false;
        // timerAfterPosted = 0;
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
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
