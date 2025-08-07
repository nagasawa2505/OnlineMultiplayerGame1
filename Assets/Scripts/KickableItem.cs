using UnityEngine;

// 蹴れるアイテムクラス
public class KickableItem : Item
{
    // 最後に蹴ったプレイヤー
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
        // プレイヤーに接触したら止まる
        rbody.drag = 0.25f;

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
            rbody.AddForce(player.transform.forward * kickFactor, ForceMode.Impulse);

            kickingPlayer = player;

            SetOwner(player);
            isFixedState = true;
            Invoke("UnsetIsFixedState", 1.5f);
        }
    }

    void UnsetIsFixedState()
    {
        isFixedState = false;
    }
}
