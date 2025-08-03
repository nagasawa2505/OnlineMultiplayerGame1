using UnityEngine;

public enum PlayerEvent : int
{
    None,
    Kicking,
    Carrying,
    Throwing,
}

// プレイヤークラス
public abstract class Player : SynchronizedObject
{
    [SerializeField]
    protected PlayerEvent currentEvent;
    protected Item eventItem;

    string clientId;
    protected bool isMyself;

    protected float sqrMoveThreshold = 0.01f;

    protected float timerResetEvent;
    protected bool isStopTimerResetEvent;

    protected float moveFactor = 6f;
    protected float jumpFactor = 7.5f;
    protected float turnFactor = 80f;
    protected float kickFactor = 180f;

    protected Collider otherCollider;
    protected Rigidbody rbody;
    protected Animator animator;

    protected override void Awake()
    {
        base.Awake();
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        rbody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        // イベント初期化チェック
        if (!isStopTimerResetEvent && currentEvent != PlayerEvent.None)
        {
            if (timerResetEvent > 0)
            {
                timerResetEvent -= Time.deltaTime;
            }
            else
            {
                ResetPlayerEvent();
            }
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        if (other == otherCollider)
        {
            return;
        }

        // 接してる物をセット
        otherCollider = other;

        Player player = other.GetComponent<OtherPlayer>();
        if (player == null)
        {
            return;
        }

        if (currentEvent == PlayerEvent.Carrying)
        {
            ThrowItem();
        }
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);

        // 接してる物を解除
        otherCollider = null;
    }

    // 動いてるかを返す
    protected virtual bool IsMoving(Vector3 pos1, Vector3 pos2)
    {
        float dx = pos1.x - pos2.x;
        float dz = pos1.z - pos2.z;
        return dx * dx + dz * dz > sqrMoveThreshold;
    }

    // 物を蹴る
    protected virtual Item KickItem(KickableItem kickedItem = null)
    {
        KickableItem item;

        if (IsMyself())
        {
            // 接してる物が無ければ終了
            if (otherCollider == null)
            {
                return null;
            }

            // 蹴れる物じゃなければ終了
            item = otherCollider.GetComponent<KickableItem>();
            if (item == null)
            {
                return null;
            }
        }
        else
        {
            if (kickedItem == null)
            {
                return null;
            }
            item = kickedItem;
        }

        item.Kicked(this, kickFactor);

        return item;
    }

    // 物を持つ
    public virtual CarryableItem HoldItem(CarryableItem carryiedItem = null)
    {
        CarryableItem item;

        if (IsMyself())
        {
            // 接してる物が無ければ終了
            if (otherCollider == null)
            {
                return null;
            }

            // 持てる物じゃなければ終了
            item = otherCollider.GetComponent<CarryableItem>();
            if (item == null)
            {
                return null;
            }
        }
        else
        {
            if (carryiedItem == null)
            {
                return null;
            }
            item = carryiedItem;
        }

        // プレイヤーの配下にする
        if (!item.Attach(this))
        {
            return null;
        }

        // 持ってる間イベントの初期化を待たせる
        isStopTimerResetEvent = true;

        return item;
    }

    // 物を捨てる
    public virtual void DropItem()
    {
        // 何も持ってなければ終了
        if (eventItem == null)
        {
            return;
        }

        // アイテムの親を元に戻す
        ((CarryableItem)eventItem).Dettach();

        // イベント初期化を開始
        isStopTimerResetEvent = false;
    }

    // 物を投げ捨てる
    protected virtual CarryableItem ThrowItem()
    {
        // 何も持ってなければ終了
        if (eventItem == null)
        {
            return null;
        }

        // アイテムの親を元に戻す
        ((CarryableItem)eventItem).Thrown(this);

        // イベント初期化を開始
        isStopTimerResetEvent = false;

        return (CarryableItem)eventItem;
    }

    public virtual PlayerEvent GetPlayerEvent()
    {
        return currentEvent;
    }

    // イベント初期化
    protected virtual void ResetPlayerEvent()
    {
        currentEvent = PlayerEvent.None;
        eventItem = null;
    }

    public virtual void SetClientId(string id)
    {
        if (string.IsNullOrEmpty(clientId))
        {
            clientId = id;
        }
    }

    public virtual string GetClientId()
    {
        return clientId;
    }

    public virtual void SetIsMyself(bool either)
    {
        isMyself = either;
    }

    public virtual bool IsMyself()
    {
        return isMyself;
    }

    public virtual Item GetEventItem()
    {
        return eventItem;
    }
}
