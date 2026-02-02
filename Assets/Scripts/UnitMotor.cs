using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 负责角色的平面移动与垂直机动
public class UnitMotor : MonoBehaviour
{
    [Header("Mobility Stats")] public float moveSpeed = 8f;
    public float jumpForce = 10f;
    public float gravity = 20f;

    [Header("Visuals")] public Transform shadowTransform; // 拖入脚下的影子对象

    private Vector3 velocity;
    private CharacterController controller; // 使用CC比Rigidbody更容易控制手感
    private float verticalVelocity;

    void Start()
    {
        controller = gameObject.AddComponent<CharacterController>();
        // 调整CC中心，防止陷入地面
        // controller.center = new Vector3(0, 1, 0); 
    }

    void Update()
    {
        HandleMovement();
        HandleVerticality();
        UpdateShadow();
    }

    void HandleMovement()
    {
        // 1. 获取平面输入 (X/Z)
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(h, 0, v).normalized;

        // 2. 应用移动速度
        if (direction.magnitude >= 0.1f)
        {
            controller.Move(direction * moveSpeed * Time.deltaTime);

            // 简单的朝向鼠标旋转逻辑
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100, LayerMask.GetMask("Ground")))
            {
                Vector3 lookPoint = hit.point;
                lookPoint.y = transform.position.y; // 保持视线水平
                transform.LookAt(lookPoint);
            }
        }
    }

    void HandleVerticality()
    {
        // 3. 立体机动：跳跃 (Z-Axis Displacement)
        if (controller.isGrounded)
        {
            verticalVelocity = -2f; // 保持贴地
            if (Input.GetKeyDown(KeyCode.Space))
            {
                verticalVelocity = jumpForce; // 瞬间改变高度
            }
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }

        controller.Move(new Vector3(0, verticalVelocity, 0) * Time.deltaTime);
    }

    void UpdateShadow()
    {
        // 4. 视差处理：影子永远贴在地面 y=0.01
        if (shadowTransform)
        {
            shadowTransform.position = new Vector3(transform.position.x, 0.01f, transform.position.z);
            shadowTransform.rotation = Quaternion.identity;
        }
    }

    public bool IsAirborne
    {
        get
        {
            if (controller == null) return false;

            // 判定逻辑修改：
            // 1. !controller.isGrounded: 物理判定已经离地
            // 2. verticalVelocity > 0.5f: 刚刚按下跳跃键，有了向上的速度（解决起跳瞬间判定失效的问题）
            return !controller.isGrounded || verticalVelocity > 0.5f;
        }
    }
}