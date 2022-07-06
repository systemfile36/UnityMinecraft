using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// SliderControl을 참조해서 ISettingControl을 구현
/// </summary>
public class MaxFrameRateControl : SettingControl
{

    public override bool IsChanged
    {
        get
        {
            //정수로 반올림 하여 비교 (settings에 반영할 때랑 맞추어야 함)
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

        //정수로 반올림 하여 반영
        set.targetFrameRate = Mathf.RoundToInt(sliderControl.slider.value);
    }

    public override void SetSettingValue(ref Settings set)
    {
        if (set == null)
            return;

        sliderControl.slider.value = set.targetFrameRate;
        
    }
}
