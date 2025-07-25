using UnityEngine;

// 蹴れるアイテムクラス
public class KickableItem : Item
{
    protected float angularDrag = 2f;
    Player kickedPlayer;
    int stopCheckCount;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        // 転がりやすさセット
        rbody.angularDrag = angularDrag;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        // 蹴られて停止した
        if (kickedPlayer != null && !isMoving)
        {
            // 蹴られた直後に判定するのを避ける
            if (stopCheckCount < 10)
            {
                stopCheckCount++;
            }
            else
            {
                kickedPlayer = null;
            }
        }
    }

    // 蹴られる
    public virtual void Kicked(Player player, float kickFactor)
    {
        if (player == null)
        {
            return;
        }

        if (player.IsMyself())
        {
            SetSyncState(SyncState.SendOnly);
            rbody.AddForce(player.transform.forward * kickFactor, ForceMode.Impulse);

            // 誰に蹴られてるか保存しておく
            kickedPlayer = player;
            stopCheckCount = 0;
        }
        else
        {
            SetSyncState(SyncState.ReceiveOnly);
        }
    }

    public virtual Player GetKickedPlayer()
    {
        return kickedPlayer;
    }
}
