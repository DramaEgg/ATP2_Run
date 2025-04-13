using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;

public class ProjectileSound : MonoBehaviour
{
    [Header("Sound Settings")]
    public float soundIntensity = 8f; // Ĭ�Ϻܴ������
    public float soundRadius = 20f; // ����������Χ
    public float soundDuration = 3f; // ��������ʱ��

    [Header("Debug")]
    public bool showDebug = true;

    private bool soundActivated = false;

    void OnCollisionEnter(Collision collision)
    {
        // ����һ����ײʱ��������
        if (!soundActivated)
        {
            // ��ײ��ǽ�����ŷ�������
            if (collision.gameObject.CompareTag("Wall") ||
                collision.gameObject.CompareTag("Ground") ||
                collision.gameObject.layer == LayerMask.NameToLayer("Default"))
            {
                MakeSound();
            }
        }
    }

    void MakeSound()
    {
        soundActivated = true;

        // ��SoundManagerע����������¼�
        SoundManager.Instance.RegisterSoundEvent(transform.position, soundIntensity, soundRadius, soundDuration);

        if (showDebug)
        {
            Debug.Log($"�������� {transform.position} ����������ǿ��: {soundIntensity}");
            // ���������ﲥ��������Ч������
        }
    }

    void OnDrawGizmosSelected()
    {
        //if (showDebug && soundActivated)
        
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, soundRadius);
        
    }
}
