using UnityEngine;

public enum PlayerEvent : int
{
    None,
    Kicking,
    Carrying,
}

// プレイヤークラス
public abstract class Player : SynchronizedObject
{
    string clientId;

    protected bool isMyself;
    protected PlayerEvent currentEvent;
    protected float timerResetEvent;
    protected bool isStopTimerResetEvent;

    protected float sqrMoveThreshold = 0.01f;

    protected float moveFactor = 10f;
    protected float turnFactor = 128f;
    protected float kickFactor = 200f;

    protected Rigidbody rbody;
    protected Animator animator;
    protected Collider otherCollider;
    protected Item eventItem;

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
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);

        // 接してる物を解除
        otherCollider = null;
    }

    // 物を持つ
    protected virtual void HoldItem(CarryableItem carryingItem = null)
    {
        return;
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
        ((CarryableItem)eventItem).Dettach(this);

        isStopTimerResetEvent = false;
    }

    // 物を蹴る
    protected virtual void KickItem()
    {
        return;
    }

    // イベントをセット
    public virtual void SetPlayerEvent(PlayerEvent evt, Item targetItem)
    {
        return;
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
