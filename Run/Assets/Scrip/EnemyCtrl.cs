using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyCtrl : MonoBehaviour
{
    private NavMeshAgent agent;


    public enum EnemyState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Return,
        Search,
        Die
    }

    public EnemyState currentState;

    [Header("Timer")]
    public float stateTimer = 0f;
    public float idleTime = 2f;
    public float searchTimer = 0f;
    public float searchDuration = 3f;

    public float chaseTimer = 0f;
    public float chaseDuration = 5f;


    [Header("Enemy Speed in different States")]

    public float patrolSpeed;
    public float chaseSpeed;

    public float attackDistance = 0.5f;
    public float turnSpeed;

    [Header("Patroling")]
    public Vector3 startPos;
    public float waitTime = 3f;
    public Vector3[] wayPoints;


    [Header("Random Searching")]
    public Vector3 lastShownPos;
    public float patrolRadius = 20f;

    [Header("Alert")]
    public float currentAlertLevel = 0;
    public float maxAlertLevel = 10f;
    public float chasedLevel = 7f;
    public float lastAlertTriggerTime;
    public float quietTime = 3f;

    [Header("Alert Slider")]
    public GameObject sliderGameObj;
    public Slider alertSlider;
    public string alertString = "!";
    public string worryString = "?";
    public Image image;
    //public TMP_Text alterText;

    private Coroutine currentBehavior;

    [Header("view")]
    public Light spotLight;
    public float viewDistance = 10f;
    public LayerMask viewMask;
    float viewAngle = 90f;


    [Header("Path Transform")]
    public Transform pathHolder;
    Transform player;

    [Header("Refering from Player")]
    public GameObject playerObj;
    public ThirdPersonController playerController;
    public PlayerHealth playerHealth;

    [Header("Attack")]
    public bool canAttack = true;
    public bool isAttacking = false;
    public float damageValue;
    public float EnemyHP;

    //public Animator animator;
    //public SpriteRenderer spriteRenderer;

    //public AudioSource enemyADS;
    //public AudioClip attacking;


    public bool isDead;

    public delegate void EnemyDeathHandler(EnemyCtrl enemy);
    public static event EnemyDeathHandler OnEnemyDeath;


    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        startPos = transform.position;
        playerObj = GameObject.FindWithTag("Player");
        playerController = playerObj.GetComponent<ThirdPersonController>();
        playerHealth = playerObj.GetComponent<PlayerHealth>();

        lastAlertTriggerTime = Time.time;

        //enemyADS = GetComponent<AudioSource>();

    }

    void Start()
    {
        //Get PlayerComponent
        player = GameObject.FindGameObjectWithTag("Player").transform;

        //Enemy Start Speed
        agent.speed = patrolSpeed;

        //Spot Angle 
        spotLight.spotAngle = viewAngle;

        wayPoints = new Vector3[pathHolder.childCount];

        //待定
        for (int i = 0; i < wayPoints.Length; i++)
        {
            wayPoints[i] = pathHolder.GetChild(i).position;
            wayPoints[i].y = transform.position.y;
        }

        currentBehavior = StartCoroutine(PatrolBehavior(wayPoints));

        //Slider-Alert
        alertSlider.maxValue = maxAlertLevel;
        alertSlider.value = 0f;
        sliderGameObj.SetActive(false);

        //animator.Play("Move");
    }

    void Update()
    {
        if (isDead) return;

        UpdateState();

        ViewColorChanged();

        alertTimerCheck();
        AlertBehavior();

        AlertSlider();

        //图片朝向
        //if (agent.velocity.sqrMagnitude > 0.01f)
        //{
        //    if (agent.velocity.x>0)
        //    {
        //        spriteRenderer.flipX= false;
        //    }
        //    else
        //    {
        //        spriteRenderer.flipX = true;
        //    }
        //}

        //nemyHpCtrl();

    }


    //先禁用了，老是报错
    //void EnemyHpCtrl()
    //{
    //    EnemyHP -= 1;

    //    if (EnemyHP <= 0)
    //    {
    //        Debug.Log("Enemy Die");
    //        EnemyDie();
    //    }
    //}

    //public void EnemyDie()
    //{
    //    Debug.Log("Enemy Die");
    //    isDead = true;
    //    agent.enabled = false;
    //    //animator.SetBool("isDead",true);

    //    //if Die, move out from liberay
    //    OnEnemyDeath?.Invoke(this);
    //    Destroy(gameObject,3f);
    //}

    //Alert Check
    void ViewColorChanged()
    {
        if (CanSeePlayer())
        {
            spotLight.color = Color.red;

        }
        else
        {
            spotLight.color = Color.yellow;
        }
    }

    public bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        // 检查距离是否在视距内
        if (distanceToPlayer > viewDistance) return false;

        // 检查夹角是否在视角范围内
        float angle = Vector3.Angle(transform.forward, directionToPlayer.normalized);
        if (angle > viewAngle / 2) return false;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, directionToPlayer.normalized, out hit, viewDistance))
        {
            if (hit.transform == player)
            {
                return true;
            }
        }

        return false;
    }
    //public bool CanSeePlayer() {

    //    if (Vector3.Distance(transform.position,player.position) < viewDistance) {

    //        Vector3 dirToPlayer = (player.position - transform.position).normalized;
    //        float angleBetweenGuardAndPlayer = Vector3.Angle(transform.forward,dirToPlayer);

    //        if (angleBetweenGuardAndPlayer<viewAngle/2f)  
    //        {
    //            if (!Physics.Linecast(transform.position,player.position,viewMask))
    //            {
    //                //is in Grass Check
    //                if (playerController.isInGrass && playerController.isCrouching)
    //                {
    //                    return false;
    //                }
    //                return true;
    //            }
    //        }
    //    }
    //    return false;
    //}

    void AlertSlider()
    {
        alertSlider.value = currentAlertLevel;

        //Slider Show Up
        if (currentAlertLevel > 0)
        {
            sliderGameObj.SetActive(true);
        }
        else
        {
            sliderGameObj.SetActive(false);
        }

        if (currentAlertLevel < maxAlertLevel)
        {
            image.color = Color.yellow;
            //alterText.text = worryString;
        }
        else
        {
            image.color = Color.red;
            //alterText.text = alertString;
        }

    }

    void AlertBehavior()
    {
        currentAlertLevel = Mathf.Clamp(currentAlertLevel, 0f, maxAlertLevel);
        // Debug.Log("currentAlertLevel" + currentAlertLevel);
        if (CanSeePlayer())
        {
            currentAlertLevel += 3 * Time.deltaTime;
            playerDetected();
            //UI Lerp 在这里加
        }

    }

    public void alertTimerCheck()
    {
        if (Time.time - lastAlertTriggerTime > quietTime)
        {
            ReduceAlertLevel();
        }
    }

    public void ReduceAlertLevel()
    {
        if (currentAlertLevel > 0)
        {
            currentAlertLevel -= 1 * Time.deltaTime;
            currentAlertLevel = Mathf.Max(currentAlertLevel, 0f);
        }
    }

    public void playerDetected()
    {
        lastAlertTriggerTime = Time.time;
    }

    void UpdateState()
    {
        switch (currentState)
        {

            case EnemyState.Idle:
                IdleBehavior();
                if (currentAlertLevel >= chasedLevel)
                {
                    ChangeState(EnemyState.Chase);
                }
                break;

            case EnemyState.Patrol:
                PatrolBehavior(wayPoints);
                if (currentAlertLevel >= chasedLevel)
                {
                    ChangeState(EnemyState.Chase);
                }
                break;

            case EnemyState.Chase:
                //ChaseBehavior();

                if (!CanSeePlayer())
                {
                    // 如果玩家在藏匿或者敌人找不到玩家，进入搜索状态
                    if (playerController != null && playerController.isHiding)
                    {
                        // 玩家正在藏匿，立即进入搜索状态
                        ChangeState(EnemyState.Search);
                    }
                    else if (Vector3.Distance(transform.position, player.position) > viewDistance)
                    {
                        // 距离过远，进入搜索状态
                        ChangeState(EnemyState.Search);
                    }
                }
                // 保留原有的逻辑
                else if (currentAlertLevel < chasedLevel && Vector3.Distance(transform.position, player.position) > viewDistance)
                {
                    ChangeState(EnemyState.Search);
                }
                break;

            case EnemyState.Attack:
                //AttackBehavior();
                break;

            case EnemyState.Search:
                SearchArea();
                if (currentAlertLevel >= chasedLevel)
                {
                    ChangeState(EnemyState.Chase);
                }
                break;

            case EnemyState.Return:
                ReturnToStart();
                if (Vector3.Distance(transform.position, startPos) < 1.0f)
                {
                    ChangeState(EnemyState.Patrol); // 到达起始位置后返回巡逻状态
                }
                // 检查是否需要追击玩家
                else if (currentAlertLevel >= chasedLevel)
                {
                    ChangeState(EnemyState.Chase);
                } 
                break;
            //case EnemyState.Die:
            //    EnemyDie();
            //    break;
            default:
                break;
        }
    }

    void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;

        currentState = newState;


        if (currentBehavior != null)
        {
            StopCoroutine(currentBehavior);
        }

        //FSM-Enemy
        switch (newState)
        {
            case EnemyState.Idle:
                IdleBehavior();
                agent.speed = 0f;
                break;

            case EnemyState.Patrol:
                currentBehavior = StartCoroutine(PatrolBehavior(wayPoints));
                agent.speed = patrolSpeed;
                break;

            case EnemyState.Chase:
                currentBehavior = StartCoroutine(ChaseBehavior());
                agent.speed = chaseSpeed;
                break;

            case EnemyState.Return:
                //currentBehavior = StartCoroutine(ReturnToStart());
                ReturnToStart();
                agent.speed = patrolSpeed;
                break;

            case EnemyState.Attack:
                currentBehavior = StartCoroutine(AttackBehavior());
                agent.speed = chaseSpeed;
                break;

            case EnemyState.Search:
                SearchArea();
                agent.speed = patrolSpeed;
                searchTimer = 0f;
                break;

                //case EnemyState.Die:
                //    EnemyDie();
                //    agent.speed = 0f;
                //    break;


        }

    }

    IEnumerator PatrolBehavior(Vector3[] wayPoints)
    {

        Debug.Log("Patroling");


        int targetWaypointIndex = 1;
        Vector3 targetWaypoint = wayPoints[targetWaypointIndex];


        while (true) {
            if (!agent.pathPending || agent.remainingDistance < 0.5f)
            {
                targetWaypointIndex = (targetWaypointIndex + 1) % wayPoints.Length;
                targetWaypoint = wayPoints[targetWaypointIndex];
                //new destination
                agent.SetDestination(targetWaypoint);
                yield return new WaitForSeconds(waitTime);
            }

            yield return null;

        }
    }

    //IEnumerator TurnToFace(Vector3 lookTarget)
    //{

    //    Debug.Log("Turn Face");

    //    Vector3 dirToLookTarget = (lookTarget - transform.position).normalized; //当前向量数据
    //    float targetAngle = 90 - Mathf.Atan2(dirToLookTarget.z, dirToLookTarget.x) * Mathf.Rad2Deg; //当前向量参数的方向，转为角度，计算目标角度



    //    while (true)
    //    {

    //        //计算当前角度与目标角度的差值
    //        float angleDifference = Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle));
    //        //打印Check
    //        //   Debug.Log("Angle difference: " + angleDifference);
    //        //   Debug.Log($"Current Angle: {transform.eulerAngles.y}, Target Angle: {targetAngle}");

    //        //当角度差小于一定阈值，结束协程
    //        if (angleDifference < 0.1f)
    //        {
    //            //         Debug.Log("TurnTO Face break");
    //            yield break;
    //        }

    //        //旋转向目标方向
    //        Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);
    //        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);


    //        yield return null;
    //        //       Debug.Log("TurnToFace completed.");

    //    }
    //}

    //Drawing Gizmos for distance in view, road and conor.
    void OnDrawGizmos()
    {
        Vector3 startPos = pathHolder.GetChild(0).transform.position;
        Vector3 previousPos = startPos;
        Gizmos.color = Color.yellow;
        foreach (Transform waypoint in pathHolder)
        {
            Gizmos.DrawSphere(waypoint.position, .3f);
            Gizmos.DrawLine(previousPos, waypoint.position);
            previousPos = waypoint.position;
        }
        Gizmos.DrawLine(previousPos, startPos);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * viewDistance);


        /*Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, soundRadius);*/

    }

    void IdleBehavior()
    {
        Debug.Log("Idle");

        if (stateTimer < idleTime)
        {
            Debug.Log("StateTimer<idleTime");
            stateTimer += Time.deltaTime;
        }
        else
        {
            Debug.Log("StateTimer>idleTime");

            ChangeState(EnemyState.Patrol);
            stateTimer = 0;
        }
    }



    IEnumerator ChaseBehavior()
    {
        Debug.Log("Chasing");
        chaseTimer = 0f;

        //if canseePlayer or distance is in 10f, then won't stop chasing,
        while (currentState == EnemyState.Chase && !isDead)
        {
            //Can See Player
            if (CanSeePlayer() || currentAlertLevel >= chasedLevel)
            {
                lastShownPos = player.position;
                agent.SetDestination(player.position);

                if (Vector3.Distance(transform.position, player.position) < attackDistance)
                {
                    StartCoroutine(AttackBehavior());
                }

            }
            else //Cant See
            {
                if (currentAlertLevel < chasedLevel)
                {
                    ChangeState(EnemyState.Search);
                    yield break;
                }
                else
                {
                    // 追击玩家最后出现的位置
                    agent.SetDestination(lastShownPos);

                    // 如果已经接近最后位置但仍看不到玩家
                    if (Vector3.Distance(transform.position, lastShownPos) < 1.5f)
                    {
                        chaseTimer += Time.deltaTime;

                        // 如果在最后位置附近找了一段时间还是找不到，转入搜索状态
                        if (chaseTimer > 3.0f)
                        {
                            ChangeState(EnemyState.Search);
                            yield break;
                        }
                    }
                }
            }

            yield return null;

        }
        yield break;

    }


    private void ReturnToStart()
    {
        Debug.Log("Return To Start");
        agent.SetDestination(startPos);
        if (!agent.hasPath || agent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathInvalid)
        {
            ChangeState(EnemyState.Patrol);
        }
    }


    IEnumerator AttackBehavior()
    {
        while (true)
        {
            if (CanSeePlayer() && Vector3.Distance(transform.position, player.position) < attackDistance)
            {
                if (canAttack)
                {
                    playerHealth.Damage(damageValue);
                    Debug.Log("Attack");
                    //音频
                    //enemyADS.PlayOneShot(attacking);  
                    //先禁用，不然老报错
                    //EnemyHpCtrl();
                    canAttack = false;
                    yield return new WaitForSeconds(2f);
                    canAttack = true;
                }

            }
            yield return null;
        }
    }

 void SearchArea()
{
    Debug.Log("Search");
    
    // 如果能直接看到玩家，立即切换回追击状态
    if (CanSeePlayer())
    {
        ChangeState(EnemyState.Chase);
        return;
    }
    
    if (searchTimer < searchDuration)
    {
        searchTimer += Time.deltaTime;
        
        // 如果正在调查声音
        if (isInvestigatingSound)
        {
            // 如果没有路径或已到达声音位置
            if (!agent.hasPath || Vector3.Distance(transform.position, soundInvestigationPosition) < 1.5f || agent.remainingDistance < 0.5f)
            {
                // 到达声音位置后，向四周搜索
                Vector3 randomPoint = RandomNavMeshPoint(soundInvestigationPosition, patrolRadius/2);
                agent.SetDestination(randomPoint);
                
                // 如果已经在声音位置搜索了一段时间，不再特别关注它
                if (Vector3.Distance(transform.position, soundInvestigationPosition) < 3f)
                {
                    isInvestigatingSound = false;
                }
            }
            else
            {
                // 继续前往声音位置
                agent.SetDestination(soundInvestigationPosition);
            }
        }
        // 如果不是调查声音，就是普通搜索
        else
        {
            // 如果没有路径或已到达目标点，设置新的随机搜索点
            if (!agent.hasPath || agent.remainingDistance < 0.5f)
            {
                Vector3 randomPoint = RandomNavMeshPoint(lastShownPos, patrolRadius);
                agent.SetDestination(randomPoint);
            }
        }
    }
    else
    {
        // 搜索时间结束，重置声音调查状态
        isInvestigatingSound = false;
        // 返回初始位置
        ChangeState(EnemyState.Return);
    }
}



    private Vector3 soundInvestigationPosition;
    private bool isInvestigatingSound = false;

    public void InvestigateSound(Vector3 soundPosition)
    {
        // 保存声音位置
        soundInvestigationPosition = soundPosition;
        isInvestigatingSound = true;

        // 根据当前状态和警戒值决定行为
        if (currentState == EnemyState.Idle || currentState == EnemyState.Patrol)
        {
            // 如果当前在巡逻或待机，直接去调查
            ChangeState(EnemyState.Search);
        }
        else if (currentState == EnemyState.Chase)
        {
            // 如果当前在追逐玩家，但声音足够引人注目，可能会分心去看
            // 这取决于游戏设计，可以注释掉这部分如果你不希望敌人在追逐中被分心
            if (currentAlertLevel < maxAlertLevel * 0.7f) // 如果不确定玩家在哪
            {
                ChangeState(EnemyState.Search);
            }
        }

        //// 无论如何，更新警戒值
        //playerDetected();
    }

    private Vector3 RandomNavMeshPoint(Vector3 center, float radius) 
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += center;

        if (NavMesh.SamplePosition(randomDirection,out NavMeshHit hit, radius, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return center;
    }

}
