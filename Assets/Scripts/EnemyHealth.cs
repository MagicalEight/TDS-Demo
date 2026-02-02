using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Visuals (Optional)")]
    public GameObject deathEffect; // 死亡时的特效

    void Start()
    {
        currentHealth = maxHealth;
    }

    // 公共方法：允许外部（子弹）调用来造成伤害
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log($"{name} 受到伤害: {amount}. 剩余血量: {currentHealth}");

        // 这里可以加受击闪白、音效等逻辑

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // 生成死亡特效
        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        // 销毁敌人物体
        Destroy(gameObject);
    }
}