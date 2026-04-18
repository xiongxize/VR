using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRPlayerController : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform headTransform;
    [SerializeField] private float moveSpeed = 3.0f;
    [SerializeField] private float rotationSpeed = 180.0f;
    
    private Vector2 moveInput;
    private Vector2 lookInput;
    
    private void Awake()
    {
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }
        
        if (headTransform == null)
        {
            headTransform = Camera.main.transform;
        }
    }
    
    private void Update()
    {
        HandleMovement();
    }
    
    private void HandleMovement()
    {
        // 获取移动输入（可以从XR控制器或键盘获取）
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        // 计算移动方向
        Vector3 moveDirection = new Vector3(horizontal, 0, vertical);
        moveDirection = headTransform.TransformDirection(moveDirection);
        moveDirection.y = 0;
        moveDirection.Normalize();
        
        // 应用移动
        characterController.SimpleMove(moveDirection * moveSpeed);
        
        // 处理旋转（如果需要）
        float rotation = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        transform.Rotate(0, rotation, 0);
    }
}