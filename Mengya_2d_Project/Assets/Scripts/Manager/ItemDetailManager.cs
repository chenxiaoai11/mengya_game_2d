using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ItemDetailManager : MonoBehaviour
{
    // 1. �ֶ���ֵ�ֶΣ�����֮ǰһ������ק��ǰ�����������
    public PlayerMovement playerMovement;
    public GameObject itemDetailPanel;
    public Image itemShowImage;
    public TMP_Text detailItemName;
    public TMP_Text detailItemDesc;

    // 2. �����ڵ�����ֻ��֤��ǰ������һ��ʵ�������糡����
    public static ItemDetailManager Instance;
    private ItemData currentSelectedItem;
    public Button takeButton;
    public GameObject clickBlocker;
    public string clickBlockerName = "ItemDetailClickBlocker";
    public bool IsPanelOpen()
    {
        return itemDetailPanel != null && itemDetailPanel.activeInHierarchy;
    }

    void Awake()
    {
        // �����ڵ�������ǰ�����������ʵ�����������µ�
        if (Instance == null)
        {
            Instance = this;
            // �Ƴ� DontDestroyOnLoad���������糡��������ֻ�ڵ�ǰ������Ч
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 3. ��ʾ������壨�߼����䣬���ֶ���ֵ�������
    public void ShowDetailPanel(ItemData itemData)
    {
        Debug.Log($"=== ��ʾ��������� ===");
        Debug.Log($"itemData�Ƿ�Ϊ��: {itemData == null}");
        Debug.Log($"itemDetailPanel�Ƿ�Ϊ��: {itemDetailPanel == null}");
        Debug.Log($"playerMovement�Ƿ�Ϊ��: {playerMovement == null}");

        if (itemData == null || itemDetailPanel == null || playerMovement == null)
        {
            Debug.LogWarning("��Ʒ/���/PlayerΪ�գ��޷���ʾ���飡");
            return;
        }

        playerMovement.LockPlayerMovement();
        currentSelectedItem = itemData;
        UpdateDetailPanelContent(itemData);
        EnsureClickBlocker();
        if (clickBlocker != null) clickBlocker.SetActive(true);
        if (itemDetailPanel != null)
        {
            var rt = itemDetailPanel.transform as RectTransform;
            var parent = itemDetailPanel.transform.parent;
            if (parent != null) itemDetailPanel.transform.SetAsLastSibling();
        }
        itemDetailPanel.SetActive(true);
        if (takeButton != null)
        {
            takeButton.interactable = CanPickup(itemData);
        }
        Debug.Log($"��ʾ{itemData.itemName}�������");
    }

    // 4. ����������ݣ��߼����䣩
    private void UpdateDetailPanelContent(ItemData itemData)
    {
        if (itemShowImage != null)
        {
            itemShowImage.sprite = itemData.itemIcon;
            itemShowImage.gameObject.SetActive(itemData.itemIcon != null);
            itemShowImage.type = Image.Type.Simple;
            itemShowImage.preserveAspect = true;
        }
        if (detailItemName != null)
        {
            detailItemName.text = itemData.itemName;
        }
        if (detailItemDesc != null)
        {
            detailItemDesc.text = itemData.itemDescription;
        }
    }

    // 5. �����������߼����䣩
    public void HideDetailPanel()
    {
        if (itemDetailPanel != null)
        {
            itemDetailPanel.SetActive(false);
        }
        if (clickBlocker != null)
        {
            clickBlocker.SetActive(false);
        }
        if (playerMovement != null)
        {
            playerMovement.UnlockPlayerMovement();
        }
    }

    public void OnTakeButtonClick()
    {
        if (currentSelectedItem == null || BackpackManager.Instance == null)
        {
            Debug.LogWarning("�޷�ʰȡ��Ʒ��δѡ����Ʒ�򱳰���������Ч");
            HideDetailPanel();
            return;
        }

        if (!CanPickup(currentSelectedItem))
        {
            Debug.LogWarning("�����ڽ̳̹关卡，物品不可拾取");
            HideDetailPanel();
            return;
        }

        BackpackManager.Instance.AddItemToBackpack(currentSelectedItem);
        currentSelectedItem = null;
        HideDetailPanel();
    }

    public void OnCancelButtonClick()
    {
        Debug.Log("ȡ��ʰȡ���ر��������");
        HideDetailPanel();
    }

    private bool CanPickup(ItemData item)
    {
        if (item == null) return false;
        if (item.belongToLevel <= 0) return false;
        var sceneName = SceneManager.GetActiveScene().name ?? "";
        if (sceneName.Contains("Level0") || sceneName.Contains("Tutorial")) return false;
        return true;
    }

    private void EnsureClickBlocker()
    {
        if (clickBlocker != null) return;
        if (itemDetailPanel == null) return;
        var parent = itemDetailPanel.transform.parent;
        if (parent == null) return;
        var existing = parent.Find(clickBlockerName);
        if (existing != null)
        {
            clickBlocker = existing.gameObject;
        }
        else
        {
            var go = new GameObject(clickBlockerName);
            var rt = go.AddComponent<RectTransform>();
            var img = go.AddComponent<Image>();
            go.transform.SetParent(parent, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            img.color = new Color(0f, 0f, 0f, 0.3f);
            img.raycastTarget = true;
            clickBlocker = go;
        }
        var canvas = parent.GetComponentInParent<Canvas>();
        if (canvas != null && canvas.GetComponent<GraphicRaycaster>() == null)
        {
            canvas.gameObject.AddComponent<GraphicRaycaster>();
        }
        clickBlocker.transform.SetSiblingIndex(itemDetailPanel.transform.GetSiblingIndex());
        itemDetailPanel.transform.SetAsLastSibling();
    }
}
