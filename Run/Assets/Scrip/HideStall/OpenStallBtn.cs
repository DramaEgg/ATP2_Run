using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenStallBtn : MonoBehaviour
{
    public bool doorOpened = false;//false是关，true是开门

    public Transform pivotPoint;
    public float rotationSpeed = 90f;

    public GameObject targetDoor;

    private bool isRotating = false;

    public Quaternion initialRotation,targetRotation;

    // Start is called before the first frame update
    void Start()
    {
        initialRotation = targetDoor.transform.rotation;
        targetRotation = initialRotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (isRotating)
        {
            targetDoor.transform.rotation = Quaternion.Slerp(targetDoor.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            // 检查是否已经接近目标旋转
            if (Quaternion.Angle(targetDoor.transform.rotation, targetRotation) < 0.9f)
            {
                targetDoor.transform.rotation = targetRotation;
                isRotating = false;
            }
        }
    }

    public void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Input.GetKeyDown(KeyCode.F) && !isRotating)
            {
                //Open the Door,play the animation
                doorOpened = !doorOpened;
                DoorCtrl();
            }
        }
    }

    public void DoorCtrl()
    {
        if (!doorOpened)
        {
            StartCoroutine(RotateDoor(-90));
        }
        else
        {
            StartCoroutine(RotateDoor(90));
        }
    }

    // 协程处理旋转动画
    private IEnumerator RotateDoor(float angle)
    {
        isRotating = true;

        // 保存当前物体的世界位置
        Vector3 originalPosition = targetDoor.transform.position;

        // 计算相对于锚点的旋转
        targetDoor.transform.RotateAround(pivotPoint.position, Vector3.down, angle);

        // 计算新的目标旋转
        targetRotation = targetDoor.transform.rotation;

        // 恢复原始位置，让旋转在Update中平滑进行
        targetDoor.transform.position = originalPosition;

        yield return null;
    }


}
