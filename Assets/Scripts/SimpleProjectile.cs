using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleProjectile : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 20f;
    public float lifeTime = 3f;
    public float damage = 25f; // 伤害数值

    [Header("VFX")]
    public GameObject hitEffect; // 命中时的火花/爆炸特效

    void Start()
    {
        Destroy(gameObject, lifeTime); // 超时自毁
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
    }

    // === 核心：碰撞检测 ===
    // 注意：勾选 Collider 上的 IsTrigger
    void OnTriggerEnter(Collider other)
    {
        // 1. 既然是打敌人，先判断撞到的是不是敌人
        // (推荐使用 Tag，或者检查是否有 EnemyHealth 组件)
        if (other.CompareTag("Enemy"))
        {
            // 2. 获取敌人身上的血量脚本
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            
            if (enemy != null)
            {
                // 3. 扣血
                enemy.TakeDamage(damage);
            }

            // 4. 命中处理 (生成特效 + 销毁子弹)
            HitDestruction();
        }
        //else if (other.CompareTag("Obstacle")) // 比如墙壁
       // {
        //    HitDestruction();
       // }
    }

    void HitDestruction()
    {
        // 生成命中特效
        if (hitEffect != null)
            Instantiate(hitEffect, transform.position, Quaternion.identity);
            
        // 销毁子弹自身
        Destroy(gameObject);
    }
}