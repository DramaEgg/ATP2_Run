using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProjectionCtrl : MonoBehaviour
{
    public GameObject projectionCanvas, loadingContent,coadContent;
    public Slider waitingSlider;

    private float currentTimer = 0f; //�ȴ���ʱ��
    private float waitingTime = 20f; //�ȴ���ʱ��
    private bool isStarted2Wait = false;
    private bool finishedWait = false;

    // Start is called before the first frame update
    void Start()
    {
        projectionCanvas.SetActive(false);
        coadContent.SetActive(false);

        waitingSlider.minValue= 0;
        waitingSlider.maxValue = waitingTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (isStarted2Wait && !finishedWait)
        {
            WaitingCounter();
        }

        waitingSlider.value = currentTimer;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("PlayerIn");
            if (Input.GetKeyDown(KeyCode.F))
            { 
                projectionCanvas.SetActive(true);
                loadingContent.SetActive(true);
                isStarted2Wait = true;
                Debug.Log("��ʼ�ȴ�");
            }
        }
    }

    public void WaitingCounter()
    {
            if (currentTimer < waitingTime )
            {
                currentTimer += Time.deltaTime; //�ȴ���
            }
            else
            {
                currentTimer = waitingTime;

                loadingContent.SetActive(false);
                coadContent.SetActive(true);
                finishedWait = true;  //�����ȴ�
                 Debug.Log("�����ȴ�");
            }   
    }
}
