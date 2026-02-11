using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �����������ӵ�UI���ƣ����𱳾���ʾ����Ʒͼ��ˢ��/���
/// </summary>
public class BackpackSlot : MonoBehaviour
{
    [Header("UI�������")]
    [Tooltip("���ӱ���Image���")]
    public Image slotBackground;
    [Tooltip("��Ʒͼ��Image����������ڸ����ӽڵ㣩")]
    public Image itemIcon;
    public float padding = 8f;

    void Awake()
    {
        // ǿ�Ƽ������壬��ֹ�߼�ʧЧ
        gameObject.SetActive(true);
        // �Զ�����δ��ֵ��UI���
        AutoFindComponents();
        // ��ʼ������Ĭ��״̬
        ResetSlot();
    }

    /// <summary>
    /// �Զ�����UI����������ֶ���ֵ��©��
    /// </summary>
    private void AutoFindComponents()
    {
        // ���ұ������
        if (slotBackground == null)
        {
            slotBackground = transform.Find("Background")?.GetComponent<Image>();
            slotBackground ??= GetComponent<Image>(); // ���ף�������Ϊ����
            Debug.Log($"{gameObject.name} �����������{(slotBackground != null ? "�ɹ�" : "ʧ��")}");
        }

        // ����ͼ�����
        if (itemIcon == null)
        {
            itemIcon = transform.Find("Icon")?.GetComponent<Image>();
            itemIcon ??= GetComponentInChildren<Image>(); // ���ף��ӽڵ��һ��Image
            Debug.Log($"{gameObject.name} ͼ���������{(itemIcon != null ? "�ɹ�" : "ʧ��")}");
        }
    }

    /// <summary>
    /// ���ø��ӵ�Ĭ��״̬����ʾ���������ͼ�꣩
    /// </summary>
    public void ResetSlot()
    {
        // ��ʾ������������ɫ
        if (slotBackground != null)
        {
            slotBackground.enabled = true;
            slotBackground.color = Color.white;
        }

        // ���ͼ��
        if (itemIcon != null)
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
            itemIcon.color = Color.white;
        }
    }

    /// <summary>
    /// ���¸�����ʾ����Ʒͼ��
    /// </summary>
    /// <param name="icon">��Ʒͼ�꣨null����գ�</param>
    public void UpdateSlot(Sprite icon)
    {
        // �����÷���
        if (this == null || gameObject == null || slotBackground == null || itemIcon == null)
        {
            Debug.LogWarning($"{gameObject.name} ������������޷�����ͼ��");
            return;
        }

        // ǿ����ʾ����
        slotBackground.enabled = true;

        // ����ͼ��
        if (icon != null)
        {
            itemIcon.sprite = icon;
            itemIcon.enabled = true;
            itemIcon.preserveAspect = true;
            var slotRt = slotBackground.rectTransform;
            var iconRt = itemIcon.rectTransform;
            iconRt.anchorMin = new Vector2(0.5f, 0.5f);
            iconRt.anchorMax = new Vector2(0.5f, 0.5f);
            iconRt.pivot = new Vector2(0.5f, 0.5f);
            iconRt.anchoredPosition = Vector2.zero;
            itemIcon.SetNativeSize();
            var slotSize = slotRt.rect.size;
            var targetW = Mathf.Max(0f, slotSize.x - padding);
            var targetH = Mathf.Max(0f, slotSize.y - padding);
            var scaleX = iconRt.sizeDelta.x > 0f ? targetW / iconRt.sizeDelta.x : 1f;
            var scaleY = iconRt.sizeDelta.y > 0f ? targetH / iconRt.sizeDelta.y : 1f;
            var s = Mathf.Min(scaleX, scaleY);
            iconRt.sizeDelta = iconRt.sizeDelta * s;
            Debug.Log($"{gameObject.name} �ɹ���ʾͼ�꣺{icon.name}");
        }
        else
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
            Debug.Log($"{gameObject.name} �����Ʒͼ��");
        }
    }

    /// <summary>
    /// ��ո��ӵ���Ʒͼ�꣨����������
    /// </summary>
    public void ClearSlot()
    {
        if (this == null || gameObject == null || itemIcon == null)
        {
            Debug.LogWarning($"{gameObject.name} �����Ч���޷����ͼ��");
            return;
        }

        itemIcon.sprite = null;
        itemIcon.enabled = false;
    }
}
