using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraController : MonoBehaviour
{
    static CameraController self;

    Transform myPlayer;
    const float offsetFront = 5f;
    const float offsetTop = 2.5f;
    const float followSpeed = 7.5f;
    const float rotateSpeed = 7.5f;

    public bool isTitle;

    void Awake()
    {
        if (self != null && self != this)
        {
            Destroy(gameObject);
            return;
        }
        self = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "Title")
        {
            isTitle = true;
        }
    }

    void FixedUpdate()
    {
        if (isTitle)
        {
            transform.Rotate(Vector3.right, 2.5f * Time.fixedDeltaTime);
        }
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
                int teamNum = GameController.GetTeamNumber();
                transform.rotation = teamNum % 2 == 0 ? Quaternion.Euler(270, 180, 0) : Quaternion.Euler(270, 0, 0);
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
