using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("交互设置")]
    [SerializeField] private float interactionDistance = 2.5f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private Transform holdPosition;
    [SerializeField] private float holdForce = 10f;

    [Header("发射设置")]
    //[SerializeField] private float minLaunchForce = 5f;
    //[SerializeField] private float maxLaunchForce = 20f;
    //[SerializeField] private float chargeRate = 5f;
    [SerializeField] private LineRenderer aimLine;

    [Header("角色朝向")]
    public float rotationSpeed = 5f;
    private Vector3 mouseStartPos;
    private Quaternion characterStartPos;
    private bool isRotating = false;

    // 私有变量
    private GameObject heldObject;
    private Rigidbody heldRigidbody;
    public bool isHolding = false;
    private bool isAiming = false;
    public float launchForce = 10f;
    public Camera mainCamera;

    // 按键持续时间跟踪
    public bool isFKeyDown = false;

    public void Start()
    {
            // 使用玩家自身作为交互中心
            interactionCenter = transform;

        // 初始化瞄准线
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
      
        // 只在按F键并且瞄准时显示瞄准线
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

        // 如果找到了可交互物体
        if (nearestObject != null && !isHolding)
        {
            // 获取物体的刚体组件
            Rigidbody rb = nearestObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                heldObject = nearestObject;
                heldRigidbody = rb;

                // 禁用重力
                heldRigidbody.useGravity = false;

                // 可选：禁用与其他物体的碰撞
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
        // 计算物体到持有位置的方向和距离
        Vector3 directionToHoldPos = holdPosition.position - heldRigidbody.position;

        // 使用刚体移动物体（比直接设置位置更自然）
        heldRigidbody.velocity = directionToHoldPos * holdForce;

        // 可选：稳定旋转
        heldRigidbody.angularVelocity = Vector3.Lerp(heldRigidbody.angularVelocity, Vector3.zero, Time.deltaTime * 5f);
    }

    [Header("感应范围设置")]
    [SerializeField] private float detectionRadius = 2.5f;
    [SerializeField] private Transform interactionCenter; // 可以是玩家身体中心或前方一点的空物体

    // 可交互物体列表（在范围内的物体）
    private List<GameObject> interactableObjects = new List<GameObject>();
    private GameObject nearestObject = null;

    private void FindNearestInteractableObject()
    {
        // 使用球形检测周围的可交互物体
        Collider[] hitColliders = Physics.OverlapSphere(
            interactionCenter != null ? interactionCenter.position : transform.position,
            detectionRadius,
            interactableLayer
        );

        float closestDistance = float.MaxValue;
        nearestObject = null;

        // 遍历找到的全部碰撞体
        foreach (var hitCollider in hitColliders)
        {
            // 确认物体有刚体组件
            if (hitCollider.attachedRigidbody != null)
            {
                // 计算距离
                float distance = Vector3.Distance(
                    interactionCenter != null ? interactionCenter.position : transform.position,
                    hitCollider.transform.position
                );

                // 如果这个物体更近，更新最近物体
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    nearestObject = hitCollider.gameObject;
                }
            }
        }
    }

    // 放下物体（不发射）
    private void DropObject()
    {
        if (heldObject != null)
        {
            // 恢复物理属性
            heldRigidbody.useGravity = true;

            // 恢复碰撞
            if (heldObject.GetComponent<Collider>())
            {
                heldObject.GetComponent<Collider>().enabled = true;
            }

            // 给物体一个很小的力，让它自然掉落
            heldRigidbody.AddForce(Vector3.down * 0.1f, ForceMode.Impulse);

            // 重置变量
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

            // 重置变量
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

            //添加UI在这里显示当前力度
        }
    
    }


    // 绘制交互范围（仅在编辑器中可见，帮助调试）
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
