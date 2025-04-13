using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // 单例实例
    public static SoundManager Instance;

    // 存储当前活跃的声音事件
    private List<SoundEvent> activeSoundEvents = new List<SoundEvent>();

    // 声音事件类
    public class SoundEvent
    {
        public Vector3 position;
        public float intensity;
        public float radius;
        public float duration;
        public float timeRemaining;

        public SoundEvent(Vector3 pos, float intense, float rad, float dur)
        {
            position = pos;
            intensity = intense;
            radius = rad;
            duration = dur;
            timeRemaining = dur;
        }
    }

    void Awake()
    {
        // 实现单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // 更新声音事件计时器，移除过期的声音事件
        for (int i = activeSoundEvents.Count - 1; i >= 0; i--)
        {
            activeSoundEvents[i].timeRemaining -= Time.deltaTime;

            if (activeSoundEvents[i].timeRemaining <= 0)
            {
                activeSoundEvents.RemoveAt(i);
            }
        }
    }

    // 注册新的声音事件
    public void RegisterSoundEvent(Vector3 position, float intensity, float radius, float duration)
    {
        SoundEvent newEvent = new SoundEvent(position, intensity, radius, duration);
        activeSoundEvents.Add(newEvent);

        // 通知所有敌人检查这个新声音
        NotifyEnemiesOfSound(newEvent);
    }

    // 通知所有敌人有新声音
    private void NotifyEnemiesOfSound(SoundEvent soundEvent)
    {
        // 查找所有带有SoundCheck组件的敌人
        SoundCheck[] allEnemySoundChecks = FindObjectsOfType<SoundCheck>();

        foreach (SoundCheck enemyCheck in allEnemySoundChecks)
        {
            // 调用敌人的声音检测方法
            enemyCheck.CheckForSound(soundEvent.position, soundEvent.intensity, soundEvent.radius);
        }
    }

    void OnDrawGizmos()
    {
        // 可视化所有活跃的声音事件
        foreach (SoundEvent soundEvent in activeSoundEvents)
        {
            Gizmos.color = new Color(0, 1, 1, 0.3f); // 半透明青色
            Gizmos.DrawSphere(soundEvent.position, soundEvent.radius);
        }
    }
}