using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitState : MonoBehaviour
{
    [Header("Status")]
    public float currentHeight = 0f; // 当前逻辑高度
    public bool isPlayer = false;

    [Header("Visuals")]
    public Transform bodyModel; // 实际的角色模型
    public Transform shadowDecal; // 脚下的影子（必须有！）

    void Update()
    {
        // 视觉同步：身体飞起来，影子留在地上
        if (bodyModel)
            bodyModel.localPosition = new Vector3(0, currentHeight, 0);
        
        // 影子随高度变淡或变小（增强视差感）
        if (shadowDecal)
        {
            float scale = Mathf.Clamp(1f - (currentHeight / 10f), 0.2f, 1f);
            shadowDecal.localScale = new Vector3(scale, scale, scale);
        }
    }

    // 调试用：让非玩家单位模拟跳跃
    void FixedUpdate() 
    {
        if (!isPlayer) 
        {
            // 简单的正弦波模拟敌人上下浮空
            currentHeight = Mathf.Abs(Mathf.Sin(Time.time * 2f)) * 3.5f; 
        }
    }
}
