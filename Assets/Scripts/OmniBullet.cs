using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class OmniBullet : MonoBehaviour
{
    [Header("Ballistics")]
    public float speed = 25f;
    public float lifeTime = 3f;
    
    [Header("Pitch Tracking")]
    public float verticalTrackingRate = 10f; 
    
    private Vector3 targetPosition;
    private Vector3 horizontalDir; // 缓存水平方向，确保匀速直线运动

    public void Initialize(Vector3 direction, float vtr, Vector3 targetPos)
    {
        // 提取水平方向的归一化向量
        horizontalDir = new Vector3(direction.x, 0, direction.z).normalized;
        verticalTrackingRate = vtr;
        targetPosition = targetPos;
        
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // 1. 计算水平位移 (X/Z 平面)
        Vector3 nextHorizontalPos = transform.position + horizontalDir * speed * Time.deltaTime;

        // 2. 计算垂直修正 (Y 轴 / 原 Pitch 逻辑)
        // 算法：ΔZ = min(VTR * dt, 距离)
        float newY = Mathf.MoveTowards(transform.position.y, targetPosition.y, verticalTrackingRate * Time.deltaTime);
        
        // 3. 统一应用位置更新
        // 这样可以确保 XZ 轴在飞，同时 Y 轴在根据 VTR 爬升
        transform.position = new Vector3(nextHorizontalPos.x, newY, nextHorizontalPos.z);

        // 4. 让子弹朝向飞行方向（可选，为了视觉效果）
        if (horizontalDir != Vector3.zero)
        {
            transform.forward = horizontalDir;
        }

        // 碰撞检测
        CheckCollision(speed * Time.deltaTime);
    }

    void CheckCollision(float stepDistance)
    {
        // 简单的射线检测
        if (Physics.Raycast(transform.position, horizontalDir, out RaycastHit hit, stepDistance))
        {
            if (hit.collider.tag != "Player") // 避免撞到自己
            {
                Destroy(gameObject);
            }
        }
    }
}