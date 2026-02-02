using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Settings")]
    public GameObject enemyPrefab;   // 敌人的预制体
    public KeyCode respawnKey = KeyCode.R; // 重生按键

    [Header("Spawn Points")]
    public Transform[] spawnPoints;  // 生成点列表

    void Update()
    {
        if (Input.GetKeyDown(respawnKey))
        {
            RespawnAll();
        }
    }

    void RespawnAll()
    {
        // 1. 清除当前场景里的旧敌人
        // 我们通过 Tag 来查找，这样即使是你手动拖进场景的敌人也能被清理
        GameObject[] existingEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        foreach (GameObject enemy in existingEnemies)
        {
            Destroy(enemy);
        }

        Debug.Log($"<color=yellow>已清理 {existingEnemies.Length} 个旧单位。</color>");

        // 2. 在每个生成点生成新敌人
        if (enemyPrefab != null && spawnPoints.Length > 0)
        {
            foreach (Transform point in spawnPoints)
            {
                if (point != null)
                {
                    Instantiate(enemyPrefab, point.position, point.rotation);
                }
            }
            Debug.Log($"<color=green>重生完成！生成了 {spawnPoints.Length} 个新单位。</color>");
        }
    }

    // 辅助：在 Scene 窗口画出生成点位置，防止找不到
    void OnDrawGizmos()
    {
        if (spawnPoints != null)
        {
            Gizmos.color = Color.cyan;
            foreach (Transform point in spawnPoints)
            {
                if (point != null)
                {
                    // 画一个球表示生成位置
                    Gizmos.DrawWireSphere(point.position, 0.5f);
                    // 画个小人标示方向
                    Gizmos.DrawIcon(point.position, "d_User Icon", true); 
                }
            }
        }
    }
}