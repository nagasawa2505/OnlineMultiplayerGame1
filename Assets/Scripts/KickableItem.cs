using UnityEngine;

// 蹴れるアイテムクラス
public class KickableItem : Item
{
    bool isKicked;
    public float angularDrag = 2f;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        // 同期状態を戻すタイミングを調整
        delayResetSyncState = 1f;

        // 転がりやすさセット
        rbody.angularDrag = angularDrag;
    }

    // 蹴られる
    public void Kicked(Player player, float kickFactor)
    {
        isKicked = true;

        if (player == null)
        {
            return;
        }

        rbody.AddForce(player.transform.forward * kickFactor, ForceMode.Impulse);

        isKicked = false;
    }

    // 蹴られてたら何もしない
    protected override void OnTriggerEnter(Collider other)
    {
        if (isKicked)
        {
            return;
        }

        base.OnTriggerEnter(other);
    }

    public bool IsKicked()
    {
        return isKicked;
    }
}
