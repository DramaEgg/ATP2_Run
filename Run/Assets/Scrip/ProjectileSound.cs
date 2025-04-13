using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;

public class ProjectileSound : MonoBehaviour
{
    [Header("Sound Settings")]
    public float soundIntensity = 8f; // 默认很大的声音
    public float soundRadius = 20f; // 声音传播范围
    public float soundDuration = 3f; // 声音持续时间

    [Header("Debug")]
    public bool showDebug = true;

    private bool soundActivated = false;

    void OnCollisionEnter(Collision collision)
    {
        // 当第一次碰撞时激活声音
        if (!soundActivated)
        {
            // 碰撞到墙或地面才发出声音
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

        // 向SoundManager注册这个声音事件
        SoundManager.Instance.RegisterSoundEvent(transform.position, soundIntensity, soundRadius, soundDuration);

        if (showDebug)
        {
            Debug.Log($"发射物在 {transform.position} 发出声音，强度: {soundIntensity}");
            // 可以在这里播放声音特效或粒子
        }
    }

    void OnDrawGizmosSelected()
    {
        //if (showDebug && soundActivated)
        
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, soundRadius);
        
    }
}
