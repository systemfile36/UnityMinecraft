using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BGMVolumeControl : SettingControl
{
    public override bool IsChanged
    {
        get
        {
            //부동 소수점 비교를 위하여 근사값을 사용한다.
            //퍼센티지인 slider.value를 0 ~ 1사이의 값으로 변환 후 비교
            if (Mathf.Approximately(
                (float)Math.Round(sliderControl.slider.value / 100, 2),
                GameManager.Mgr.settings.bgmVolume))
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

        //100으로 나누고 2의 자리에서 반올림 한 후, 0 ~ 1로 범위를 고정한다.
        set.bgmVolume = Mathf.Clamp01((float)Math.Round(sliderControl.slider.value / 100, 2));
    }

    public override void SetSettingValue(ref Settings set)
    {
        if (set == null)
            return;

        sliderControl.slider.value = set.bgmVolume * 100;
    }
}
