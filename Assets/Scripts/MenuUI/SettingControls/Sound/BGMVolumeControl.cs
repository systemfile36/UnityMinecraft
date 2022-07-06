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
            //�ε� �Ҽ��� �񱳸� ���Ͽ� �ٻ簪�� ����Ѵ�.
            //�ۼ�Ƽ���� slider.value�� 0 ~ 1������ ������ ��ȯ �� ��
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

        //100���� ������ 2�� �ڸ����� �ݿø� �� ��, 0 ~ 1�� ������ �����Ѵ�.
        set.bgmVolume = Mathf.Clamp01((float)Math.Round(sliderControl.slider.value / 100, 2));
    }

    public override void SetSettingValue(ref Settings set)
    {
        if (set == null)
            return;

        sliderControl.slider.value = set.bgmVolume * 100;
    }
}
