using UnityEngine;

// 操作対象以外のプレイヤークラス
public class OtherPlayer : Player
{
    float exitTimer;
    const float TimeoutSecExit = 10f;
    bool isAnimeKicking;
    int kickAnimeCount;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        // 通信切断タイマーセット
        ResetExitTimer();
    }

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

        // 通信切断チェック
        if (exitTimer > 0)
        {
            exitTimer -= Time.fixedDeltaTime;
        }
        else
        {
            ExitRoom();
        }

        // 受信内容を反映
        transform.position = Vector3.Lerp(transform.position, receivedPosition, 0.25f);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, receivedRotation, 0.25f);

        // アニメーション更新
        animator.SetBool("isMoving", IsMoving(transform.position, receivedPosition));
        animator.SetBool("isKicking", isAnimeKicking);
    }

    // 動いてるかを返す
    public bool IsMoving(Vector3 pos1, Vector3 pos2)
    {
        float dx = pos1.x - pos2.x;
        float dz = pos1.z - pos2.z;
        return dx * dx + dz * dz > sqrMoveThreshold;
    }

    // 物を持つ
    protected override void HoldItem(CarryableItem carryingItem = null)
    {
        base.HoldItem(carryingItem);

        if (carryingItem == null)
        {
            return;
        }

        isStopTimerResetEvent = true;

        // プレイヤーの配下にする
        carryingItem.Attach(this);
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
    }

    // イベントをセットして行動させる
    public override void SetPlayerEvent(PlayerEvent evt, Item targetItem)
    {
        switch (evt)
        {
            case PlayerEvent.None:
                if (currentEvent == PlayerEvent.Carrying)
                {
                    DropItem();
                }
                break;

            case PlayerEvent.Kicking:
                timerResetEvent = 0.5f;
                eventItem = targetItem;
                if (currentEvent != PlayerEvent.Kicking)
                {
                    KickItem();

                    // 蹴るアニメ開始
                    isAnimeKicking = true;
                    kickAnimeCount = 0;
                }
                break;

            case PlayerEvent.Carrying:
                timerResetEvent = 0.5f;
                eventItem = targetItem;
                if (currentEvent != PlayerEvent.Carrying)
                {
                    HoldItem((CarryableItem)targetItem);  
                }
                break;
        }
        currentEvent = evt;
    }

    public virtual void ResetExitTimer()
    {
        exitTimer = TimeoutSecExit;
    }

    // 通信タイムアウト処理
    protected virtual void ExitRoom()
    {
        MyDebug.Log($"client {GetClientId()} timeout");
        PlayersController.RemovePlayer(this);
        Destroy(this.gameObject);
    }
}
