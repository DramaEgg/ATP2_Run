using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("��������")]
    [SerializeField] private float interactionDistance = 2.5f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private Transform holdPosition;
    [SerializeField] private float holdForce = 10f;

    [Header("��������")]
    //[SerializeField] private float minLaunchForce = 5f;
    //[SerializeField] private float maxLaunchForce = 20f;
    //[SerializeField] private float chargeRate = 5f;
    [SerializeField] private LineRenderer aimLine;

    [Header("��ɫ����")]
    public float rotationSpeed = 5f;
    private Vector3 mouseStartPos;
    private Quaternion characterStartPos;
    private bool isRotating = false;

    // ˽�б���
    private GameObject heldObject;
    private Rigidbody heldRigidbody;
    public bool isHolding = false;
    private bool isAiming = false;
    public float launchForce = 10f;
    public Camera mainCamera;

    // ��������ʱ�����
    public bool isFKeyDown = false;

    public void Start()
    {
            // ʹ�����������Ϊ��������
            interactionCenter = transform;

        // ��ʼ����׼��
        if (aimLine == null)
        {
            aimLine = gameObject.GetComponent<LineRenderer>();
            aimLine.startWidth = 0.05f;
            aimLine.endWidth = 0.05f;
            aimLine.positionCount = 2;
            aimLine.material = new Material(Shader.Find("Sprites/Default"));
            aimLine.startColor = Color.red;
            aimLine.endColor = Color.yellow;
        }
        aimLine.enabled = false;
    }


    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartRotation();
        }

        if (Input.GetMouseButtonUp(0))
        {
            isRotating = false;
        }

        if (isRotating)
        { 
            UpdateRotation();
        }

       
        if (Input.GetKeyDown(KeyCode.F))
        {
            isFKeyDown = !isFKeyDown;//New IN

            if (isFKeyDown)
            {
                PickUpObj();
            }
            else
            {
                DropObject();
            }
            Debug.Log("Down");
        }
        
        //if (Input.GetKeyUp(KeyCode.F))
        //{
        //    DropObject();
        //    isFKeyDown = false;
        //    Debug.Log("UP");
        //}


        if (isFKeyDown && isHolding)
        {
            if (Input.GetMouseButtonDown(0))
            {
                isAiming = true;
                StartCharging();
            }
            else if (Input.GetMouseButtonUp(0) && isCharging)
            {
                LaunchObj();
                isFKeyDown = false;
            }
            Charging();
        }
        else
        {
            isAiming = false;
        }
      
        // ֻ�ڰ�F��������׼ʱ��ʾ��׼��
        aimLine.enabled = isFKeyDown && isAiming;
        if(aimLine.enabled) UpdateAimLine();

        Debug.Log("isAiming" + isAiming);



        if (isHolding && heldObject != null)
        {
            MoveObjectToHoldPosition();
        }


    }

    public void StartRotation()
    { 
        mouseStartPos = Input.mousePosition;
        characterStartPos = transform.rotation;
        isRotating = true;
    }

    public void UpdateRotation()
    {
        float deltaX = (Input.mousePosition.x - mouseStartPos.x)* rotationSpeed;
        float rotationY = deltaX * 0.1f;
        transform.rotation = characterStartPos * Quaternion.Euler(0,rotationY,0);
    }

    public void PickUpObj()
    {
        FindNearestInteractableObject();

        // ����ҵ��˿ɽ�������
        if (nearestObject != null && !isHolding)
        {
            // ��ȡ����ĸ������
            Rigidbody rb = nearestObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                heldObject = nearestObject;
                heldRigidbody = rb;

                // ��������
                heldRigidbody.useGravity = false;

                // ��ѡ�������������������ײ
                if (heldObject.GetComponent<Collider>())
                {
                    heldObject.GetComponent<Collider>().enabled = false;
                }

                //isFKeyDown = !isFKeyDown;//New IN
                isHolding = true;
            }
        }

    }

    private void MoveObjectToHoldPosition()
    {
        // �������嵽����λ�õķ���;���
        Vector3 directionToHoldPos = holdPosition.position - heldRigidbody.position;

        // ʹ�ø����ƶ����壨��ֱ������λ�ø���Ȼ��
        heldRigidbody.velocity = directionToHoldPos * holdForce;

        // ��ѡ���ȶ���ת
        heldRigidbody.angularVelocity = Vector3.Lerp(heldRigidbody.angularVelocity, Vector3.zero, Time.deltaTime * 5f);
    }

    [Header("��Ӧ��Χ����")]
    [SerializeField] private float detectionRadius = 2.5f;
    [SerializeField] private Transform interactionCenter; // ����������������Ļ�ǰ��һ��Ŀ�����

    // �ɽ��������б��ڷ�Χ�ڵ����壩
    private List<GameObject> interactableObjects = new List<GameObject>();
    private GameObject nearestObject = null;

    private void FindNearestInteractableObject()
    {
        // ʹ�����μ����Χ�Ŀɽ�������
        Collider[] hitColliders = Physics.OverlapSphere(
            interactionCenter != null ? interactionCenter.position : transform.position,
            detectionRadius,
            interactableLayer
        );

        float closestDistance = float.MaxValue;
        nearestObject = null;

        // �����ҵ���ȫ����ײ��
        foreach (var hitCollider in hitColliders)
        {
            // ȷ�������и������
            if (hitCollider.attachedRigidbody != null)
            {
                // �������
                float distance = Vector3.Distance(
                    interactionCenter != null ? interactionCenter.position : transform.position,
                    hitCollider.transform.position
                );

                // ��������������������������
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    nearestObject = hitCollider.gameObject;
                }
            }
        }
    }

    // �������壨�����䣩
    private void DropObject()
    {
        if (heldObject != null)
        {
            // �ָ���������
            heldRigidbody.useGravity = true;

            // �ָ���ײ
            if (heldObject.GetComponent<Collider>())
            {
                heldObject.GetComponent<Collider>().enabled = true;
            }

            // ������һ����С������������Ȼ����
            heldRigidbody.AddForce(Vector3.down * 0.1f, ForceMode.Impulse);

            // ���ñ���
            heldObject = null;
            heldRigidbody = null;
            isHolding = false;
        }
    }

    ////Normal One
    void UpdateAimLine()
    {
        aimLine.SetPosition(0, holdPosition.position);
        Vector3 launchDirection = holdPosition.forward;
        aimLine.SetPosition(1, holdPosition.position + launchDirection * 5f);


        //aimLine.SetPosition(0, launchPoint.transform.position);
        //aimLine.SetPosition(1, launchPoint.transform.position + currentAimDirection * 5f);
    }

    float currentForceMultiplier = 1f;
    float minForceMultiplier = 1f;
    float maxForceMul = 5f;
    float chargingSpeed = 2f;
    bool isCharging = false;
    float chargeDirection = 1f; //1 = add, -1 = less

    //Normal 1.0Ver
    public void LaunchObj()
    {
        isAiming = false;
        isCharging = false;
        if (heldObject!= null)
        {
            heldRigidbody.useGravity = true;
            if (heldObject.GetComponent<Collider>())
            {
                heldObject.GetComponent<Collider>().enabled = true;
            }

            float actualLaunchForce = launchForce * currentForceMultiplier;
            heldRigidbody.AddForce(transform.forward * actualLaunchForce, ForceMode.Impulse);

            // ���ñ���
            heldObject = null;
            heldRigidbody = null;
            isHolding = false;
            currentForceMultiplier = minForceMultiplier;

        }
    }

    public void StartCharging()
    {
        if (heldObject != null && !isCharging )
        {
            isCharging= true;
            currentForceMultiplier = minForceMultiplier;
        }
    }

    public void Charging()
    {
        if (isCharging)
        {
            currentForceMultiplier += chargeDirection * chargingSpeed * Time.deltaTime;

            if (currentForceMultiplier >= maxForceMul)
            {
                currentForceMultiplier = maxForceMul;
                chargingSpeed = -1f;
            }
            else if (currentForceMultiplier <= minForceMultiplier)
            {
                currentForceMultiplier = minForceMultiplier;
                chargingSpeed = 1f;
            }

            //���UI��������ʾ��ǰ����
        }
    
    }


    // ���ƽ�����Χ�����ڱ༭���пɼ����������ԣ�
    private void OnDrawGizmosSelected()
    {
        if (interactionCenter != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(interactionCenter.position, detectionRadius);
        }
        else
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }


}
