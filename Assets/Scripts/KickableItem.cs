using UnityEngine;

// 蹴れるアイテムクラス
public class KickableItem : Item
{
    // 蹴ってるプレイヤー
    protected Player kickingPlayer;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        // 転がりやすさセット
        rbody.angularDrag = 2f;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        // 減衰力を戻す
        rbody.drag = 0;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        // 蹴られた直後なら終了
        if (kickingPlayer != null)
        {
            return;
        }

        // プレイヤーに接触したら止まる
        rbody.drag = 2f;

        base.OnTriggerEnter(other);
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
            if (kickingPlayer != null)
            {
                return;
            }

            // 同期状態を変更
            SetSyncState(SyncState.SendOnly);

            rbody.AddForce(player.transform.forward * kickFactor, ForceMode.Impulse);

            Invoke("UnsetKickingPlayer", 0.05f);

            kickingPlayer = player;
        }
        else
        {
            // 同期状態を変更
            SetSyncState(SyncState.ReceiveOnly);
        }
    }

    protected virtual void UnsetKickingPlayer()
    {
        kickingPlayer = null;
    }
}
