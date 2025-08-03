using UnityEngine;

// 同期状態
public enum SyncState
{
    ReceiveOnly,   // 受信のみ
    SendOnly,      // 送信のみ
}

// 通信で同期されるものクラス
public abstract class SynchronizedObject : MonoBehaviour
{
    [SerializeField]
    SyncState syncState;

    protected float positionThreshold = 0.01f;
    protected float rotationThreshold = 1f;

    protected Vector3 lastSentPosition;
    protected Quaternion lastSentRotation;

    protected Vector3 receivedPosition;
    protected Quaternion receivedRotation;

    protected virtual void Awake()
    {
        return;
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        return;
    }

    protected virtual void FixedUpdate()
    {
        return;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        return;
    }

    protected virtual void LateUpdate()
    {
        return;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        return;
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        return;
    }

    public virtual bool IsSentPositionChanged(ref Vector3 currentPosition)
    {
        return Vector3.Distance(currentPosition, lastSentPosition) > positionThreshold;
    }

    public virtual bool IsSentRotationChanged(ref Quaternion currentRotation)
    {
        return Quaternion.Angle(currentRotation, lastSentRotation) > rotationThreshold;
    }

    public virtual void SetLastSentPosition(Vector3 position)
    {
        lastSentPosition = position;
    }

    public virtual Vector3 GetSentLastPosition()
    {
        return lastSentPosition;
    }

    public virtual void SetLastSentRotation(Quaternion rotation)
    {
        lastSentRotation = rotation;
    }

    public virtual Quaternion GetSentLastRotation()
    {
        return lastSentRotation;
    }

    public virtual void SetReceivedPosition(Vector3 position)
    {
        receivedPosition = position;
    }

    public virtual void SetReceivedRotation(Quaternion rotation)
    {
        receivedRotation = rotation;
    }

    public virtual void SetSyncState(SyncState state)
    {
        syncState = state;
    }

    public virtual SyncState GetSyncState()
    {
        return syncState;
    }
}
