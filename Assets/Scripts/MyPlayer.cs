using UnityEngine;

// 操作対象のプレイヤークラス
public class MyPlayer : Player
{
    bool isInteractive;
    bool isKick;
    bool isAnimeKicking;
    int kickAnimeCount;

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        // 蹴るアニメ終了チェック
        if (isAnimeKicking)
        {
            if (kickAnimeCount < 40)
            {
                kickAnimeCount++;
            }
            else
            {
                isAnimeKicking = false;
            }
        }

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
            // 蹴る
            KickItem();

            // 蹴るアニメ開始
            isAnimeKicking = true;
            kickAnimeCount = 0;
        }
        isKick = false;

        // 蹴った物情報の送信を止める
        if (isStopTimerResetEvent && currentEvent == PlayerEvent.Kicking)
        {
            Player player = ((KickableItem)eventItem).GetKickedPlayer();
            {
                if (player == null || player != this)
                {
                    isStopTimerResetEvent = false;
                }
            }
        }

        // アニメーション更新
        animator.SetBool("isMoving", rbody.velocity.sqrMagnitude > sqrMoveThreshold);
        animator.SetBool("isKicking", isAnimeKicking);
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
        float axisH = Input.GetAxis("Horizontal");
        transform.Rotate(0, axisH * turnFactor * Time.deltaTime, 0);

        float axisV = Input.GetAxis("Vertical");
        float moveZ = axisV > 0f ? axisV * moveFactor : 0;

        Vector3 moveGlobal = transform.TransformDirection(new Vector3(0, rbody.velocity.y, moveZ));
        rbody.velocity = moveGlobal;

        // なんかアクション入力 
        if (Input.GetKeyDown(KeyCode.E))
        {
            isInteractive = true;
        }

        // キック入力
        if (currentEvent != PlayerEvent.Kicking && Input.GetKeyDown(KeyCode.Space))
        {
            isKick = true;
        }
    }

    // 物を持つ
    protected override void HoldItem(CarryableItem carryingItem = null)
    {
        base.HoldItem(carryingItem);

        // 接してる物が無ければ終了
        if (otherCollider == null)
        {
            return;
        }

        // 持てる物じゃなければ終了
        CarryableItem item = otherCollider.GetComponent<CarryableItem>();
        if (item == null)
        {
            return;
        }

        // プレイヤーの配下にする
        item.Attach(this);

        isStopTimerResetEvent = true;

        // イベント更新
        SetPlayerEvent(PlayerEvent.Carrying, item);
    }

    // 物を蹴る
    protected override void KickItem()
    {
        base.KickItem();

        if (otherCollider == null)
        {
            return;
        }

        KickableItem item = otherCollider.GetComponent<KickableItem>();
        if (item == null)
        {
            return;
        }

        item.Kicked(this, kickFactor);

        isStopTimerResetEvent = true;

        // イベント更新
        SetPlayerEvent(PlayerEvent.Kicking, item);
    }

    // 行動に対応するイベントをセット
    public override void SetPlayerEvent(PlayerEvent evt, Item item)
    {
        base.SetPlayerEvent(evt, item);

        switch (evt)
        {
            case PlayerEvent.None:
                break;

            case PlayerEvent.Kicking:
                timerResetEvent = 0.5f;
                eventItem = item;
                break;

            case PlayerEvent.Carrying:
                timerResetEvent = 0.5f;
                eventItem = item;
                break;
        }
        currentEvent = evt;
    }
}
