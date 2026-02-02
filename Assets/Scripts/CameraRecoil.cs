using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRecoil : MonoBehaviour
{
    [Header("Spring Settings (弹簧参数)")]
    // 刚度：越大回弹越快，越有“紧绷感”
    public float stiffness = 120f; 
    // 阻尼：越大停下来的越快，越小震荡次数越多
    public float damping = 15f;    

    [Header("Recoil Strength (后坐力强度)")]
    public float posStrength = 0.5f; // 位移强度
    public float rotStrength = 2.0f; // 旋转强度

    // 内部弹簧变量
    private Vector3 posVelocity;
    private Vector3 rotVelocity;
    private Vector3 targetPos;     // 目标永远是 (0,0,0)
    private Vector3 targetRot;     // 目标永远是 (0,0,0)

    // 当前的偏移量
    private Vector3 currentPosOffset;
    private Vector3 currentRotOffset;

    void Update()
    {
        // === 1. 位置弹簧计算 (Position Spring) ===
        // 核心公式：加速度 = (目标差值 * 刚度) - (当前速度 * 阻尼)
        Vector3 posForce = (targetPos - currentPosOffset) * stiffness - (posVelocity * damping);
        posVelocity += posForce * Time.deltaTime;
        currentPosOffset += posVelocity * Time.deltaTime;

        // === 2. 旋转弹簧计算 (Rotation Spring) ===
        Vector3 rotForce = (targetRot - currentRotOffset) * stiffness - (rotVelocity * damping);
        rotVelocity += rotForce * Time.deltaTime;
        currentRotOffset += rotVelocity * Time.deltaTime;

        // === 3. 应用到 Transform (局部坐标) ===
        // 这样不会影响父物体的跟随逻辑
        transform.localPosition = currentPosOffset;
        transform.localRotation = Quaternion.Euler(currentRotOffset);
    }

    /// <summary>
    /// 接收后坐力冲量
    /// </summary>
    /// <param name="fireDirection">玩家开火的世界方向</param>
    public void FireImpulse(Vector3 fireDirection)
    {
        // 归一化方向
        fireDirection.Normalize();

        // 1. 位移冲量：向开火的反方向“踢”相机
        // 注意：我们需要把世界方向转为相机的局部方向，或者直接在世界空间算好分量
        // 但为了简单且效果好，我们直接让相机向“后”移动
        // 这里做一个简单的映射：假设相机的 Z 轴大致对应世界的前后
        // 更好的做法是直接用反向矢量
        
        // 将世界方向的后坐力转换为相对于相机的局部震动
        // 比如：向右射击，相机应该向左震动
        Vector3 kickBack = -fireDirection * posStrength;
        
        // 由于相机是俯视斜着的（Rotated X 60），我们需要把这个平面的 KickBack 转换到相机的局部空间
        // 或者是简单的让相机在它的 Local Z 轴上后退（模拟冲击波）
        // 方案 A：简单粗暴，直接向后（Z轴）退，产生“砸脸”感
        // posVelocity += new Vector3(0, 0, -posStrength); 
        
        // 方案 B (你要求的)：基于射击方向的位移
        // 我们把世界坐标的 kickBack 施加到局部系统里，需要 InverseTransformDirection
        // 但通常 Top-down 只需要 X 和 Y (屏幕平面) 的震动
        Vector3 localKick = transform.parent.InverseTransformDirection(kickBack);
        // 忽略 Y 轴高度变化，只保留平面的震动
        localKick.y = 0; 
        
        posVelocity += localKick;


        // 2. 旋转冲量：产生一个猛烈的“抬头”或“侧倾”
        // Random.Range 让每次射击手感略微不同
        float randomRoll = Random.Range(-1f, 1f) * 0.5f; // Z轴轻微晃动
        float kickPitch = -1f; // X轴向上抬 (模拟枪口上跳带来的视觉冲击)

        rotVelocity += new Vector3(kickPitch, 0, randomRoll) * rotStrength;
    }
}