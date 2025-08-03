using UnityEngine;

// 操作対象のプレイヤークラス
public class MyPlayer : Player
{
    const KeyCode keyJump = KeyCode.J;
    const KeyCode keyInteractive = KeyCode.H;
    const KeyCode keyKick = KeyCode.K;

    [SerializeField]
    bool isGrounded;

    bool isMobileDevice;

    bool isJump;
    bool isInteractive;
    bool isKick;
    bool isMove;

    float axisH;
    float axisV;

    Vector3 move;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        SetIsMyself(true);

        isMobileDevice = GameController.IsMobileDevice();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        // 移動
        rbody.velocity = move;

        // 旋回
        transform.Rotate(0, axisH * turnFactor * Time.fixedDeltaTime, 0);

        // 接地フラグ更新
        isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 1f);

        if (isJump)
        {
            float jumpVelocity = Mathf.Sqrt(2f * jumpFactor * Mathf.Abs(Physics.gravity.y));
            rbody.velocity = new Vector3(rbody.velocity.x, jumpVelocity, rbody.velocity.z);
        }
        isJump = false;

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

        if (isMobileDevice)
        {
            MobilePadStick.SetMobileAxis(ref axisH, ref axisV);
        }
        else
        {
            axisH = Input.GetAxis("Horizontal");
            axisV = Input.GetAxis("Vertical");
        }

        if (axisV != 0)
        {
            isMove = true;

            switch (currentEvent)
            {
                case PlayerEvent.Carrying:
                    axisV *= 0.75f;
                    break;

                case PlayerEvent.Kicking:
                    axisV *= 0.5f;
                    break;

                case PlayerEvent.Throwing:
                    axisV *= 0.25f;
                    break;
            }
        }

        if (isMobileDevice)
        {
            // 移動と旋回が一体になるので左右方向の移動速度を補完
            move = transform.TransformDirection(new Vector3(0, rbody.velocity.y, axisV * (Mathf.Abs(axisH) + 1f) * moveFactor));

            if (!isJump && MobilePadButton.IsJump() && isGrounded && currentEvent != PlayerEvent.Carrying)
            {
                isJump = true;
            }

            if (!isInteractive && MobilePadButton.IsInteractive())
            {
                isInteractive = true;
            }

            if (!isKick && MobilePadButton.IsKick())
            {
                isKick = true;
            }
        }
        else
        {
            // 移動と旋回は独立
            move = transform.TransformDirection(new Vector3(0, rbody.velocity.y, axisV * moveFactor));

            // ジャンプ
            if (!isJump && Input.GetKeyDown(keyJump) && isGrounded && currentEvent != PlayerEvent.Carrying)
            {
                isJump = true;
            }

            // なんかアクション入力 
            if (!isInteractive && Input.GetKeyDown(keyInteractive))
            {
                isInteractive = true;
            }

            // キック入力
            if (!isKick && Input.GetKeyDown(keyKick))
            {
                isKick = true;
            }
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
                timerResetEvent = 1f;
                eventItem = item;
                break;
        }
        currentEvent = evt;
    }
}
