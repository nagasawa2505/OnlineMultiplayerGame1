using UnityEngine;

// 持ち運べるアイテムクラス
public class CarryableItem : Item
{
    // 持たれたときの相対位置
    Vector3 attachOffset = new Vector3(0, 1f, 2f);

    // 持たれてるか
    protected bool isAttaching;

    // 同期を元に戻すまでの時間
    protected float timerResetSyncState;

    // 捨てられたら戻る親元
    protected Transform originalParent;

    protected Rigidbody rbody;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        originalParent = transform.parent;

        rbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (timerResetSyncState > 0)
        {
            timerResetSyncState -= Time.deltaTime;
        }
        else
        {
            ResetSyncState();
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (isUpdated)
        {
            rbody.MovePosition(Vector3.Lerp(transform.position, receivedPosition, 0.25f));
            rbody.MoveRotation(transform.localRotation = Quaternion.Slerp(transform.localRotation, receivedRotation, 0.25f));
        }
        isUpdated = false;
    }

    // 同期を元に戻す
    protected virtual void ResetSyncState()
    {
        if (GetSyncState() == SyncState.Bidirectional)
        {
            return;
        }

        if (closePlayer != null)
        {
            return;
        }

        if (isAttaching)
        {
            return;
        }

        SetSyncState(SyncState.Bidirectional);
    }

    // 親をプレイヤーにする
    public void Attach(Player player)
    {
        isAttaching = true;

        // 持ってるやつが送信して他は受信
        if (player.IsMyself())
        {
            SetSyncState(SyncState.SendOnly);
        }
        else
        {
            SetSyncState(SyncState.ReceiveOnly);
        }

        // 物理動作を切る
        rbody.isKinematic = true;
 
        // プレイヤーの配下になる
        transform.SetParent(player.transform);

        // 位置調整
        transform.localPosition = attachOffset;
    }

    // 親を元に戻す
    public void Dettach(Player player)
    {
        // 元の親の配下に戻る
        transform.SetParent(originalParent);

        // 物理動作を戻す
        rbody.isKinematic = false;

        isAttaching = false;

        // 後で同期を元に戻す
        timerResetSyncState = 2f;
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

    // 同期はタイマーで管理する
    protected override void OnTriggerExit(Collider other)
    {
        closePlayer = null;
    }
}
