using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemDetailManager : MonoBehaviour
{
    // 1. 手动赋值字段（和你之前一样，拖拽当前场景的组件）
    public PlayerMovement playerMovement;
    public GameObject itemDetailPanel;
    public Image itemShowImage;
    public TMP_Text detailItemName;
    public TMP_Text detailItemDesc;

    // 2. 场景内单例（只保证当前场景有一个实例，不跨场景）
    public static ItemDetailManager Instance;
    private ItemData currentSelectedItem;

    void Awake()
    {
        // 场景内单例：当前场景如果已有实例，就销毁新的
        if (Instance == null)
        {
            Instance = this;
            // 移除 DontDestroyOnLoad！！！不跨场景保留，只在当前场景生效
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 3. 显示详情面板（逻辑不变，用手动赋值的组件）
    public void ShowDetailPanel(ItemData itemData)
    {
        Debug.Log($"=== 显示详情面板检查 ===");
        Debug.Log($"itemData是否为空: {itemData == null}");
        Debug.Log($"itemDetailPanel是否为空: {itemDetailPanel == null}");
        Debug.Log($"playerMovement是否为空: {playerMovement == null}");

        if (itemData == null || itemDetailPanel == null || playerMovement == null)
        {
            Debug.LogWarning("物品/面板/Player为空，无法显示详情！");
            return;
        }

        playerMovement.LockPlayerMovement();
        currentSelectedItem = itemData;
        UpdateDetailPanelContent(itemData);
        itemDetailPanel.SetActive(true);
        Debug.Log($"显示{itemData.itemName}详情面板");
    }

    // 4. 更新面板内容（逻辑不变）
    private void UpdateDetailPanelContent(ItemData itemData)
    {
        if (itemShowImage != null)
        {
            itemShowImage.sprite = itemData.itemIcon;
            itemShowImage.gameObject.SetActive(itemData.itemIcon != null);
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

    // 5. 其他方法（逻辑不变）
    public void HideDetailPanel()
    {
        if (itemDetailPanel != null)
        {
            itemDetailPanel.SetActive(false);
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
            Debug.LogWarning("无法拾取物品：未选中物品或背包管理器无效");
            HideDetailPanel();
            return;
        }

        BackpackManager.Instance.AddItemToBackpack(currentSelectedItem);
        currentSelectedItem = null;
        HideDetailPanel();
    }

    public void OnCancelButtonClick()
    {
        Debug.Log("取消拾取，关闭详情面板");
        HideDetailPanel();
    }
}