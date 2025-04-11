using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NewCamera : MonoBehaviour
{

    [Header("Camera Follow")]
    public Transform playerPos;

    public float followDistance = 0f;
    public float followSpeed = 3.5f;

    //用于记录摄像机与玩家之间的偏移方向
    public Vector3 initialOffset;

    private float fixedHeight;//摄像机高度

    public float fixedDistance = 0f;

    void Start()
    {
        initialOffset = initialOffset.normalized * fixedDistance;

        Vector3 startPos = playerPos.position + initialOffset;
        startPos.y = fixedHeight;
        transform.position = startPos;
    }

    void LateUpdate()
    {

        if (Input.GetMouseButtonDown(1))
        {
            RotateCamera90();
        }

        
            CameraFollow();

        transform.LookAt(playerPos);
    }

    /// <summary>
    /// When Click right BTN rotate Camera 90 degree
    /// </summary>

    void RotateCamera90()
    {
        Vector3 newOffset = Quaternion.Euler(0, 90, 0) * initialOffset;
        initialOffset = newOffset.normalized * fixedDistance;

        Vector3 newPos = playerPos.position + initialOffset;
        transform.position = newPos;
    }


    void CameraFollow()
    {
        Vector3 targetPos = playerPos.position + initialOffset.normalized * fixedDistance;
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
        
    }

}
