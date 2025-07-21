using UnityEngine;

public class CameraController : MonoBehaviour
{
    Transform myPlayer;
    public float offsetFront = 5f;
    public float offsetTop = 2.5f;
    public float followSpeed = 10f;
    public float rotateSpeed = 10f;

    // Start is called before the first frame update
    protected void Start()
    {
        transform.position = new Vector3(0, 25, -50);
    }

    void LateUpdate()
    {
        // プレイヤー取得前なら取得
        if (myPlayer == null)
        {
            Player player = PlayersController.GetMyPlayer();
            if (player != null)
            {
                myPlayer = player.transform;
            }
        }
        else
        {
            // カメラ移動
            Vector3 position = myPlayer.position - (myPlayer.forward * offsetFront) + new Vector3(0, offsetTop, 0);
            transform.position = Vector3.Lerp(transform.position, position, followSpeed * Time.deltaTime);

            // カメラ回転
            Quaternion rotation = Quaternion.LookRotation(myPlayer.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotateSpeed);
        }
    }
}
