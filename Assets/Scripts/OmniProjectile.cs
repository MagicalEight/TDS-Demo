using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OmniProjectile : MonoBehaviour
{
    private Vector3 startPos;
    private Vector3 targetPosFlat; // 目标的平面位置 (X, Z)
    private float targetHeight;    // 目标的最后已知高度
    private float bulletSpeed;
    private float vtr;             // 垂直修正效率
    private float currentHeight;
    
    private float totalDist;
    private float distTraveled = 0f;

    public void Initialize(Vector3 start, Vector3 targetFlat, float tHeight, float speed, float weaponVTR, float startH)
    {
        startPos = start;
        startPos.y = 0; // 逻辑平面修正
        targetPosFlat = targetFlat;
        targetPosFlat.y = 0;
        targetHeight = tHeight;
        bulletSpeed = speed;
        vtr = weaponVTR;
        currentHeight = startH;

        totalDist = Vector3.Distance(startPos, targetPosFlat);
        
        // 设置初始视觉位置
        transform.position = new Vector3(start.x, startH, start.z);
    }

    void Update()
    {
        float step = bulletSpeed * Time.deltaTime;
        distTraveled += step;
        
        // 1. 平面运动 (Lerp 模拟直线)
        float progress = distTraveled / totalDist;
        Vector3 currentFlatPos = Vector3.Lerp(startPos, targetPosFlat, progress);

        // 2. 垂直修正 (VTR 核心逻辑)
        // 每一帧，子弹都在努力向目标高度靠拢
        float heightDiff = targetHeight - currentHeight;
        float climbStep = vtr * Time.deltaTime;
        
        if (Mathf.Abs(heightDiff) > 0.1f)
        {
            // 向目标高度移动，但不超过最大修正能力
            currentHeight += Mathf.Sign(heightDiff) * Mathf.Min(Mathf.Abs(heightDiff), climbStep);
        }

        // 3. 应用坐标
        transform.position = new Vector3(currentFlatPos.x, currentHeight, currentFlatPos.z);

        // 4. 销毁判定
        if (progress >= 1f) Destroy(gameObject);
    }
}