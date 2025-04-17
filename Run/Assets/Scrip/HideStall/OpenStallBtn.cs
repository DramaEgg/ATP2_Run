using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenStallBtn : MonoBehaviour
{
    public bool doorOpened = false;//false�ǹأ�true�ǿ���

    public Transform pivotPoint,detectCenter; 
    public float rotationSpeed = 90f;

    //public GameObject targetDoor;

    private bool isRotating = false;

    private Quaternion initialRotation,targetRotation;

    [Header("Detect Radius F������")]
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

            // ����Ƿ��Ѿ��ӽ�Ŀ����ת
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
    /// ����תЭ�̺��� Door rotate function
    /// </summary>
    private IEnumerator RotateDoor(float angle)
    {
        isRotating = true;

        // ���浱ǰ���������λ��
        Vector3 originalPosition = transform.position;

        // ���������ê�����ת
        transform.RotateAround(pivotPoint.position, Vector3.down, angle);

        // �����µ�Ŀ����ת
        targetRotation = transform.rotation;

        // �ָ�ԭʼλ�ã�����ת��Update��ƽ������
        transform.position = originalPosition;

        yield return null;
    }

    /// <summary>
    /// �������Ƿ��ڸ��� detect if player nearby the target door.
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
        // ���Ƽ�ⷶΧ������
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(detectCenter.position, detectionRadius);
    }

}
