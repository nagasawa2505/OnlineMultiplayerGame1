﻿using System.Runtime.CompilerServices;
using System.Text;
using TMPro;
using UnityEngine;

public class FixedLengthStringBuffer
{
    readonly int maxLength;
    readonly StringBuilder buffer = new StringBuilder();

    public FixedLengthStringBuffer(int maxLength)
    {
        this.maxLength = maxLength;
    }

    public void Append(string text)
    {
        buffer.Append(text);

        // 超過した分を削除
        if (buffer.Length > maxLength)
        {
            int overflow = buffer.Length - maxLength;
            buffer.Remove(0, overflow);
        }
    }

    public void Clear()
    {
        buffer.Clear();
    }

    public override string ToString()
    {
        return buffer.ToString();
    }
}

public class MyDebug : MonoBehaviour
{
    static TextMeshProUGUI[] debugTexts;
    static FixedLengthStringBuffer[] bufs;
    static int bufSize = 200;

    // Start is called before the first frame update
    void Start()
#if UNITY_EDITOR || DEBUG
    {
        debugTexts = GetComponentsInChildren<TextMeshProUGUI>();

        bufs = new FixedLengthStringBuffer[debugTexts.Length];
        for (int i = 0; i < debugTexts.Length; i++)
        {
            bufs[i] = new FixedLengthStringBuffer(bufSize);
        }
    }
#else
    {
        return;
    }
#endif

    public static void SetText(int index, string content, bool isClear = false)
#if UNITY_EDITOR || DEBUG
    {

        if (isClear)
        {
            bufs[index].Clear();
        }
        bufs[index].Append(content);
        debugTexts[index].text = bufs[index].ToString();
    }
#else
    {
        return;
    }
#endif

    public static string Log(string message,
        [CallerLineNumber] int lineNumber = 0,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = "")
#if UNITY_EDITOR || DEBUG
    {
        string content = $"[{System.IO.Path.GetFileName(filePath)}:{lineNumber}] {memberName}() - {message}";
        Debug.Log(content);

        return content;
    }
#else
    {
        return "";
    }
#endif
}
