using UnityEngine;

public class MobilePadButton: MonoBehaviour
{
    static bool isJump;
    static bool isInteractive;
    static bool isKick;

    // Start is called before the first frame update
    void Start()
    {
        if (!GameController.IsMobileDevice())
        {
            this.gameObject.SetActive(false);
        }
    }

    void FixedUpdate()
    {
        isJump = false;
        isInteractive = false;
        isKick = false;
    }

    public void Jump()
    {
        isJump = true;
    }

    public void Interactive()
    {
        isInteractive = true;
    }

    public void Kick()
    {
        isKick = true;
    }

    public static bool IsJump()
    {
        return isJump;
    }

    public static bool IsInteractive()
    {
        return isInteractive;
    }

    public static bool IsKick()
    {
        return isKick;
    }
}
