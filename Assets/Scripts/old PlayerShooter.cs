using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class oldPlayerShooter : MonoBehaviour
{
    [Header("References")]
    public GameObject bulletPrefab;
    public Transform muzzlePoint;
    public UnitMotor myMotor;

    [Header("Fire Settings")]
    public float fireRate = 0.1f;
    private float nextFireTime = 0f;

    [Header("Spread System")]
    public float maxSpread = 25f;
    public float spreadPerShot = 5f;
    public float spreadRecoverySpeed = 20f;
    public float recoveryDelay = 0.2f;

    [Header("Jump Penalty")]
    public float jumpMultiplier = 3.0f;
    public float jumpMinBaseSpread = 5.0f; // 起跳时的保底散布基数

    // ==========================================
    // 核心修改区域：变量拆分
    // ==========================================
    [Header("Debug View (Watch This!)")]
    // 1. 这是一个只读的显示值：最终实际生效的散布 (实时更新)
    [SerializeField] private float finalSpread; 
    
    // 2. 这是内部变量：记录枪管热度 (连射增加)
    [SerializeField] private float currentHeat; 
    
    private float lastFireTime = 0f;

    void Start()
    {
        if (myMotor == null) myMotor = GetComponent<UnitMotor>();
    }

    void Update()
    {
        // === 1. 实时计算最终散布 (放在 Update 里以实现实时显示) ===
        bool isAirborne = (myMotor != null && myMotor.IsAirborne);

        // 如果在空中，强行给一个保底基数 (避免 0 * 3 = 0)
        // 如果在地面，就用当前热度
        float baseVal = isAirborne ? Mathf.Max(currentHeat, jumpMinBaseSpread) : currentHeat;
        
        // 计算乘数
        float multiplier = isAirborne ? jumpMultiplier : 1f;

        // 赋值给这个变量，你现在可以在 Inspector 实时看到它变化了！
        finalSpread = baseVal * multiplier;


        // === 2. 射击检测 ===
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireRate;
        }

        // === 3. 热度恢复 ===
        if (Time.time > lastFireTime + recoveryDelay)
        {
            if (currentHeat > 0)
                currentHeat -= spreadRecoverySpeed * Time.deltaTime;
        }
        currentHeat = Mathf.Clamp(currentHeat, 0, maxSpread);
    }

    void Fire()
    {
        if (bulletPrefab == null || muzzlePoint == null) return;

        lastFireTime = Time.time;

        // === 4. 直接使用 Update 里算好的 finalSpread ===
        float randomYaw = Random.Range(-finalSpread, finalSpread);
        Quaternion spreadRot = Quaternion.Euler(0, randomYaw, 0);
        Instantiate(bulletPrefab, muzzlePoint.position, muzzlePoint.rotation * spreadRot);

        // 增加内部热度
        currentHeat += spreadPerShot;
    }
}