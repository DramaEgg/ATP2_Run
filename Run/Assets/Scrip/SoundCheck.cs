using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundCheck : MonoBehaviour
{
    public GameObject playerObj;
    public ThirdPersonController playerController; 
    
    [Header("Alert")]
    public EnemyCtrl enemyCtrl;

    [Header("Sound Check")]
    public float alertRadius;

    void Start()
    {
        playerObj = GameObject.FindWithTag("Player");
        playerController = playerObj.GetComponent<ThirdPersonController>();
    }

    private void Update()
    {
        if (IsPlayerNearby())
        {
            if (playerController.currentSound >= 3)
            {
                enemyCtrl.currentAlertLevel += 1 * Time.deltaTime;
                enemyCtrl.playerDetected();
                Debug.Log("currentAlertLevel +1");
            }
            else if (playerController.currentSound == 5)
            {
                enemyCtrl.playerDetected();
                enemyCtrl.currentAlertLevel += 3 * Time.deltaTime;
                Debug.Log("Sound 5");
            }
            else if (playerController.currentSound == 8)
            {
                enemyCtrl.playerDetected();
                enemyCtrl.currentAlertLevel += 5 * Time.deltaTime;
                Debug.Log("Sound 8");
            }
        }
    }
    /// <summary>
    /// 检测玩家是否在附近 detect if player nearby enemy.
    /// </summary>
    public bool IsPlayerNearby()
    {

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, alertRadius);

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
        Gizmos.DrawWireSphere(transform.position, alertRadius);
    }


    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        Debug.Log("Player In ");

    //        if (playerController.currentSound >= 3)
    //        {
    //            enemyCtrl.currentAlertLevel += 1 * Time.deltaTime;
    //            enemyCtrl.playerDetected();
    //            Debug.Log("currentAlertLevel +1");
    //        }
    //    }
    //}

    //void OnTriggerStay(Collider other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        Debug.Log("Player In ");
    //        if (playerController.currentSound == 5)
    //        {
    //            enemyCtrl.playerDetected();
    //            enemyCtrl.currentAlertLevel += 3 * Time.deltaTime;
    //            Debug.Log("Sound 5");
    //        }
    //        else if (playerController.currentSound == 8)
    //        {
    //            enemyCtrl.playerDetected();
    //            enemyCtrl.currentAlertLevel += 5 * Time.deltaTime;
    //            Debug.Log("Sound 8");
    //        }
    //    }
    //}


}
