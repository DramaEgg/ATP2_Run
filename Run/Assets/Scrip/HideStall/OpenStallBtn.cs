using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenStallBtn : MonoBehaviour
{
    public bool doorOpened = false;//false是关，true是开门

    public Transform pivotPoint,detectCenter; 
    public float rotationSpeed = 90f;

    //public GameObject targetDoor;

    private bool isRotating = false;

    private Quaternion initialRotation,targetRotation;

    [Header("Detect Radius F检测距离")]
    public float detectionRadius = 1.18f;

    // Start is called before the first frame update
    void Start()
    {
        initialRotation = transform.rotation;
        targetRotation = initialRotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (isRotating)
        {
           transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            // 检查是否已经接近目标旋转
            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.9f)
            {
               transform.rotation = targetRotation;
                isRotating = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.F) && !isRotating)
        {
            if (IsPlayerNearby())
            {
                doorOpened = !doorOpened;
                DoorCtrl();
            }
        }
    }


    public void DoorCtrl()
    {
        StopAllCoroutines();

        if (!doorOpened)
        {
            StartCoroutine(RotateDoor(-90));
        }
        else
        {
            StartCoroutine(RotateDoor(90));
        }
    }

    /// <summary>
    /// 门旋转协程函数 Door rotate function
    /// </summary>
    private IEnumerator RotateDoor(float angle)
    {
        isRotating = true;

        // 保存当前物体的世界位置
        Vector3 originalPosition = transform.position;

        // 计算相对于锚点的旋转
        transform.RotateAround(pivotPoint.position, Vector3.down, angle);

        // 计算新的目标旋转
        targetRotation = transform.rotation;

        // 恢复原始位置，让旋转在Update中平滑进行
        transform.position = originalPosition;

        yield return null;
    }

    /// <summary>
    /// 检测玩家是否在附近 detect if player nearby the target door.
    /// </summary>
    public bool IsPlayerNearby()
    {

        Collider[] hitColliders = Physics.OverlapSphere(detectCenter.position, detectionRadius);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }

    void OnDrawGizmosSelected()
    {
        // 绘制检测范围的球体
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(detectCenter.position, detectionRadius);
    }

}
