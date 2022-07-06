using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewDistanceControl : SettingControl
{
    public override bool IsChanged
    {
        get
        {
            //정수로 반올림하여 비교
            if (Mathf.RoundToInt(sliderControl.slider.value) 
                != GameManager.Mgr.settings.ViewDistanceInChunks)
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

        set.ViewDistanceInChunks = Mathf.RoundToInt(sliderControl.slider.value);
    }

    public override void SetSettingValue(ref Settings set)
    {
        if (set == null)
            return;

        sliderControl.slider.value = set.ViewDistanceInChunks;
    }
}
