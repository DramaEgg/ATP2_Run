using Cinemachine;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Control")]
    public float maxHp = 10f;
    public float currentHp;
    public bool isDead = false;

    public Slider hpSlider;

    public ThirdPersonController personController;

    [Header("Rehealing- һ��ʱ��û���˺���Զ���Ѫ")]
    float currentTimer = 0;
    float targetTimer = 15f;


    [Header("Screen Shake")]
    public CinemachineImpulseSource Impulse;


    // Start is called before the first frame update
    void Start()
    {
        //HP
        currentHp = maxHp;
        hpSlider.maxValue = maxHp;
        hpSlider.value = currentHp;

        Impulse = GetComponent<CinemachineImpulseSource>();
    }

    // Update is called once per frame
    void Update()
    {
        Rehealing();
        hpSlider.value = currentHp;

    }

    public void Damage(float damage)
    {
        currentHp -= damage;


        Impulse.GenerateImpulse();


        //personController.animator.SetBool("isHurt", true);

        //personController.jumpAd.PlayOneShot(personController.hurt);

        if (currentHp <= 0)
        {
            PlayerDie();
        }
    }

    public void Rehealing()
    {
        if ( currentHp < maxHp ) //������˲�ִ��,��ʱû��ӵ��˵���һ�ι���ʱ��
        {
            //��ʼ��ʱ
            currentTimer += Time.deltaTime;
            if ( currentTimer >= targetTimer )
            {
                currentHp += 1 * Time.deltaTime;

            }
        }
        else
        {
            currentTimer= 0;
        }
    }

    void PlayerDie()
    {
        isDead = true;
        Debug.Log("PlayerDie");
        
        //����ٿ���������תFail �����
        //personController.animator.Play("PlayerDie");
        //StartCoroutine(windowsMgr.FailedGame());
    }


}
