using Cinemachine;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Control")]
    public float maxHp = 10f;
    public float currentHp;
    public bool isDead = false;

    public Slider hpSlider;

    public ThirdPersonController personController;


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
    }

    public void Damage(float damage)
    {
        currentHp -= damage;

        hpSlider.value = currentHp;

        Impulse.GenerateImpulse();


        //personController.animator.SetBool("isHurt", true);

        //personController.jumpAd.PlayOneShot(personController.hurt);

        if (currentHp <= 0)
        {
            PlayerDie();
        }
    }

    void PlayerDie()
    {
        isDead = true;
        Debug.Log("PlayerDie");
        
        //最后再开，用于跳转Fail 界面的
        //personController.animator.Play("PlayerDie");
        //StartCoroutine(windowsMgr.FailedGame());
    }


}
