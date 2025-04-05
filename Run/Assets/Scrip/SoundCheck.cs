using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundCheck : MonoBehaviour
{
    public GameObject playerObj;
   // public ThirdPersonController playerController; 
    
    [Header("Alert")]
    public EnemyCtrl enemyCtrl;

    [Header("Sound Check")]
    public float soundRadius;

    void Start()
    {
        playerObj = GameObject.FindWithTag("Player");
        //playerController = playerObj.GetComponent<ThirdPersonController>();
        enemyCtrl = GameObject.FindWithTag("Enemy001").GetComponent<EnemyCtrl>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player In ");
            //if (playerController.currentSound >=3 )
            //{
            //    enemyCtrl.currentAlertLevel += 1 * Time.deltaTime;
            //    enemyCtrl.playerDetected();
            //    Debug.Log("currentAlertLevel +1");
            //}
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player In ");
            //if (playerController.currentSound == 5)
            //{
            //    enemyCtrl.playerDetected();
            //    enemyCtrl.currentAlertLevel += 3 * Time.deltaTime;
            //    Debug.Log("Sound 5");
            //}
            //else if (playerController.currentSound == 8)
            //{
            //    enemyCtrl.playerDetected();
            //    enemyCtrl.currentAlertLevel += 5 * Time.deltaTime;
            //    Debug.Log("Sound 8");
            //}
            //else if (playerController.currentSound ==playerController.whistleSound)
            //{
            //    enemyCtrl.playerDetected();
            //    enemyCtrl.currentAlertLevel = enemyCtrl.maxAlertLevel;
            //    Debug.Log("Whistle");
            //}
        }
    }
}
