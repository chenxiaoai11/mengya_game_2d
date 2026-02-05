using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject dialogueBox;
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI dialogueLineText;
    public GameObject spacebar;

    // 打字机效果参数
    public float typewriterSpeed = 0.05f; // 每个字的显示间隔（秒）
    private Coroutine typewriterCoroutine; // 当前运行的打字机协程
    private string currentFullDialogue; // 缓存完整的对话文本

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            if (instance != this)
            {
                Destroy(gameObject);
            }
        }
        DontDestroyOnLoad(gameObject);
    }

    // 停止当前打字机效果（用于文本切换/关闭时）
    private void StopTypewriter()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }
    }

    // 启动打字机效果
    public void StartTypewriter(string _name, string _line, int _size)
    {
        // 停止上一轮的打字机
        StopTypewriter();
        // 设置角色名和文本基础参数
        characterNameText.text = _name;
        dialogueLineText.fontSize = _size;
        dialogueLineText.text = ""; // 先清空文本
        currentFullDialogue = _line; // 缓存完整文本
        // 启动协程逐字显示
        typewriterCoroutine = StartCoroutine(TypewriterCoroutine());
        // 显示对话框
        ToggleDialogueBox(true);
    }

    // 打字机协程：逐字显示文本
    private IEnumerator TypewriterCoroutine()
    {
        for (int i = 0; i < currentFullDialogue.Length; i++)
        {
            // 逐字拼接文本
            dialogueLineText.text = currentFullDialogue.Substring(0, i + 1);
            // 等待间隔（可根据需要调整速度）
            yield return new WaitForSeconds(typewriterSpeed);
        }
        // 打字完成后清空协程引用
        typewriterCoroutine = null;
    }

    // 强制完成打字机（比如按空格时直接显示全部文本）
    public void CompleteTypewriter()
    {
        StopTypewriter();
        dialogueLineText.text = currentFullDialogue;
    }

    public void ToggleDialogueBox(bool _isActive)
    {
        if (!_isActive)
        {
            // 关闭对话框时停止打字机
            StopTypewriter();
        }
        dialogueBox.SetActive(_isActive);
    }

    public void ToggleSpaceBar(bool _isActive)
    {
        spacebar.SetActive(_isActive);
    }

    // 保留原有SetupDialogue（可选，也可以删除，改用StartTypewriter）
    public void SetupDialogue(string _name, string _line, int _size)
    {
        StartTypewriter(_name, _line, _size);
    }
}