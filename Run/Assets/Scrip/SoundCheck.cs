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
    public float projectileSoundMultiplier = 1.5f; // 发射物声音的影响倍率

    [Header("Investigation")]
    public bool shouldInvestigateProjectileSounds = true;


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
                enemyCtrl.currentAlertLevel += 3 * Time.deltaTime;
                enemyCtrl.playerDetected();
                Debug.Log("currentAlertLevel +1");
            }
            else if (playerController.currentSound == 5)
            {
                enemyCtrl.playerDetected();
                enemyCtrl.currentAlertLevel += 4 * Time.deltaTime;
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
        if (playerObj == null) return false;

        float distanceToPlayer = Vector3.Distance(transform.position, playerObj.transform.position);
        float effectiveRadius = alertRadius;

        if (playerController.currentSound >= 5)
        {
            effectiveRadius = alertRadius * 1.5f; // 声音大时增加检测范围
        }
        else if (playerController.currentSound >= 8)
        {
            effectiveRadius = alertRadius * 2f; // 声音非常大时进一步增加检测范围
        }
        return distanceToPlayer <= effectiveRadius;
    }

    public void CheckForSound(Vector3 soundPosition, float soundIntensity, float soundRadius)
    {
        // 计算敌人到声音源的距离
        float distanceToSound = Vector3.Distance(transform.position, soundPosition);

        // 如果敌人在声音范围内
        if (distanceToSound <= soundRadius)
        {
            // 根据距离计算声音影响（越近影响越大）
            float distanceRatio = 1.0f - (distanceToSound / soundRadius);
            float alertIncrease = soundIntensity * distanceRatio * projectileSoundMultiplier;

            // 增加警戒值
            enemyCtrl.currentAlertLevel += alertIncrease;

            // 如果应该调查声音
            if (shouldInvestigateProjectileSounds)
            {
                // 通知敌人去调查声音位置
                enemyCtrl.InvestigateSound(soundPosition);
            }

            Debug.Log($"敌人听到声音！距离：{distanceToSound:F2}，警戒值增加：{alertIncrease:F2}");
        }
    }

    void OnDrawGizmosSelected()
    {
        // 绘制检测范围的球体
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, alertRadius);
    }




}
