using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;

public class ProjectileSound : MonoBehaviour
{
    [Header("Sound Settings")]
    public float soundIntensity = 8f; // Ĭ������ǿ��
    public float soundRadius = 20f; // ����������Χ
    public float soundDuration = 3f; // ��������ʱ��

    [Header("Collision Settings")]
    public float minTimeBetweenSounds = 0.2f; // ��������֮�����С���(��)
    public float minImpactForce = 1.0f; // ������������С��ײ��
    public bool reduceIntensityOnRepeatedCollisions = true; // ������ײ�Ƿ񽵵�����ǿ��
    public float intensityDecayFactor = 0.8f; // ������ײ������ǿ��˥������

    [Header("Debug")]
    public bool showDebug = true;

    private float lastSoundTime = -999f; // �ϴη���ʱ��
    private int collisionCount = 0; // ��ײ����

    void OnCollisionEnter(Collision collision)
    {
        // ������ײ��
        float impactForce = collision.impulse.magnitude;

        // ����Ƿ���ǽ�������ײ��������С���ʱ�����С��ײ��Ҫ��
        if ((collision.gameObject.CompareTag("Wall") ||
             collision.gameObject.CompareTag("Ground") ||
             collision.gameObject.layer == LayerMask.NameToLayer("Default")) &&
            (Time.time - lastSoundTime >= minTimeBetweenSounds) &&
            (impactForce >= minImpactForce))
        {
            MakeSound(impactForce);
        }
    }

    void MakeSound(float impactForce)
    {
        // �����ϴη���ʱ��
        lastSoundTime = Time.time;
        collisionCount++;

        // ����ʵ������ǿ�ȣ����Ի�����ײ����������ײ������
        float actualIntensity = soundIntensity;

        // ������ײ����������ǿ��
        actualIntensity = Mathf.Clamp(actualIntensity * (impactForce / minImpactForce), 1f, soundIntensity * 2f);

        // ���������������ײ˥�������Ҳ��ǵ�һ����ײ
        if (reduceIntensityOnRepeatedCollisions && collisionCount > 1)
        {
            // ʹ��ָ��˥��
            float decayMultiplier = Mathf.Pow(intensityDecayFactor, collisionCount - 1);
            actualIntensity *= decayMultiplier;
        }

        // ȷ������ǿ�Ȳ�������Сֵ
        actualIntensity = Mathf.Max(actualIntensity, 1f);

        // ��SoundManagerע����������¼�
        SoundManager.Instance.RegisterSoundEvent(transform.position, actualIntensity, soundRadius, soundDuration);

        if (showDebug)
        {
            Debug.Log($"�������� {transform.position} ����������ǿ��: {actualIntensity:F2}����ײ��: {impactForce:F2}����ײ����: {collisionCount}");
            // ���������ﲥ��������Ч������
        }
    }

    void OnDrawGizmosSelected()
    {
        if (showDebug)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, soundRadius);
        }
    }
}