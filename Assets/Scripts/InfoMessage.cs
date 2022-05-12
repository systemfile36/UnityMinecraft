using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoMessage : MonoBehaviour
{
    //����� �ȳ� �޽����� �����Ǵ� Queue
    private static ConcurrentQueue<string> infoMessages 
        = new ConcurrentQueue<string>();

    //0.1�� ����Ѵ�.
    private readonly WaitForSecondsRealtime waitForSeconds 
        = new WaitForSecondsRealtime(0.1f);

    //Animator ������Ʈ�� ���� ����
    [SerializeField]
    private Animator animator;

    //Text ������Ʈ�� ���� ����
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
    /// infoMessages�� �޽����� �߰��Ѵ�.
    /// </summary>
    public void AddMessages(string str)
    {
        
        infoMessages.Enqueue(str);
    }

    /// <summary>
    /// 0.3�� �������� �޽����� üũ�ؼ� ����Ѵ�.
    /// </summary>
    IEnumerator PrintMessages()
    {
        while(true)
        {
            string message;
            if(infoMessages.TryDequeue(out message))
            {
                //�ؽ�Ʈ�� ������ �� AppearƮ���Ÿ� set�Ͽ�
                //�ִϸ��̼��� ����Ѵ�.
                infoText.text = message;
                animator.SetTrigger("Appear");
            }

            //0.1�� ��ٸ���.
            yield return waitForSeconds;
        }
    }

}
