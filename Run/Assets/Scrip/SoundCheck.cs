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
    public float projectileSoundMultiplier = 1.5f; // ������������Ӱ�챶��

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
    /// �������Ƿ��ڸ��� detect if player nearby enemy.
    /// </summary>
    public bool IsPlayerNearby()
    {
        if (playerObj == null) return false;

        float distanceToPlayer = Vector3.Distance(transform.position, playerObj.transform.position);
        float effectiveRadius = alertRadius;

        if (playerController.currentSound >= 5)
        {
            effectiveRadius = alertRadius * 1.5f; // ������ʱ���Ӽ�ⷶΧ
        }
        else if (playerController.currentSound >= 8)
        {
            effectiveRadius = alertRadius * 2f; // �����ǳ���ʱ��һ�����Ӽ�ⷶΧ
        }
        return distanceToPlayer <= effectiveRadius;
    }

    public void CheckForSound(Vector3 soundPosition, float soundIntensity, float soundRadius)
    {
        // ������˵�����Դ�ľ���
        float distanceToSound = Vector3.Distance(transform.position, soundPosition);

        // ���������������Χ��
        if (distanceToSound <= soundRadius)
        {
            // ���ݾ����������Ӱ�죨Խ��Ӱ��Խ��
            float distanceRatio = 1.0f - (distanceToSound / soundRadius);
            float alertIncrease = soundIntensity * distanceRatio * projectileSoundMultiplier;

            // ���Ӿ���ֵ
            enemyCtrl.currentAlertLevel += alertIncrease;

            // ���Ӧ�õ�������
            if (shouldInvestigateProjectileSounds)
            {
                // ֪ͨ����ȥ��������λ��
                enemyCtrl.InvestigateSound(soundPosition);
            }

            Debug.Log($"�����������������룺{distanceToSound:F2}������ֵ���ӣ�{alertIncrease:F2}");
        }
    }

    void OnDrawGizmosSelected()
    {
        // ���Ƽ�ⷶΧ������
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, alertRadius);
    }




}
