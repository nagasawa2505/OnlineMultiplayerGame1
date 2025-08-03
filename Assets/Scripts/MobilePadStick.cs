using UnityEngine;

public class MobilePadStick: MonoBehaviour
{
    static Vector2 axis;
    const float TabMoveLen = 75;
    Rect stickArea;
    Vector2 defaultPos;
    Vector2 tapPos;

    // Start is called before the first frame update
    void Start()
    {
        if (!GameController.IsMobileDevice())
        {
            this.gameObject.SetActive(false);
        }

        defaultPos = GetComponent<RectTransform>().localPosition;

        // 左下の入力のみ取得
        float areaLength = Screen.width / 2;
        stickArea = new Rect(0, 0, areaLength, areaLength);
    }

    // タップしたとき
    public void PadDown()
    {
        // マウスクリック=タップ
        tapPos = Input.mousePosition;

        if (!stickArea.Contains(tapPos))
        {
            tapPos = Vector2.zero;
        }
    }

    // 指を離したとき
    public void PadUp()
    {
        GetComponent<RectTransform>().localPosition = defaultPos;
        axis.x = 0;
        axis.y = 0;
    }

    // ドラッグしたとき
    public void PadDrag()
    {
        if (tapPos == Vector2.zero)
        {
            return;
        }

        // タップしてから移動した位置
        Vector2 mousePosition = Input.mousePosition;

        // ずらした距離を求める
        Vector2 newTabPos = mousePosition - tapPos;

        // 方向ベクトルを作成して正規化
        axis = newTabPos.normalized;

        //newTabPos.y = 0;

        // パッドの移動距離を制限
        float len = Vector2.Distance(defaultPos, newTabPos);
        if (len > TabMoveLen)
        {
            newTabPos.x = axis.x * TabMoveLen;
            newTabPos.y = axis.y * TabMoveLen;
        }

        // 移動
        GetComponent<RectTransform>().localPosition = newTabPos;
    }

    public static void SetMobileAxis(ref float axisH, ref float axisV)
    {
        axisH = axis.x;
        axisV = axis.y;
    }
}
