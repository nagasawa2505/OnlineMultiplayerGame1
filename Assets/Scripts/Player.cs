using UnityEngine;

public enum PlayerEvent
{
    None,
    Kicking
}

// プレイヤークラス
public class Player : SynchronizedObject
{
    string clientId;
    bool isMyself;

    float timerConnectionTimeout;
    float timerResetHasItem;

    PlayerEvent currentEvent;
    float timerResetEvent;

    float sqrMoveThreshold = 0.01f;

    float moveFactor = 10f;
    float turnFactor = 128f;
    float kickFactor = 200f;

    bool isInteractive;
    bool isKick;
    bool hasItem;

    Rigidbody rbody;
    Collider otherCollider;
    CarryableItem carryingItem;
    Animator animator;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        rbody = GetComponent<Rigidbody>();
        if (!isMyself)
        {
            rbody.isKinematic = true;
        }

        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        // なんか持ってるフラグ初期化チェック
        if (hasItem)
        {
            if (timerResetHasItem > 0)
            {
                timerResetHasItem -= Time.deltaTime;
            }
            else
            {
                ResetHasItem();
            }
        }

        // キック中フラグ初期化チェック
        if (currentEvent != PlayerEvent.None)
        {
            if (timerResetEvent > 0)
            {
                timerResetEvent -= Time.deltaTime;
            }
            else
            {
                ResetEvent();
            }
        }

        // 通信切断チェック
        if (timerConnectionTimeout > 0)
        {
            timerConnectionTimeout -= Time.deltaTime;
        }
        else
        {
            TimeoutConnection();
        }

        // この端末のプレイヤー
        if (isMyself)
        {
            // 入力受付
            SetInput();
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        // この端末のプレイヤー
        if (isMyself)
        {
            // なんかするとき
            if (isInteractive)
            {
                if (hasItem)
                {
                    // 捨てる
                    DropItem();
                }
                else
                {
                    // 持つ
                    HoldItem();
                }
            }
            isInteractive = false;

            // なんか蹴るとき
            if (isKick)
            {
                Kick();
                currentEvent = PlayerEvent.Kicking;

            }
            isKick = false;

            // アニメーション更新
            animator.SetBool("isMoving", rbody.velocity.sqrMagnitude > sqrMoveThreshold);
            animator.SetBool("isKicking", currentEvent == PlayerEvent.Kicking);
        }
        else
        {
            // 受信内容を反映
            transform.position = Vector3.Lerp(transform.position, receivedPosition, 0.25f);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, receivedRotation, 0.25f);

            // アニメーション更新
            animator.SetBool("isMoving", IsMoving(transform.position, receivedPosition));
            animator.SetBool("isKicking", currentEvent == PlayerEvent.Kicking);
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        if (other == otherCollider)
        {
            return;
        }

        // 接してる物をセット
        otherCollider = other;
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);

        // 接してる物を解除
        otherCollider = null;
    }

    // プレイヤー操作
    void SetInput()
    {
        float axisH = Input.GetAxis("Horizontal");
        transform.Rotate(0, axisH * turnFactor * Time.deltaTime, 0);

        float axisV = Input.GetAxis("Vertical");
        float moveZ = axisV > 0f ? axisV * moveFactor : 0;

        Vector3 moveGlobal = transform.TransformDirection(new Vector3(0, rbody.velocity.y, moveZ));
        rbody.velocity = moveGlobal;

        // なんかアクション入力 
        if (Input.GetKeyDown(KeyCode.E))
        {
            isInteractive = true;
        }

        // キック入力
        if (currentEvent != PlayerEvent.Kicking && Input.GetKeyDown(KeyCode.Space))
        {
            isKick = true;
            timerResetEvent = 0.5f;
        }
    }

    // 動いてるかを返す
    public bool IsMoving(Vector3 pos1, Vector3 pos2)
    {
        float dx = pos1.x - pos2.x;
        float dz = pos1.z - pos2.z;
        return dx * dx + dz * dz > sqrMoveThreshold;
    }

    // 持ってる物を返す
    public CarryableItem GetCarryingItem()
    {
        return carryingItem;
    }

    // 物を持つ
    public void HoldItem(CarryableItem received = null)
    {
        if (carryingItem != null)
        {
            return;
        }

        CarryableItem target;

        // 受信内容を反映する場合
        if (received != null)
        {
            target = received;
        }
        // 近くの物を持たせる場合
        else
        {
            // 接してる物が無ければ終了
            if (otherCollider == null)
            {
                return;
            }

            // 持てる物じゃなければ終了
            CarryableItem item = otherCollider.GetComponent<CarryableItem>();
            if (item == null)
            {
                return;
            }
            target = item;
        }

        // プレイヤーの配下にする
        target.Attach(this);

        // 持ち物セット
        carryingItem = target;
        hasItem = true;
    }

    // 物を捨てる
    public void DropItem()
    {
        // 何も持ってなければ終了
        if (carryingItem == null)
        {
            return;
        }

        // アイテムの親を元に戻す
        carryingItem.Dettach(this);

        // 持ち物削除
        carryingItem = null;

        timerResetHasItem = 1f;
    }

    // なんか持ってるフラグ初期化
    void ResetHasItem()
    {
        if (carryingItem != null)
        {
            return;
        }

        if (!hasItem)
        {
            return;
        }

        if (timerResetHasItem <= 0)
        {
            hasItem = false;
        }
    }

    protected void Kick()
    {
        if (otherCollider == null)
        {
            return;
        }

        KickableItem item = otherCollider.GetComponent<KickableItem>();
        if (item == null)
        {
            return;
        }

        if (item.IsKicked())
        {
            return;
        }

        item.Kicked(this, kickFactor);
    }

    public void SetEvent(PlayerEvent evt)
    {
        switch (evt)
        {
            case PlayerEvent.None:
                break;
            case PlayerEvent.Kicking:
                timerResetEvent = 0.5f;
                break;
        }
        currentEvent = evt;
    }

    public PlayerEvent GetEvent()
    {
        return currentEvent;
    }

    // イベントフラグ初期化
    void ResetEvent()
    {
        currentEvent = PlayerEvent.None;
    }

    // 通信タイムアウト処理
    void TimeoutConnection()
    {
        if (IsMyself())
        {
            return;
        }

        MyDebug.Log($"client {clientId} timeout");
        PlayersController.RemovePlayer(this);
        Destroy(this.gameObject);
    }

    public void SetClientId(string id)
    {
        if (string.IsNullOrEmpty(clientId))
        {
            clientId = id;
        }
    }

    public string GetClientId()
    {
        return clientId;
    }

    public void SetIsMyself(bool either)
    {
        isMyself = either;
    }

    public bool IsMyself()
    {
        return isMyself;
    }

    public void SetExitTimer(float sec)
    {
        timerConnectionTimeout = sec;
    }

    public float GetExitTimer()
    {
        return timerConnectionTimeout;
    }

    public Collider GetOtherCollder()
    {
        return otherCollider;
    }
}
