using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;

public class ProjectileSound : MonoBehaviour
{
    [Header("Sound Settings")]
    public float soundIntensity = 8f; // 默认声音强度
    public float soundRadius = 20f; // 声音传播范围
    public float soundDuration = 3f; // 声音持续时间

    [Header("Collision Settings")]
    public float minTimeBetweenSounds = 0.2f; // 两次声音之间的最小间隔(秒)
    public float minImpactForce = 1.0f; // 触发声音的最小碰撞力
    public bool reduceIntensityOnRepeatedCollisions = true; // 连续碰撞是否降低声音强度
    public float intensityDecayFactor = 0.8f; // 连续碰撞的声音强度衰减因子

    [Header("Debug")]
    public bool showDebug = true;

    private float lastSoundTime = -999f; // 上次发声时间
    private int collisionCount = 0; // 碰撞计数

    void OnCollisionEnter(Collision collision)
    {
        // 计算碰撞力
        float impactForce = collision.impulse.magnitude;

        // 检查是否与墙或地面碰撞且满足最小间隔时间和最小碰撞力要求
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
        // 更新上次发声时间
        lastSoundTime = Time.time;
        collisionCount++;

        // 计算实际声音强度（可以基于碰撞力和连续碰撞次数）
        float actualIntensity = soundIntensity;

        // 根据碰撞力调整声音强度
        actualIntensity = Mathf.Clamp(actualIntensity * (impactForce / minImpactForce), 1f, soundIntensity * 2f);

        // 如果启用了连续碰撞衰减，并且不是第一次碰撞
        if (reduceIntensityOnRepeatedCollisions && collisionCount > 1)
        {
            // 使用指数衰减
            float decayMultiplier = Mathf.Pow(intensityDecayFactor, collisionCount - 1);
            actualIntensity *= decayMultiplier;
        }

        // 确保声音强度不低于最小值
        actualIntensity = Mathf.Max(actualIntensity, 1f);

        // 向SoundManager注册这个声音事件
        SoundManager.Instance.RegisterSoundEvent(transform.position, actualIntensity, soundRadius, soundDuration);

        if (showDebug)
        {
            Debug.Log($"发射物在 {transform.position} 发出声音，强度: {actualIntensity:F2}，碰撞力: {impactForce:F2}，碰撞次数: {collisionCount}");
            // 可以在这里播放声音特效或粒子
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