using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ManagerEnemyCtrl : MonoBehaviour
{
    private float moveSpeed = 4f;
    private float chargingSpeed = 1f;
    private float sprintSpeed = 7f;
    public float currentSpeed = 0f;


    private float attackDistance = 1f;
    private float chargingDistance = 3f;
    public float damageValue = 2f;
    private bool canAttack = true;

    public bool isSlow = false;
    private float slowSpeed = 1f;

    [Header("Health Ctrl")]
    public bool isDead = false;

    private Transform lastShownPos;
    private Transform player;
    private Vector3 direction;
    private Rigidbody2D rb;

    private PlayerHealth playerHP;
    private Animator animator;

    // Get the Script from Game Object
    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Transform>();
        playerHP = GameObject.Find("Player").GetComponent<PlayerHealth>();
        animator = GetComponent<Animator>();

        rb = GetComponent<Rigidbody2D>();
        currentSpeed = moveSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        //I didn't write the Enemy Dead Function, but I leave the bool as the switch incase i will edit it.
        if (!isDead)
        {
            SpeedCtrl();
            UpdateDirection();
            ChaseBehavior();
        }
    }

    void UpdateDirection()
    {
        direction = (player.position - transform.position).normalized;
    }


    void ChaseBehavior()
    {
        rb.velocity = new Vector3(direction.x, direction.y * currentSpeed);
        animator.SetTrigger("isChase");
    }

    IEnumerator AttackBehavior()
    {
        while (Vector2.Distance(transform.position, player.position) < attackDistance)
        {
            if (canAttack)
            {
                playerHP.Damage(damageValue);
                Debug.Log("Attack");
                //enemyADS.PlayOneShot(attacking);
                //EnemyHpCtrl();
                animator.SetTrigger("isBiet");
                canAttack = false;
                yield return new WaitForSeconds(2f);
                canAttack = true;
            }
            yield return null;
        }
    }

    void SpeedCtrl()
    {
        //when too far from the player, speed faster, else normal speed.
        if (Vector2.Distance(transform.position, player.position) > 10f)
        {
            currentSpeed = sprintSpeed;
        }
        else if (Vector2.Distance(transform.position, player.position) < 3f)
        {
            currentSpeed = moveSpeed;
        }

        //if it is close enough, attack the player.
        if (Vector2.Distance(transform.position, player.position) < attackDistance && canAttack)
        {
            Debug.Log("Yes");
            StartCoroutine(AttackBehavior());
        }

        // if Player ink and hit the Shark, Shark speed = slowSpeed
        if (isSlow)
        {
            currentSpeed = slowSpeed;
        }
    }

    public void ApplySlow()
    {
        StartCoroutine(SlowSpeed());
    }

    public IEnumerator SlowSpeed()
    { 
        isSlow = true;
        animator.SetTrigger("isHurt");
        yield return new WaitForSeconds(1.5f); // if the shark is attacked by the ink, then its speed turn slow in 1.5s 
        isSlow= false;
    }

}
