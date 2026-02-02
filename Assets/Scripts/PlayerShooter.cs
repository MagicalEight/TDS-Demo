using UnityEngine;
using System.Collections.Generic;

public class PlayerShooter : MonoBehaviour
{
    [Header("References")]
    public GameObject bulletPrefab;
    public Transform muzzlePoint;
    public UnitMotor myMotor;

    [Header("Fire Settings")]
    public float fireRate = 0.1f;
    private float nextFireTime = 0f;

    [Header("Area Auto-Aim (广域锁定)")]
    public float lockRadius = 3.0f; 
    public float lockRange = 15.0f; 
    
    [Header("Weapon VTR")]
    public float weaponVTR = 15.0f;

    [Header("Spread System")]
    public float maxSpread = 25f;
    public float spreadPerShot = 5f;
    public float spreadRecoverySpeed = 20f;
    public float recoveryDelay = 0.2f;

    [Header("Jump Penalty")]
    public float jumpMultiplier = 3.0f;
    public float jumpMinBaseSpread = 5.0f;

    // === 状态变量 ===
    [Header("Debug View")]
    [SerializeField] private float finalSpread;
    [SerializeField] private float currentHeat;
    
    // 【核心变化】这个变量现在是实时更新的，UI脚本可以读取它来改变准星颜色
    [SerializeField] public Transform currentLockedTarget; 
    
    private float lastFireTime = 0f;

    void Start()
    {
        if (myMotor == null) myMotor = GetComponent<UnitMotor>();
    }

    void Update()
    {
        // 1. 【新增】实时运行火控雷达
        // 每一帧都在扫描，这样你才能在UI上显示“已锁定”
        currentLockedTarget = ScanForTarget();

        // 2. 散布计算
        bool isAirborne = (myMotor != null && myMotor.IsAirborne);
        float baseVal = isAirborne ? Mathf.Max(currentHeat, jumpMinBaseSpread) : currentHeat;
        float multiplier = isAirborne ? jumpMultiplier : 1f;
        finalSpread = baseVal * multiplier;

        // 3. 射击输入
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            Fire(); // Fire 现在只负责开枪，不再负责找人了
            nextFireTime = Time.time + fireRate;
        }

        // 4. 热度恢复
        if (Time.time > lastFireTime + recoveryDelay)
        {
            if (currentHeat > 0) currentHeat -= spreadRecoverySpeed * Time.deltaTime;
        }
        currentHeat = Mathf.Clamp(currentHeat, 0, maxSpread);
    }

    void Fire()
    {
        if (bulletPrefab == null || muzzlePoint == null) return;

        lastFireTime = Time.time;

        // === 1. 直接使用 Update 里已经锁定的目标 ===
        // 这里不需要再 Physics.OverlapCapsule 了，省性能

        // === 2. 计算散布 ===
        float randomYaw = Random.Range(-finalSpread, finalSpread);
        Quaternion spreadRot = Quaternion.Euler(0, randomYaw, 0);
        Vector3 shootDir = (muzzlePoint.rotation * spreadRot) * Vector3.forward;

        // === 3. 发射 ===
        GameObject bulletObj = Instantiate(bulletPrefab, muzzlePoint.position, Quaternion.LookRotation(shootDir));
        OmniProjectile projectile = bulletObj.GetComponent<OmniProjectile>();
        
        if (projectile != null)
        {
            // 传进去 currentLockedTarget (可能为 null，也可能是一个 Transform)
            projectile.Initialize(shootDir, currentLockedTarget, weaponVTR);
        }

        currentHeat += spreadPerShot;
    }

    // === 独立出来的扫描函数 ===
    Transform ScanForTarget()
    {
        Vector3 point1 = muzzlePoint.position;
        Vector3 point2 = muzzlePoint.position + muzzlePoint.forward * lockRange;

        Collider[] hits = Physics.OverlapCapsule(point1, point2, lockRadius);

        Transform bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            Vector3 dirToEnemy = hit.transform.position - muzzlePoint.position;
            float distToEnemy = dirToEnemy.magnitude;

            // 防穿墙检测
            if (Physics.Raycast(muzzlePoint.position, dirToEnemy, out RaycastHit wallCheck, distToEnemy))
            {
                if (wallCheck.collider.transform != hit.transform && !wallCheck.collider.CompareTag("Enemy"))
                {
                    continue; 
                }
            }

            float dSqr = dirToEnemy.sqrMagnitude;
            if (dSqr < closestDistanceSqr)
            {
                closestDistanceSqr = dSqr;
                bestTarget = hit.transform;
            }
        }

        return bestTarget;
    }

    // === 视觉辅助 ===
    void OnDrawGizmos()
    {
        if (muzzlePoint != null)
        {
            // 如果锁定了目标，线变成红色；没锁定则是半透明黄色
            if (currentLockedTarget != null)
                Gizmos.color = Color.red;
            else
                Gizmos.color = new Color(1, 1, 0, 0.2f);
            
            Vector3 start = muzzlePoint.position;
            Vector3 end = muzzlePoint.position + muzzlePoint.forward * lockRange;
            
            Gizmos.DrawWireSphere(start, lockRadius);
            Gizmos.DrawWireSphere(end, lockRadius);
            Gizmos.DrawLine(start + Vector3.up * lockRadius, end + Vector3.up * lockRadius);
            Gizmos.DrawLine(start - Vector3.up * lockRadius, end - Vector3.up * lockRadius);
            Gizmos.DrawLine(start + Vector3.right * lockRadius, end + Vector3.right * lockRadius);
            Gizmos.DrawLine(start - Vector3.right * lockRadius, end - Vector3.right * lockRadius);

            // 如果锁定了，画一条线连向目标，方便调试
            if (currentLockedTarget != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(muzzlePoint.position, currentLockedTarget.position);
            }
        }
    }
}