using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraFollow2D : MonoBehaviour
{
    [Header("跟随目标")]
    public Transform player; // 拖拽玩家对象到这里

    [Header("跟随参数")]
    public float smoothSpeed = 0.125f; // 跟随平滑度，值越小越平滑
    public Vector3 offset; // 相机与玩家的偏移量（比如让玩家在画面中心偏下）

    [Header("边界限制（可选）")]
    public bool enableBounds = true; // 是否启用边界限制
    public float minX; // 相机最左能移动到的X坐标
    public float maxX; // 相机最右能移动到的X坐标
    public float minY; // 相机最下能移动到的Y坐标
    public float maxY; // 相机最上能移动到的Y坐标

    void FixedUpdate()
    {
        if (player == null) return;

        // 计算目标位置
        Vector3 desiredPosition = player.position + offset;

        // 平滑插值到目标位置
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // 边界限制（如果启用）
        if (enableBounds)
        {
            smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minX, maxX);
            smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, minY, maxY);
        }

        // 更新相机位置
        transform.position = smoothedPosition;
    }
}