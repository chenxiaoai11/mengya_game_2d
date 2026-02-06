using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// 玩家移动和场景切换核心逻辑
public class PlayerMovement : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5f; // 普通移动速度
    public float runSpeed = 8f; // 奔跑速度（新增）
    private float currentSpeed; // 当前使用的速度（新增）

    [Header("场景切换")]
    public string level2SceneName = "Level2"; // 第二关场景名称，需和Build Settings一致
    private bool isCanTriggerLevel = true; // 防止重复触发场景切换

    [Header("物品拾取设置")]
    public float pickUpRadius = 2f;
    private bool isMoveLocked = false; // 移动锁定状态：true=停止移动 false=可移动

    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private Vector3 originalScale; // 存储玩家初始缩放，用于移动时转向

    public Vector3 GetPlayerPosition()
    {
        return transform.position;
    }

    // 当物体被选中时，在Scene视图绘制拾取范围的圆圈（辅助调试，运行时可见）
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue; // 圆圈颜色为蓝色
        Gizmos.DrawWireSphere(transform.position, pickUpRadius); // 绘制拾取范围（以玩家为中心）
    }

    void Awake()
    {
        // 获取玩家的Rigidbody2D组件
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogWarning("玩家缺少Rigidbody2D组件，自动添加");
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        // 关闭2D重力（平面移动）
        rb.gravityScale = 0;

        // 记录玩家的初始缩放大小（用于转向时恢复Scale值的基准）
        originalScale = transform.localScale;
        Debug.Log($"已记录初始缩放：{originalScale}");

        currentSpeed = moveSpeed; // 初始化当前速度为普通速度（新增）
    }

    void Update()
    {
        // 如果移动被锁定，直接返回，不执行任何移动逻辑
        if (isMoveLocked)
        {
            moveDirection = Vector2.zero;
            return;
        }

        // 获取输入：水平方向（AD键）
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        moveDirection = new Vector2(horizontalInput, 0).normalized;

        // 核心修改：检测Shift + 左右方向（A/D）组合键，切换奔跑/普通速度
        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && horizontalInput != 0)
        {
            currentSpeed = runSpeed; // 按下Shift+A/D，使用奔跑速度
        }
        else
        {
            currentSpeed = moveSpeed; // 松开任意键，恢复普通速度
        }

        // 转向逻辑（原有逻辑不变）
        if (horizontalInput != 0)
        {
            float targetScaleX = horizontalInput > 0 ? originalScale.x : -originalScale.x;
            transform.localScale = new Vector3(targetScaleX, originalScale.y, originalScale.z);
        }
    }

    void FixedUpdate()
    {
        // 修改：使用currentSpeed动态切换速度，替代原固定moveSpeed
        rb.velocity = moveDirection * currentSpeed;
    }

    // 检测是否触发场景切换的碰撞器
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"触发碰撞体：{other.gameObject.name}，标签：{other.tag}");

        // 判断是否触发第二关切换（通过标签筛选）
        if (other.CompareTag("Level2Trigger") && isCanTriggerLevel)
        {
            TriggerLevel2();
        }
    }

    // 触发第二关切换（同步逻辑）
    private void TriggerLevel2()
    {
        isCanTriggerLevel = false; // 防止重复触发
        isMoveLocked = true; // 切换时停止移动
        rb.velocity = Vector2.zero; // 停止当前移动
        Debug.Log($"开始切换到第二关：{level2SceneName}");

        // 直接同步加载场景（无异步，简化逻辑）
        LoadLevel2Sync();
    }

    // 同步加载第二关（可修改，替换为异步）
    private void LoadLevel2Sync()
    {
        SceneManager.LoadScene(level2SceneName);
        Debug.Log($"第二关 {level2SceneName} 已加载，准备同步UI");

        // 同步背包数据到UI
        if (BackpackManager.Instance != null)
        {
            Debug.Log("同步背包数据到UI");
            BackpackManager.Instance.SyncGlobalDataToUI();
        }
        else
        {
            Debug.LogError("同步失败：BackpackManager.Instance为空");
        }
    }

    // ===== 物品拾取相关接口（原有逻辑不变）=====
    /// <summary>
    /// 拾取物品时调用，锁定玩家移动
    /// </summary>
    public void LockPlayerMovement()
    {
        isMoveLocked = true;
        rb.velocity = Vector2.zero;
        Debug.Log("玩家移动已锁定（物品拾取中）");
    }

    /// <summary>
    /// 完成拾取/取消拾取时调用，解锁玩家移动
    /// </summary>
    public void UnlockPlayerMovement()
    {
        isMoveLocked = false;
        Debug.Log("玩家移动已解锁");
    }
}