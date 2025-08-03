using UnityEngine;

// 操作対象以外のプレイヤークラス
public class OtherPlayer : Player
{
    const float TimeoutSecExit = 10f;
    float exitTimer;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        SetIsMyself(false);

        rbody.isKinematic = true;

        // 通信切断タイマーセット
        ResetExitTimer();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

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
        animator.SetBool("isKicking", currentEvent == PlayerEvent.Kicking);
        animator.SetBool("isThrowing", currentEvent == PlayerEvent.Throwing);
    }

    // 他端末から受信したイベントを反映
    public virtual void SetReceivedPlayerEvent(PlayerEvent evt, Item targetItem)
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
                    KickItem((KickableItem)targetItem);
                }
                break;

            case PlayerEvent.Carrying:
                timerResetEvent = 0.25f;
                eventItem = targetItem;
                if (currentEvent != PlayerEvent.Carrying)
                {
                    HoldItem((CarryableItem)targetItem);  
                }
                break;

            case PlayerEvent.Throwing:
                timerResetEvent = 1f;
                eventItem = targetItem;
                if (currentEvent == PlayerEvent.Carrying)
                {
                    ThrowItem();
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
        Debug.Log($"client {GetClientId()} timeout");
        PlayersController.RemovePlayer(this);
        Destroy(this.gameObject);
    }
}
