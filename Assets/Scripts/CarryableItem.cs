using UnityEngine;

// 持ち運べるアイテムクラス
public class CarryableItem : KickableItem
{
    // 持ってるプレイヤー
    [SerializeField]
    protected Player carryingPlayer;

    // 捨てられたら戻る親元
    protected Transform originalParent;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        // 戻り先セット
        originalParent = transform.parent;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (isFixedState && carryingPlayer != null)
        {
            // 位置調整
            transform.position = carryingPlayer.transform.position + carryingPlayer.transform.forward * 1f + carryingPlayer.transform.up * 1.5f;
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (isFixedState || carryingPlayer != null)
        {
            return;
        }

        base.OnTriggerEnter(other);
    }

    protected override void Die()
    {
        if (isFixedState || carryingPlayer != null)
        {
            carryingPlayer.DropItem();
        }

        base.Die();
    }

    // 蹴られる
    public override void Kicked(Player player, float kickFactor)
    {
        if (isFixedState || carryingPlayer != null)
        {
            return;
        }

        base.Kicked(player, kickFactor);
    }

    // プレイヤーに持たれる
    public bool Attach(Player player)
    {
        if (isFixedState || carryingPlayer != null)
        {
            return false;
        }

        // 持ち主セット
        carryingPlayer = player;

        // 移動時に干渉するのでコライダーを切る
        GetComponent<SphereCollider>().enabled = false;

        // 物理動作を切る
        rbody.isKinematic = true;

        // プレイヤーの配下になる
        transform.SetParent(player.transform);

        if (player.IsMyself())
        {
            SetOwner(player);
            isFixedState = true;
        }

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

        isFixedState = false;
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

            carryingPlayer = null;
            rbody.isKinematic = false;
            rbody.AddForce(player.transform.forward * 5f + Vector3.up * 7.5f, ForceMode.VelocityChange);   
        }
        Invoke("Dettach", 1f);
    }
}
