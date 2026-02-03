using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// 玩家移动和关卡切换核心逻辑
public class PlayerMovement : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5f; // 移动速度

    [Header("关卡设置")]
    public string level2SceneName = "Level2"; // 第二关场景名称（需和Build Settings一致）
    private bool isCanTriggerLevel = true; // 防止重复触发关卡切换

    [Header("物品拾取相关")]
    private bool isMoveLocked = false; // 移动锁定状态：true=停止移动 false=可移动

    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private Vector3 originalScale; // 存储玩家初始缩放，用于移动时翻转

    void Awake()
    {
        // 获取玩家的Rigidbody2D组件
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogWarning("玩家缺少Rigidbody2D组件，已自动添加");
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        // 关闭重力（2D平面移动）
        rb.gravityScale = 0;

        // 记录玩家的初始缩放大小（用于转向时恢复Scale值）
        originalScale = transform.localScale;
        Debug.Log($"已记录玩家初始缩放：{originalScale}");
    }

    void Update()
    {
        // 如果移动被锁定，直接返回，不执行任何移动逻辑
        if (isMoveLocked)
        {
            // 清空移动方向，阻止任何输入移动
            moveDirection = Vector2.zero;
            return;
        }

        // 获取输入：水平方向（AD键）
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        moveDirection = new Vector2(horizontalInput, 0).normalized;

        // 转向逻辑（通过修改X轴缩放实现左右翻转，保留原始缩放大小避免拉伸）
        if (horizontalInput != 0)
        {
            float targetScaleX = horizontalInput > 0 ? originalScale.x : -originalScale.x;
            transform.localScale = new Vector3(targetScaleX, originalScale.y, originalScale.z);
        }
    }

    void FixedUpdate()
    {
        // 移动执行（在FixedUpdate中保证移动平滑）
        rb.velocity = moveDirection * moveSpeed;
    }

    // 检测是否触发关卡切换（碰撞体触发）
    void OnTriggerEnter2D(Collider2D other)
    {
        // 调试日志：查看碰撞对象
        Debug.Log($"触发碰撞对象：{other.gameObject.name}，标签：{other.tag}");

        // 判断是否触发第二关切换（通过标签筛选）
        if (other.CompareTag("Level2Trigger") && isCanTriggerLevel)
        {
            TriggerLevel2();
        }
    }

    // 触发第二关切换（同步加载版本）
    private void TriggerLevel2()
    {
        isCanTriggerLevel = false; // 防止重复触发
        isMoveLocked = true; // 切换时停止移动
        rb.velocity = Vector2.zero; // 停止当前移动
        Debug.Log($"开始切换到第二关：{level2SceneName}");

        // 直接同步加载关卡（覆盖当前场景）
        LoadLevel2Sync();
    }

    // 同步加载第二关（核心修改：替换异步为同步）
    private void LoadLevel2Sync()
    {
        // 直接加载场景（覆盖当前场景）
        SceneManager.LoadScene(level2SceneName);
        Debug.Log($"第二关 {level2SceneName} 已加载完成，准备同步背包UI");

        // 同步刷新背包UI（无需延迟，同步加载后直接执行）
        if (BackpackManager.Instance != null)
        {
            Debug.Log("立即同步背包数据到UI");
            BackpackManager.Instance.SyncGlobalDataToUI();
        }
        else
        {
            Debug.LogError("同步失败！BackpackManager.Instance为空");
        }
    }

    // ===== 物品拾取相关：锁定/解锁移动 =====
    /// <summary>
    /// 拾取物品时调用，停止玩家移动
    /// </summary>
    public void LockPlayerMovement()
    {
        isMoveLocked = true;
        // 立即停止玩家的当前移动（避免惯性）
        rb.velocity = Vector2.zero;
        Debug.Log("玩家移动已停止（物品拾取中）");
    }

    /// <summary>
    /// 完成拾取/取消拾取后调用，恢复玩家移动
    /// </summary>
    public void UnlockPlayerMovement()
    {
        isMoveLocked = false;
        Debug.Log("玩家移动已解锁");
    }
}