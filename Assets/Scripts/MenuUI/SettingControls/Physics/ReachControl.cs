using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ReachControl : SettingControl
{

    public override bool IsChanged
    {
        get
        {
            //�ε� �Ҽ��� �񱳸� ���Ͽ� �ٻ簪�� ����Ѵ�.
            if (Mathf.Approximately(
                (float)Math.Round(sliderControl.slider.value, 1),
                GameManager.Mgr.settings.reach))
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

        //1�� �ڸ����� �ݿø�
        set.reach = (float)Math.Round(sliderControl.slider.value, 1);
    }

    public override void SetSettingValue(ref Settings set)
    {
        if (set == null)
            return;

        sliderControl.slider.value = set.reach;
    }
}