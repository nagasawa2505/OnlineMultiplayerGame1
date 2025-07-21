using UnityEngine;

// プレイヤークラス
public class Player : SynchronizedObject
{
    string clientId;
    bool isMyself;

    const float TimeoutSecReset = 1f;
    float timerResetHasItem;
    float timerConnectionTimeout;

    Vector3 move;
    float moveFactor = 3f;
    float jumpFactor = 2f;

    bool isInteractive;
    bool isJumping;
    bool hasItem;

    Rigidbody rbody;
    Collider otherCollider;
    CarryableItem carryingItem;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        rbody = GetComponent<Rigidbody>();
        if (!isMyself)
        {
            rbody.isKinematic = true;
        }
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        // この端末のプレイヤー
        if (isMyself)
        {
            // 入力受付
            SetInput();
        }

        // なんか持ってるフラグ初期化チェック
        if (timerResetHasItem > 0)
        {
            timerResetHasItem -= Time.deltaTime;
        }
        else
        {
            ResetHasItem();
        }

        // 通信切断チェック
        if (timerConnectionTimeout > 0)
        {
            timerConnectionTimeout -= Time.deltaTime;
        }
        else
        {
            Timeout();
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        // この端末のプレイヤー
        if (isMyself)
        {
            // ジャンプ状態を反映
            if (isJumping)
            {
                float jumpVelocity = Mathf.Sqrt(2f * jumpFactor * Mathf.Abs(Physics.gravity.y));
                rbody.velocity = new Vector3(rbody.velocity.x, jumpVelocity, rbody.velocity.z);
                isJumping = false;
            }

            // 移動情報を反映
            if (move != Vector3.zero)
            {
                Vector3 velocity = move * moveFactor;
                rbody.velocity = new Vector3(velocity.x, rbody.velocity.y, velocity.z);
                transform.rotation = Quaternion.LookRotation(move);
            }

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
                isInteractive = false;
            }
        }
        else
        {
            // 受信内容を反映
            transform.position = Vector3.Lerp(transform.position, receivedPosition, 0.25f);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, receivedRotation, 0.25f);
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
        // 入力を取得して移動情報をセット
        float axisH = Input.GetAxisRaw("Horizontal");
        float axisV = Input.GetAxisRaw("Vertical");
        move = new Vector3(axisH, 0, axisV).normalized;

        if (Input.GetKeyDown(KeyCode.E))
        {
            isInteractive = true;
        }

        // ジャンプ入力
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isJumping = true;
        }
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

        timerResetHasItem = TimeoutSecReset;
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

    // 通信タイムアウト処理
    void Timeout()
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
