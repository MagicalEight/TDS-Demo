using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    [Header("References")]
    public GameObject bulletPrefab; // 拖入你的子弹 Prefab
    public Transform muzzlePoint;   // 拖入玩家模型上的“枪口”子物体

    [Header("Settings")]
    public float fireRate = 0.2f;   // 射击间隔 (秒)
    private float nextFireTime = 0f;

    void Update()
    {
        // 检测鼠标左键 (0) 且 过了射击冷却时间
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Fire()
    {
        if (bulletPrefab == null || muzzlePoint == null) return;

        // 核心逻辑：在枪口位置生成子弹
        // quaternion.identity 代表无旋转，但我们需要子弹朝向枪口的方向
        // 所以使用 muzzlePoint.rotation
        Instantiate(bulletPrefab, muzzlePoint.position, muzzlePoint.rotation);
    }
}