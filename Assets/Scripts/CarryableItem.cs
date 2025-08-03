using UnityEngine;

// 持ち運べるアイテムクラス
public class CarryableItem : KickableItem
{
    // 持たれたときの相対位置
    protected Vector3 attachOffset = new Vector3(0, 1.5f, 1f);

    // 捨てられたら戻る親元
    protected Transform originalParent;

    // 持ってるプレイヤー
    [SerializeField]
    protected Player carryingPlayer;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        // 戻り先セット
        originalParent = transform.parent;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (carryingPlayer != null)
        {
            return;
        }

        base.OnTriggerEnter(other);
    }

    // 蹴られる
    public override void Kicked(Player player, float kickFactor)
    {
        if (carryingPlayer != null)
        {
            return;
        }

        base.Kicked(player, kickFactor);
    }

    // プレイヤーに持たれる
    public bool Attach(Player player)
    {
        if (carryingPlayer != null)
        {
            return false;
        }

        if (player.IsMyself())
        {
            SetSyncState(SyncState.SendOnly);
        }
        else
        {
            SetSyncState(SyncState.ReceiveOnly);
        }

        // 持ち主セット
        carryingPlayer = player;

        // 移動時に干渉するのでコライダーを切る
        GetComponent<SphereCollider>().enabled = false;

        // 物理動作を切る
        rbody.isKinematic = true;

        // プレイヤーの配下になる
        transform.SetParent(player.transform);

        // 位置調整
        transform.localPosition = attachOffset;

        return true;
    }

    // プレイヤーから捨てられる
    public void Dettach()
    {
        // 元の親の配下に戻る
        transform.SetParent(originalParent);

        // 物理動作を戻す
        rbody.isKinematic = false;

        // コライダーを戻す
        GetComponent<SphereCollider>().enabled = true;

        // 持ち主解除
        carryingPlayer = null;
    }

    // プレイヤーから投げ捨てられる
    public void Thrown(Player player)
    {
        if (player == null)
        {
            return;
        }

        if (player.IsMyself())
        {
            if (carryingPlayer == null)
            {
                return;
            }

            SetSyncState(SyncState.SendOnly);

            rbody.AddForce(player.transform.forward * 5f + Vector3.up * 7.5f, ForceMode.VelocityChange);
        }
        else
        {
            SetSyncState(SyncState.ReceiveOnly);
        }

        Invoke("Dettach", 1f);
    }
}
