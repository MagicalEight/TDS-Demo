using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WeaponSystem : MonoBehaviour
{
    [Header("References")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Yaw Stats (Planar Accuracy)")]
    public float baseSpreadAngle = 2f;      // 静止精度
    public float movementSpread = 15f;      // 移动惩罚
    public float spreadRecovery = 50f;      // 准星回正速度
    public float heatPerShot = 5f;          // 连射热度
    public float maxSpread = 30f;           // 最大散布

    [Header("Pitch Stats (Vertical Tracking)")]
    public float vtrValue = 5f;             // VTR: 这把枪打高处目标的能力

    private float currentSpread = 0f;
    private float currentHeat = 0f;
    private CharacterController ownerController;

    void Start()
    {
        ownerController = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleShooting();
        ManageSpread();
    }

    void HandleShooting()
    {
        if (Input.GetMouseButton(0)) // 连射测试
        {
             if (Time.frameCount % 5 == 0) // 简单的射速控制
             {
                 Fire();
             }
        }
    }

    void ManageSpread()
    {
        // 1. 计算移动惩罚
        float velocityRatio = ownerController.velocity.magnitude / 10f; // 假设最大速度10
        float movePenalty = Mathf.Lerp(0, movementSpread, velocityRatio);

        // 2. 散布恢复 logic
        currentHeat = Mathf.MoveTowards(currentHeat, 0, spreadRecovery * Time.deltaTime);
        
        // 3. 最终散布 = 基础 + 移动 + 热度
        currentSpread = Mathf.Clamp(baseSpreadAngle + movePenalty + currentHeat, 0, maxSpread);
    }

    void Fire()
    {
        // --- 核心逻辑 A: 获取目标高度 ---
        // 射线检测鼠标指着的位置 (Ground)
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 targetPos = Vector3.zero;
        
        if (Physics.Raycast(ray, out RaycastHit hit, 100, LayerMask.GetMask("Ground")))
        {
            targetPos = hit.point;
            // *关键*：如果鼠标悬停在敌人身上，应该获取敌人的Y轴高度。
            // 这里为了演示，我们假设如果按住 Shift，就在瞄准空中 (模拟智能锁定)
            if (Input.GetKey(KeyCode.LeftShift)) 
            {
                targetPos.y = 4.0f; // 假设空中目标高度
            }
        }

        // --- 核心逻辑 B: Yaw 轴平面散布 ---
        // 获取枪口正前方
        Vector3 forward = transform.forward;
        // 在 -spread 到 +spread 之间随机一个角度
        float randomYaw = Random.Range(-currentSpread, currentSpread);
        // 将这个角度应用到 Quaternion 上 (围绕 Y 轴旋转)
        Quaternion spreadRot = Quaternion.Euler(0, randomYaw, 0);
        // 得到最终带有误差的发射方向
        Vector3 finalDir = spreadRot * forward;

        // 生成子弹
        GameObject b = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(finalDir));
        OmniBullet script = b.GetComponent<OmniBullet>();
        
        // --- 核心逻辑 C: 注入 VTR 参数 ---
        script.Initialize(finalDir, vtrValue, targetPos);

        // 增加热度
        currentHeat += heatPerShot;
    }
    
    // Debug UI: 画出散布扇区
    void OnDrawGizmos()
    {
        if(firePoint) {
            Gizmos.color = Color.red;
            Vector3 leftLimit = Quaternion.Euler(0, -currentSpread, 0) * transform.forward;
            Vector3 rightLimit = Quaternion.Euler(0, currentSpread, 0) * transform.forward;
            Gizmos.DrawRay(firePoint.position, leftLimit * 5);
            Gizmos.DrawRay(firePoint.position, rightLimit * 5);
        }
    }
}
