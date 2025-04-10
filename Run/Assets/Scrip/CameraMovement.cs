using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [Header("Camera Rotate")]
    public Transform rotateTarget;//旋转的目标
    public float xSpeed = 200;//控制y轴上的旋转速度，如速度为0，则禁用y轴旋转
    public float ySpeed = 200;//控制y轴上的旋转速度，如速度为0，则禁用y轴旋转
    public float mSpeed = 10;
    public float yMinLimit = -50;
    public float yMaxLimit = 50;
    public float distance = 2;
    public float minDistance = 2;
    public float maxDistance = 30;

    bool rightMouseClicked = false;

    float currentAngle = 0f;
    float targetAngle = 0f;
    float rotationAngle = 45f;

    public float height = 2f;             // 摄像机高度
    public float rotationSpeed = 5f;      // 旋转插值速度
    bool isRotating = false;

    float damping = 2f;
    //bool needDamping = false;
    public bool needDamping = true;

    public float x, y;

    [Header("Camera Follow")]
    public Transform playerPos;
    private Vector3 targetPosition;

    public float followDistance = 4.97f;
    public float followSpeed = 3.5f;

    private Vector3 initialOffset;

    private Quaternion targetRotation;


    void Start()
    {
        initialOffset = transform.position - playerPos.position;

        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
     
        targetRotation = transform.rotation;
    }

        void LateUpdate()
    {
        CameraRotate();
        CameraFollow();
    }



    void CameraFollow()
    {
        Vector3 targetPos = playerPos.position + initialOffset;
        Vector3 currentOffset = transform.position - playerPos.position;

        float currentDistance = currentOffset.magnitude;

        if (currentDistance > followDistance)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
        }
    }

    static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }

    void CameraRotate()
    {
        if (rotateTarget)
        {
            //use the light button of mouse to rotate the camera
            if (Input.GetMouseButton(1))
            {
                //x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;//
                //y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

                x += rotationAngle;

                if (x >= 360) x -= 360;
                rightMouseClicked = true;
                y = ClampAngle(y, yMinLimit, yMaxLimit);
            }
            //distance -= Input.GetAxis("Mouse ScrollWheel") * mSpeed;
            //distance = Mathf.Clamp(distance, minDistance, maxDistance);
            Quaternion rotation = Quaternion.Euler(y, x, 0.0f);
            Vector3 disVector = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = rotation * disVector + rotateTarget.position;
            //adjust the camera
            if (needDamping)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * damping);
                transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * damping);
            }
            else
            {
                transform.rotation = rotation;
                transform.position = position;
            }
        }

    }


}
