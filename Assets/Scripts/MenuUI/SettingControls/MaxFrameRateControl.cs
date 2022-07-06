using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// SliderControl�� �����ؼ� ISettingControl�� ����
/// </summary>
public class MaxFrameRateControl : SettingControl
{

    public override bool IsChanged
    {
        get
        {
            //������ �ݿø� �Ͽ� �� (settings�� �ݿ��� ���� ���߾�� ��)
            if (Mathf.RoundToInt(sliderControl.slider.value) 
                != GameManager.Mgr.settings.targetFrameRate)
            {
                return true;
            }
            else
                return false;
        }
    }

    public override void ApplyToSettings(ref Settings set)
    {
        if (set == null)
            return;

        //������ �ݿø� �Ͽ� �ݿ�
        set.targetFrameRate = Mathf.RoundToInt(sliderControl.slider.value);
    }

    public override void SetSettingValue(ref Settings set)
    {
        if (set == null)
            return;

        sliderControl.slider.value = set.targetFrameRate;
        
    }
}
