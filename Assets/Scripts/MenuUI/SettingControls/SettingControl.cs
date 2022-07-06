using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 설정 컨트롤들이 상속해야할 추상 클래스
/// 필요한 메소드들이 추상 메소드로 선언되어 있다.
/// </summary>
public abstract class SettingControl : MonoBehaviour
{
    public SliderControl sliderControl = null;

    /// <summary>
    /// InitControl을 호출함
    /// </summary>
    protected virtual void Awake()
    {
        //컨트롤 초기화
        InitControl();
    }

    //비활성화된 오브젝트의 Awake 메시지는 호출되지 않으므로 따로 만든 초기화 메소드
    /// <summary>
    /// 할당된 컨트롤을 초기화 하는 메소드, 
    /// 기본으로는 sliderControl 컴포넌트를 받아오게 되어 있다.
    /// </summary>
    public virtual void InitControl()
    {
        if (sliderControl == null)
        {
            sliderControl = GetComponent<SliderControl>();
            if (sliderControl == null)
                Debug.LogError($"{transform.name} : missing SliderControl Component");
        }
    }

    /// <summary>
    /// 값을 현재 로드된 Setting과 비교하여 
    /// 변경되었는지 체크한다.
    /// </summary>
    public abstract bool IsChanged { get; }

    /// <summary>
    /// Settings 클래스를 참조로 받아 각 컨트롤에 맞는 멤버에 
    /// 값을 할당한다.
    /// </summary>
    public abstract void ApplyToSettings(ref Settings set);

    /// <summary>
    /// sliderControl의 포맷과 값으로 세팅 값을 문자열로 반환
    /// </summary>
    /// <returns></returns>
    public virtual string GetSettingValue()
    {
        return string.Format(sliderControl.formatStr, sliderControl.slider.value);
    }

    /// <summary>
    /// Settings를 참조로 받아 컨트롤에 값 반영
    /// </summary>
    /// <param name="set"></param>
    public abstract void SetSettingValue(ref Settings set);

    /// <summary>
    /// 인자로 받은 string 형태의 value를 
    /// float로 변환하여 sliderControl에 반영한다.
    /// </summary>
    /// <param name="value"></param>
    public virtual void SetSettingValue(string value)
    {
        try
        {
            sliderControl.slider.value = float.Parse(value);
        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    
}
