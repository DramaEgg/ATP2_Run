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
    public float currentAlertLevel=0;
    public float maxAlertLevel = 10f;
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
    public float viewDistance =10f;
    public LayerMask viewMask;
    float viewAngle = 180f;


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

        lastAlertTriggerTime= Time.time;

        //enemyADS = GetComponent<AudioSource>();

    }

    void Start()
    {
        //Get PlayerComponent
        player = GameObject.FindGameObjectWithTag("Player").transform;

        //Enemy Start Speed
        agent.speed = patrolSpeed;

        //Spot Angle 
        spotLight.spotAngle = viewAngle ;

        wayPoints = new Vector3[pathHolder.childCount];
        
       //待定
        for (int i=0; i < wayPoints.Length; i++)
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

    public bool CanSeePlayer() {

        if (Vector3.Distance(transform.position,player.position) < viewDistance) {
           
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            float angleBetweenGuardAndPlayer = Vector3.Angle(transform.forward,dirToPlayer);

            if (angleBetweenGuardAndPlayer<viewAngle/2f)  
            {
                if (!Physics.Linecast(transform.position,player.position,viewMask))
                {
                    //is in Grass Check
                    if (playerController.isInGrass && playerController.isCrouching)
                    {
                        return false;
                    }
                    return true;
                }
            }
        }
        return false;
    }

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
        currentAlertLevel = Mathf.Clamp(currentAlertLevel,0f,maxAlertLevel);
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
        if (Time.time - lastAlertTriggerTime > quietTime )
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
        lastAlertTriggerTime= Time.time;
    }

    void UpdateState()
    {
        switch (currentState)
        {

            case EnemyState.Idle:
                IdleBehavior();
                if (currentAlertLevel==maxAlertLevel)
                {
                    ChangeState(EnemyState.Chase);
                }
                //Idle -> Patrol writed in IdleBehavior
                /* if (stateTimer<=0)
                 {
                     ChangeState(EnemyState.Patrol);
                    stateTimer= 2;
                 }*/
                break;

            case EnemyState.Patrol:
                PatrolBehavior(wayPoints);
                if (currentAlertLevel == maxAlertLevel)
                {
                    ChangeState(EnemyState.Chase);
                }
                break;

            case EnemyState.Chase:
                ChaseBehavior();
                //change it into IEn
/*                if (Vector3.Distance(transform.position, player.position) < attackDistance)
                {
                    ChangeState(EnemyState.Attack);
                }*/
                if (currentAlertLevel < maxAlertLevel || Vector3.Distance(transform.position, player.position) > viewDistance)
                {
                    ChangeState(EnemyState.Search);
                }
                break;

            case EnemyState.Attack:
                AttackBehavior();
                if (EnemyHP <= 0)
                {
                    ChangeState(EnemyState.Die);
                }
                break;

            case EnemyState.Search:
                SearchArea();
                if (currentAlertLevel == maxAlertLevel)
                {
                    ChangeState(EnemyState.Chase);
                }
                break;

            case EnemyState.Return:
                ReturnToStart();
                /*if (Vector3.Distance(transform.position,startPos)<=0.1f)
                {
                    ChangeState(EnemyState.Patrol);
                }*/
                if (Vector3.Distance(transform.position, startPos) < 1f)
                {
                    ChangeState(EnemyState.Idle);
                }


                if (currentAlertLevel == maxAlertLevel)
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
        

        if (currentBehavior!=null)
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
        Vector3 targetWaypoint = wayPoints [targetWaypointIndex];


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
            Gizmos.DrawSphere(waypoint.position,.3f);
            Gizmos.DrawLine(previousPos,waypoint.position);
            previousPos = waypoint.position;
        }
        Gizmos.DrawLine(previousPos,startPos);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position,transform.forward* viewDistance);


        /*Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, soundRadius);*/

    }

    void IdleBehavior() 
    {
        Debug.Log("Idle");
        
        if (stateTimer<idleTime)
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
            if (currentAlertLevel == maxAlertLevel)
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
                ChangeState(EnemyState.Search);
                yield break;
            }
           
            yield return null;

        }
        yield break;

    }


    private void ReturnToStart()
    {
        Debug.Log("Return To Start");
        agent.SetDestination(startPos);
    }


    IEnumerator AttackBehavior() 
    {
        while (true)
        {
            if (Vector3.Distance(transform.position, player.position) < attackDistance)
            {
                if (canAttack)
                {
                    playerHealth.Damage(damageValue);
                    Debug.Log("Attack");
                    //音频
                    //enemyADS.PlayOneShot(attacking);  
                    //先禁用，不然老报错
                    //EnemyHpCtrl();
                    canAttack= false;
                    yield return new WaitForSeconds(2f);
                    canAttack = true;
                }

            }
            yield return null;
        }
    }

    void SearchArea()
    {
       /* if (CanSeePlayer())
        {
            ChangeState(EnemyState.Chase);
            return;
        }*/

        Debug.Log("Search");
            if (searchTimer < searchDuration)
            {
                searchTimer += Time.deltaTime;
                if (!agent.hasPath || agent.remainingDistance < 0.5f)
                {
                    Vector3 randomPoint = RandomNavMeshPoint(lastShownPos, patrolRadius);
                    agent.SetDestination(randomPoint);
                }
            }
            else
            {
                ChangeState(EnemyState.Return);
            }
        
       
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

    //public void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.gameObject.CompareTag("Player"))
    //    {
    //        Instantiate(explosionEffectPreFab, explosionPos.position, Quaternion.identity);
    //        Debug.Log("炸了");
    //    }
    //}

}
