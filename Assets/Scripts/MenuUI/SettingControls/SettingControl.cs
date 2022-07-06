using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��� ���� ��Ʈ�ѵ��� ����ؾ��� �߻� Ŭ����
/// �ʿ��� �޼ҵ���� �߻� �޼ҵ�� ����Ǿ� �ִ�.
/// </summary>
public abstract class SettingControl : MonoBehaviour
{
    public SliderControl sliderControl = null;

    /// <summary>
    /// InitControl�� ȣ����
    /// </summary>
    protected virtual void Awake()
    {
        //��Ʈ�� �ʱ�ȭ
        InitControl();
    }

    //��Ȱ��ȭ�� ������Ʈ�� Awake �޽����� ȣ����� �����Ƿ� ���� ���� �ʱ�ȭ �޼ҵ�
    /// <summary>
    /// �Ҵ�� ��Ʈ���� �ʱ�ȭ �ϴ� �޼ҵ�, 
    /// �⺻���δ� sliderControl ������Ʈ�� �޾ƿ��� �Ǿ� �ִ�.
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
    /// ���� ���� �ε�� Setting�� ���Ͽ� 
    /// ����Ǿ����� üũ�Ѵ�.
    /// </summary>
    public abstract bool IsChanged { get; }

    /// <summary>
    /// Settings Ŭ������ ������ �޾� �� ��Ʈ�ѿ� �´� ����� 
    /// ���� �Ҵ��Ѵ�.
    /// </summary>
    public abstract void ApplyToSettings(ref Settings set);

    /// <summary>
    /// sliderControl�� ���˰� ������ ���� ���� ���ڿ��� ��ȯ
    /// </summary>
    /// <returns></returns>
    public virtual string GetSettingValue()
    {
        return string.Format(sliderControl.formatStr, sliderControl.slider.value);
    }

    /// <summary>
    /// Settings�� ������ �޾� ��Ʈ�ѿ� �� �ݿ�
    /// </summary>
    /// <param name="set"></param>
    public abstract void SetSettingValue(ref Settings set);

    /// <summary>
    /// ���ڷ� ���� string ������ value�� 
    /// float�� ��ȯ�Ͽ� sliderControl�� �ݿ��Ѵ�.
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
