using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenStallBtn : MonoBehaviour
{
    public bool doorOpened = false;//false�ǹأ�true�ǿ���

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

            // ����Ƿ��Ѿ��ӽ�Ŀ����ת
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

    // Э�̴�����ת����
    private IEnumerator RotateDoor(float angle)
    {
        isRotating = true;

        // ���浱ǰ���������λ��
        Vector3 originalPosition = targetDoor.transform.position;

        // ���������ê�����ת
        targetDoor.transform.RotateAround(pivotPoint.position, Vector3.down, angle);

        // �����µ�Ŀ����ת
        targetRotation = targetDoor.transform.rotation;

        // �ָ�ԭʼλ�ã�����ת��Update��ƽ������
        targetDoor.transform.position = originalPosition;

        yield return null;
    }


}
