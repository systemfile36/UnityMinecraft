using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EditDelayControl : SettingControl
{
    public override bool IsChanged
    {
        get
        {
            //부동 소수점 비교를 위하여 근사값을 사용한다.
            if (Mathf.Approximately(
                (float)Math.Round(sliderControl.slider.value, 2),
                GameManager.Mgr.settings.EditDelay))
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

        //1의 자리에서 반올림
        set.EditDelay = (float)Math.Round(sliderControl.slider.value, 2);
    }

    public override void SetSettingValue(ref Settings set)
    {
        if (set == null)
            return;

        sliderControl.slider.value = set.EditDelay;
    }
}
