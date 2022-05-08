using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Player의 EditDelay를 참조하여 UI에 표시한다.
/// </summary>
public class DelayIndicatorControl : MonoBehaviour
{
    /// <summary>
    /// DelayIndicator의 이미지 컴포넌트
    /// </summary>
    public Image delayIndicator;

    /// <summary>
    /// 딜레이 참조를 위한 플레이어
    /// </summary>
    public StarterAssets.FirstPersonController player;

    void Update()
    {
        //만약 EditDelay가 0이상이라면
        //== 블럭 설치가 수행되어 델타 타임이 초기화 된 후 줄어들고 있다면
        if(player.EditDelay >= 0)
        {
            //딜레이 표시기를 남은 시간 만큼 선형 보간하여 채운다.
            //EditDelay가 줄어들수록 0에 가까워 진다.
            delayIndicator.fillAmount = Mathf.Lerp(0, 1, 
                player.EditDelay/GameManager.Mgr.settings.EditDelay);

            //막대가 조금 남는 것을 막기 위함
            if(player.EditDelay < Time.deltaTime)
                delayIndicator.fillAmount = 0;
        }
    }
}
