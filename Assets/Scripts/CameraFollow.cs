using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // 拖入 Player 根物体

    [Header("Settings")]
    public float smoothTime = 0.2f; // 跟随延迟时间，越小越硬，越大越软
    public Vector3 offset; // 相机与玩家的固定距离

    [Header("Tactical Peeking (战术窥视)")]
    public bool enableMousePeek = true;
    public float peekRange = 3f; // 相机向鼠标方向偏移的最大距离
    
    private Vector3 currentVelocity; // SmoothDamp 的内部变量

    void Start()
    {
        // 如果没有手动设置 Offset，自动计算当前相对距离
        if (target != null)
        {
            offset = transform.position - target.position;
        }
    }

    // 使用 LateUpdate 确保在玩家移动计算完毕后才移动相机，防止抖动
    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = target.position + offset;

        // --- 战术偏移逻辑 ---
        if (enableMousePeek)
        {
            // 获取鼠标在屏幕上的归一化位置 (0到1中心点为0.5)
            Vector3 mouseScreenPos = Input.mousePosition;
            // 转化为 -1 到 1 的区间
            float mouseX = (mouseScreenPos.x / Screen.width * 2) - 1; 
            float mouseY = (mouseScreenPos.y / Screen.height * 2) - 1;
            
            // 限制偏移量，避免相机跑太远
            Vector3 peekOffset = new Vector3(mouseX, 0, mouseY) * peekRange;
            
            // 注意：因为相机是斜视的，这里的 Z 轴偏移需要适当调整适配视角
            // 但为了简单，我们直接叠加到目标位置
            targetPos += peekOffset;
        }
        // ------------------

        // 平滑移动核心函数
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, smoothTime);
    }
}
