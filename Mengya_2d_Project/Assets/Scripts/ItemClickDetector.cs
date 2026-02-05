using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemClickDetector : MonoBehaviour
{
    private ItemData currentItemData; // 当前点击的物品数据
    private PlayerMovement player;

    void Awake()
    {
        // 查找玩家（通过Player标签，和你原有逻辑一致）
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.GetComponent<PlayerMovement>();
        }
        else
        {
            Debug.LogWarning("未找到Player物体，无法进行距离检测！");
        }
    }

    void Start()
    {
        // 获取当前物品身上的ItemData组件
        currentItemData = GetComponent<ItemData>();

        // 安全校验
        if (currentItemData == null)
        {
            Debug.LogWarning("当前物品未挂载ItemData组件，请添加！");
        }
    }

    void Update()
    {
        // 检测鼠标左键点击
        if (Input.GetMouseButtonDown(0))
        {
            CheckItemClick();
        }
    }

    /// <summary>
    /// 检测是否点击到当前物品（已修复z轴赋值错误）
    /// </summary>
    private void CheckItemClick()
    {
        if (player == null || currentItemData == null)
        {
            return;
        }

        // 2. 射线检测（你的原有逻辑，判断是否点击到当前物品）
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null && hit.collider.gameObject == this.gameObject)
        {
            // 3. 【新增核心】计算玩家与物品的直线距离
            float distanceToPlayer = Vector3.Distance(player.GetPlayerPosition(), transform.position);

            // 4. 判断距离是否在拾取半径内
            if (distanceToPlayer <= player.pickUpRadius)
            {
                // 符合条件：触发详情面板
                ItemDetailManager.Instance.ShowDetailPanel(currentItemData);
            }
            else
            {
                // 超出半径：给出提示（可选，方便玩家知晓）
                Debug.LogWarning($"请靠近物品！当前距离：{distanceToPlayer:F2}，需要≤{player.pickUpRadius}");
                // 可选：添加UI提示（比如屏幕中间显示“距离过远，无法拾取”）
            }
        }
     
    }

    /// <summary>
    /// 弹出物品详情面板
    /// </summary>
    private void ShowItemDetail()
    {
        if (currentItemData == null || ItemDetailManager.Instance == null)
        {
            Debug.LogWarning("无法显示详情面板：物品数据或详情管理器不存在！");
            return;
        }

        // 调用详情管理器，显示面板并传递物品数据
        ItemDetailManager.Instance.ShowDetailPanel(currentItemData);
    }
}