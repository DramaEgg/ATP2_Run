using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FliterCtrl : MonoBehaviour
{
    public Image fliterImg;
    public AnimationCurve showCurve;
    public AnimationCurve hideCurve;
    public float animationSpeed;

    public float maxAlpha;
    public float minAlpha;
    public float breathingRate;
    bool isBreathing = false;

    private PlayerHealth playerHealth;


    private void Start()
    {
       playerHealth = GameObject.Find("Player").GetComponent<PlayerHealth>();
       SetAlpha(0f);

    }

    public void Update()
    {
        if (playerHealth.currentHp < 4f)
        {
            if(!isBreathing)
            {
                isBreathing = true;
                StartCoroutine("BreathingEffect");
            }
        }
        else
        {
            if (isBreathing)
            {
                StopAllCoroutines();
                StartCoroutine(HideFliter());
                isBreathing = false;
            }
        }
    }

    IEnumerator BreathingEffect()
    { 
        while (true)
        {
            yield return StartCoroutine(ShowFliter(minAlpha, maxAlpha, showCurve));

            yield return StartCoroutine(ShowFliter(maxAlpha, minAlpha, hideCurve));

        }
    }


    IEnumerator ShowFliter(float startAlpha, float targetAlpha, AnimationCurve curve)
    {
        float timer = 0;
        float duration = 1f / breathingRate;
        
        while ( timer < duration )
        {
            float t = timer / duration;
            float curveValue = curve.Evaluate(t);
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, curveValue);

            SetAlpha(alpha);

            timer += Time.deltaTime;
            yield return null;
        }

        SetAlpha(targetAlpha);
    }

    IEnumerator HideFliter()
    {
        Debug.Log("HideFliter协程已启动");

        float startAlpha = fliterImg.color.a;
        float timer = 0;
        float duration = 1f;

        while (timer < duration)
        {
            float t = timer / duration;
            float alpha = Mathf.Lerp(startAlpha, 0f, hideCurve.Evaluate(t));

            SetAlpha(alpha);

            timer += Time.deltaTime * animationSpeed;
            yield return null;
        }

        SetAlpha(0f);
    }

    public void SetAlpha(float alpha)
    { 
        Color color = fliterImg.color;
        color.a = alpha;
        fliterImg.color = color;
    }


}
