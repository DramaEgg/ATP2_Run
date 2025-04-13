using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // ����ʵ��
    public static SoundManager Instance;

    // �洢��ǰ��Ծ�������¼�
    private List<SoundEvent> activeSoundEvents = new List<SoundEvent>();

    // �����¼���
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
        // ʵ�ֵ���ģʽ
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
        // ���������¼���ʱ�����Ƴ����ڵ������¼�
        for (int i = activeSoundEvents.Count - 1; i >= 0; i--)
        {
            activeSoundEvents[i].timeRemaining -= Time.deltaTime;

            if (activeSoundEvents[i].timeRemaining <= 0)
            {
                activeSoundEvents.RemoveAt(i);
            }
        }
    }

    // ע���µ������¼�
    public void RegisterSoundEvent(Vector3 position, float intensity, float radius, float duration)
    {
        SoundEvent newEvent = new SoundEvent(position, intensity, radius, duration);
        activeSoundEvents.Add(newEvent);

        // ֪ͨ���е��˼�����������
        NotifyEnemiesOfSound(newEvent);
    }

    // ֪ͨ���е�����������
    private void NotifyEnemiesOfSound(SoundEvent soundEvent)
    {
        // �������д���SoundCheck����ĵ���
        SoundCheck[] allEnemySoundChecks = FindObjectsOfType<SoundCheck>();

        foreach (SoundCheck enemyCheck in allEnemySoundChecks)
        {
            // ���õ��˵�������ⷽ��
            enemyCheck.CheckForSound(soundEvent.position, soundEvent.intensity, soundEvent.radius);
        }
    }

    void OnDrawGizmos()
    {
        // ���ӻ����л�Ծ�������¼�
        foreach (SoundEvent soundEvent in activeSoundEvents)
        {
            Gizmos.color = new Color(0, 1, 1, 0.3f); // ��͸����ɫ
            Gizmos.DrawSphere(soundEvent.position, soundEvent.radius);
        }
    }
}