using UnityEngine;

// 操作対象のプレイヤークラス
public class MyPlayer : Player
{
    bool isInteractive;
    bool isKick;
    bool isMove;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        SetIsMyself(true);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (isInteractive)
        {
            if (currentEvent == PlayerEvent.Carrying)
            {
                // 捨てる
                DropItem();
            }
            else
            {
                // 持つ
                HoldItem();
            }
        }
        isInteractive = false;

        if (isKick)
        {
            if (currentEvent != PlayerEvent.Carrying)
            {
                // 蹴る
                KickItem();
            }
        }
        isKick = false;

        // アニメーション更新
        animator.SetBool("isMoving", isMove);
        animator.SetBool("isKicking", currentEvent == PlayerEvent.Kicking);
        animator.SetBool("isThrowing", currentEvent == PlayerEvent.Throwing);
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        // 入力受付
        GetInput();
    }

    // プレイヤー操作
    void GetInput()
    {
        isMove = false;

        float axisH = Input.GetAxis("Horizontal");
        transform.Rotate(0, axisH * turnFactor * Time.deltaTime, 0);

        float axisV = Input.GetAxis("Vertical");
        float moveZ = axisV > 0f ? axisV * moveFactor : 0;

        if (moveZ != 0)
        {
            isMove = true;

            switch (currentEvent)
            {
                case PlayerEvent.Carrying:
                    moveZ *= 0.5f;
                    break;

                case PlayerEvent.Kicking:
                    moveZ *= 0.5f;
                    break;

                case PlayerEvent.Throwing:
                    moveZ *= 0.25f;
                    break;
            }
        }

        Vector3 moveGlobal = transform.TransformDirection(new Vector3(0, rbody.velocity.y, moveZ));
        rbody.velocity = moveGlobal;

        // なんかアクション入力 
        if (Input.GetKeyDown(KeyCode.E))
        {
            isInteractive = true;
        }

        // キック入力
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isKick = true;
        }
    }

    // 物を蹴る
    protected override Item KickItem(KickableItem kickedItem = null)
    {
        Item item = base.KickItem();

        // イベント更新
        SetSendPlayerEvent(PlayerEvent.Kicking, item);

        return item;
    }

    // 物を持つ
    public override CarryableItem HoldItem(CarryableItem carryiedItem = null)
    {
        CarryableItem item = base.HoldItem(carryiedItem);
        if (item != null)
        {
            // イベント更新
            SetSendPlayerEvent(PlayerEvent.Carrying, item);
        }

        return item;
    }

    // 物を投げ捨てる
    protected override CarryableItem ThrowItem()
    {
        CarryableItem item = base.ThrowItem();
        if (item != null)
        {
            // イベント更新
            SetSendPlayerEvent(PlayerEvent.Throwing, item);
        }

        return item;
    }

    // 他端末に送信するイベントをセット
    public virtual void SetSendPlayerEvent(PlayerEvent evt, Item item)
    {
        switch (evt)
        {
            case PlayerEvent.None:
                break;

            case PlayerEvent.Kicking:
                timerResetEvent = 0.5f;
                eventItem = item;
                break;

            case PlayerEvent.Carrying:
                timerResetEvent = 0.25f;
                eventItem = item;
                break;

            case PlayerEvent.Throwing:
                timerResetEvent = 0.75f;
                eventItem = item;
                break;
        }
        currentEvent = evt;
    }
}
