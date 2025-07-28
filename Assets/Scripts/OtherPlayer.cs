using UnityEngine;

// 操作対象以外のプレイヤークラス
public class OtherPlayer : Player
{
    const float TimeoutSecExit = 10f;
    float exitTimer;

    protected override void Awake()
    {
        base.Awake();

        SetIsMyself(false);
    }

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

    // 動いてるかを返す
    public bool IsMoving(Vector3 pos1, Vector3 pos2)
    {
        float dx = pos1.x - pos2.x;
        float dz = pos1.z - pos2.z;
        return dx * dx + dz * dz > sqrMoveThreshold;
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
                    KickItem();
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

            case PlayerEvent.Throwing:
                timerResetEvent = 0.75f;
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
        MyDebug.Log($"client {GetClientId()} timeout");
        PlayersController.RemovePlayer(this);
        Destroy(this.gameObject);
    }
}
