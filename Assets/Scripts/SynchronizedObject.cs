using UnityEngine;

// 同期状態
public enum SyncState
{
    Bidirectional, // 送受信する
    SendOnly,      // 送信のみ
    ReceiveOnly,   // 受信のみ
    None,          // 送受信しない
}

// 通信で同期されるものクラス
public abstract class SynchronizedObject : MonoBehaviour
{
    protected SyncState syncState;

    protected float positionThreshold = 0.05f;
    protected float rotationThreshold = 0.5f;

    protected Vector3 lastSentPosition;
    protected Quaternion lastSentRotation;

    protected Vector3 receivedPosition;
    protected Quaternion receivedRotation;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        return;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        return;
    }

    protected virtual void FixedUpdate()
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

    public virtual void SetReceivedRotation(Vector4 rotation)
    {
        receivedRotation = new Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);
    }
}
