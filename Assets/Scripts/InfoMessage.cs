using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoMessage : MonoBehaviour
{
    //출력할 안내 메시지가 보관되는 Queue
    private static ConcurrentQueue<string> infoMessages 
        = new ConcurrentQueue<string>();

    //0.1초 대기한다.
    private readonly WaitForSecondsRealtime waitForSeconds 
        = new WaitForSecondsRealtime(0.1f);

    //Animator 컴포넌트에 대한 참조
    [SerializeField]
    private Animator animator;

    //Text 컴포넌트에 대한 참조
    [SerializeField]
    private Text infoText;

    void Awake()
    {
        StartCoroutine(PrintMessages());
    }

    void OnDisable()
    {
        StopCoroutine(PrintMessages());
    }

    /// <summary>
    /// infoMessages에 메시지를 추가한다.
    /// </summary>
    public void AddMessages(string str)
    {
        
        infoMessages.Enqueue(str);
    }

    /// <summary>
    /// 0.3초 간격으로 메시지를 체크해서 출력한다.
    /// </summary>
    IEnumerator PrintMessages()
    {
        while(true)
        {
            string message;
            if(infoMessages.TryDequeue(out message))
            {
                //텍스트를 세팅한 뒤 Appear트리거를 set하여
                //애니메이션을 재생한다.
                infoText.text = message;
                animator.SetTrigger("Appear");
            }

            //0.1초 기다린다.
            yield return waitForSeconds;
        }
    }

}
