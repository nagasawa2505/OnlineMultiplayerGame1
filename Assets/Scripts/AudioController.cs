using UnityEngine;

// 音声管理クラス
public partial class AudioController : MonoBehaviour
{
    static AudioController self;

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
}
