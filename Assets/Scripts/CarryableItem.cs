using UnityEngine;

// 持ち運べるアイテムクラス
public class CarryableItem : KickableItem
{
    // 持たれたときの相対位置
    protected Vector3 attachOffset = new Vector3(0, 1f, 2f);

    // 捨てられたら戻る親元
    protected Transform originalParent;

    // 持たれてるか
    protected bool isAttaching;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        // 戻り先セット
        originalParent = transform.parent;
    }

    // 同期を元に戻す
    protected override void ResetSyncState()
    {
        if (syncState == SyncState.Bidirectional)
        {
            return;
        }

        if (!isAttaching && !isMoving && timerResetSyncState <= 0f)
        {
            syncState = SyncState.Bidirectional;
            timerResetSyncState = delayResetSyncState;

            return;
        }

        if (timerResetSyncState > 0)
        {
            timerResetSyncState -= Time.fixedDeltaTime;
        }
    }

    // プレイヤーに持たれる
    public void Attach(Player player)
    {
        isAttaching = true;

        // 物理動作を切る
        rbody.isKinematic = true;
 
        // プレイヤーの配下になる
        transform.SetParent(player.transform);

        // 位置調整
        transform.localPosition = attachOffset;
    }

    // プレイヤーから捨てられる
    public void Dettach(Player player)
    {
        // 元の親の配下に戻る
        transform.SetParent(originalParent);

        // 物理動作を戻す
        rbody.isKinematic = false;

        isAttaching = false;
    }

    // 持たれてたら何もしない
    protected override void OnTriggerEnter(Collider other)
    {
        if (isAttaching)
        {
            return;
        }

        base.OnTriggerEnter(other);
    }
}
