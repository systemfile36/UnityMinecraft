using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Player�� EditDelay�� �����Ͽ� UI�� ǥ���Ѵ�.
/// </summary>
public class DelayIndicatorControl : MonoBehaviour
{
    /// <summary>
    /// DelayIndicator�� �̹��� ������Ʈ
    /// </summary>
    public Image delayIndicator;

    /// <summary>
    /// ������ ������ ���� �÷��̾�
    /// </summary>
    public StarterAssets.FirstPersonController player;

    void Update()
    {
        //���� EditDelay�� 0�̻��̶��
        //== �� ��ġ�� ����Ǿ� ��Ÿ Ÿ���� �ʱ�ȭ �� �� �پ��� �ִٸ�
        if(player.EditDelay >= 0)
        {
            //������ ǥ�ñ⸦ ���� �ð� ��ŭ ���� �����Ͽ� ä���.
            //EditDelay�� �پ����� 0�� ����� ����.
            delayIndicator.fillAmount = Mathf.Lerp(0, 1, 
                player.EditDelay/GameManager.Mgr.settings.EditDelay);

            //���밡 ���� ���� ���� ���� ����
            if(player.EditDelay < Time.deltaTime)
                delayIndicator.fillAmount = 0;
        }
    }
}
