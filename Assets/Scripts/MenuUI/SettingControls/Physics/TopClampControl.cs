using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TopClampControl : SettingControl
{

    public override bool IsChanged
    {
        get
        {
            //������ ��ȯ�Ͽ� ���Ѵ�.
            if (Mathf.RoundToInt(sliderControl.slider.value)
                != Mathf.RoundToInt(GameManager.Mgr.settings.TopClamp))
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

        //������ �ݿø�
        set.TopClamp = Mathf.Round(sliderControl.slider.value);
    }

    public override void SetSettingValue(ref Settings set)
    {
        if (set == null)
            return;

        sliderControl.slider.value = set.TopClamp;
    }
}
