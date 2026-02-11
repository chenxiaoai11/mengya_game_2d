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
    public Image characterPortrait;

    // ���ֻ�Ч������
    public float typewriterSpeed = 0.05f; // ÿ���ֵ���ʾ������룩
    private Coroutine typewriterCoroutine; // ��ǰ���еĴ��ֻ�Э��
    private string currentFullDialogue; // ���������ĶԻ��ı�

    private void Awake()
    {
        instance = this;
    }

    // ֹͣ��ǰ���ֻ�Ч���������ı��л�/�ر�ʱ��
    private void StopTypewriter()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }
    }

    // �������ֻ�Ч��
    public void StartTypewriter(string _name, string _line, int _size)
    {
        // ֹͣ��һ�ֵĴ��ֻ�
        StopTypewriter();
        // ���ý�ɫ�����ı���������
        characterNameText.text = _name;
        dialogueLineText.fontSize = _size;
        dialogueLineText.text = ""; // ������ı�
        currentFullDialogue = _line; // ���������ı�
        // ����Э��������ʾ
        typewriterCoroutine = StartCoroutine(TypewriterCoroutine());
        // ��ʾ�Ի���
        ToggleDialogueBox(true);
    }

    public void StartTypewriter(string _name, string _line, int _size, Sprite _portrait)
    {
        StopTypewriter();
        characterNameText.text = _name;
        dialogueLineText.fontSize = _size;
        dialogueLineText.text = "";
        currentFullDialogue = _line;
        typewriterCoroutine = StartCoroutine(TypewriterCoroutine());
        if (characterPortrait != null)
        {
            if (_portrait != null)
            {
                characterPortrait.sprite = _portrait;
                characterPortrait.enabled = true;
                characterPortrait.preserveAspect = true;
            }
            else
            {
                characterPortrait.sprite = null;
                characterPortrait.enabled = false;
            }
        }
        ToggleDialogueBox(true);
    }

    // ���ֻ�Э�̣�������ʾ�ı�
    private IEnumerator TypewriterCoroutine()
    {
        for (int i = 0; i < currentFullDialogue.Length; i++)
        {
            // ����ƴ���ı�
            dialogueLineText.text = currentFullDialogue.Substring(0, i + 1);
            // �ȴ�������ɸ�����Ҫ�����ٶȣ�
            yield return new WaitForSeconds(typewriterSpeed);
        }
        // ������ɺ����Э������
        typewriterCoroutine = null;
    }

    // ǿ����ɴ��ֻ������簴�ո�ʱֱ����ʾȫ���ı���
    public void CompleteTypewriter()
    {
        StopTypewriter();
        dialogueLineText.text = currentFullDialogue;
    }

    public void ToggleDialogueBox(bool _isActive)
    {
        if (!_isActive)
        {
            // �رնԻ���ʱֹͣ���ֻ�
            StopTypewriter();
        }
        dialogueBox.SetActive(_isActive);
    }

    public void ToggleSpaceBar(bool _isActive)
    {
        spacebar.SetActive(_isActive);
    }

    // ����ԭ��SetupDialogue����ѡ��Ҳ����ɾ��������StartTypewriter��
    public void SetupDialogue(string _name, string _line, int _size)
    {
        StartTypewriter(_name, _line, _size);
    }
}
