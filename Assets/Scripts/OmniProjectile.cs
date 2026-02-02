using UnityEngine;

public class OmniProjectile : MonoBehaviour
{
    [Header("Ballistics")]
    public float speed = 25f;
    public float damage = 10f;
    public float lifeTime = 3f;

    [Header("Tracking System")]
    [Tooltip("垂直修正上限：每秒最大爬升速度")]
    public float vtr = 15f;             
    public float verticalHitbox = 1.2f; 
    [Tooltip("瞄准高度偏移：1.2 代表瞄准胸口")]
    public float heightOffset = 1.2f; 

    // === 内部状态 (发射后就不变了) ===
    private Vector3 flatVelocity;     // 水平速度向量
    private float fixedVerticalSpeed; // 计算好的垂直恒定速度

    public void Initialize(Vector3 dir, Transform target, float weaponVTR)
    {
        // 1. 初始化基础参数
        dir.y = 0; // 抹平水平方向
        Vector3 flatDir = dir.normalized;
        flatVelocity = flatDir * speed; // 水平速度恒定
        vtr = weaponVTR;
        
        Destroy(gameObject, lifeTime);

        // === 2. 发射瞬间的核心计算 (Snapshot Calculation) ===
        if (target != null)
        {
            // A. 获取目标当前时刻的数据
            Vector3 targetPos = target.position;
            float myY = transform.position.y;
            float targetY = targetPos.y + heightOffset; // 瞄准胸口

            // B. 计算水平距离
            // 我们只关心平面距离，因为子弹水平速度是恒定的
            Vector3 myPosFlat = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 targetPosFlat = new Vector3(targetPos.x, 0, targetPos.z);
            float dist = Vector3.Distance(myPosFlat, targetPosFlat);

            // C. 计算飞行时间 (Time to Impact)
            // 时间 = 距离 / 水平速度
            float flightTime = dist / speed;

            // D. 计算需要的垂直速度
            // 如果要在 flightTime 秒后刚好到达 targetY，我需要每秒飞多少？
            float heightDiff = targetY - myY;
            float neededVSpeed = heightDiff / flightTime;

            // E. 应用 VTR 限制 (Clamp)
            // 如果需要的速度太快(目标太高或太近)，就只能用最大VTR，导致Miss
            fixedVerticalSpeed = Mathf.Clamp(neededVSpeed, -vtr, vtr);
        }
        else
        {
            // 没锁到人，就直飞
            fixedVerticalSpeed = 0f;
        }

        // === 3. 立即设定初始朝向 ===
        UpdateRotation();
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // === 1. 应用位移 ===
        // 这一步非常简单：水平走水平的，垂直走垂直的
        // 不再实时追踪目标，所以不会上下乱跳
        Vector3 displacement = flatVelocity * dt; // XZ位移
        displacement.y = fixedVerticalSpeed * dt; // Y位移

        transform.position += displacement;

        // === 2. 视觉朝向更新 ===
        UpdateRotation();
    }

    void UpdateRotation()
    {
        // 合成总速度向量
        Vector3 totalVelocity = flatVelocity;
        totalVelocity.y = fixedVerticalSpeed;

        if (totalVelocity != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(totalVelocity);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // 判定逻辑不变
            float enemyCenterY = other.transform.position.y + heightOffset;
            float heightDiff = Mathf.Abs(transform.position.y - enemyCenterY);

            if (heightDiff <= verticalHitbox)
            {
                // Instantiate(hitEffect, transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
        }
        //else if (other.gameObject.layer == LayerMask.NameToLayer("Ground") || other.CompareTag("Obstacle"))
        //{
            //Destroy(gameObject);
        //}
    }
}