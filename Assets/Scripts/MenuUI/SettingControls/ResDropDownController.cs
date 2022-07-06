using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Resolution�� DropDown�� ��Ʈ�� �Ѵ�.
/// </summary>
public class ResDropDownController : SettingControl
{
    private TMP_Dropdown dropDown;

    public FullScreenControl fullScreenControl;

    /// <summary>
    /// �ػ� DropDown�� ǥ�õǴ� �ؽ�Ʈ�� ����
    /// {0}x{1} �տ� �ٴ´�.
    /// </summary>
    private string captionFormat = "Resolution : ";


    /// <summary>
    /// ���� ���������� ����� �� ����Ǿ��� �� ����
    /// </summary>
    public override bool IsChanged 
    { 
        get 
        {
            string currentSavedRes = GameManager.Mgr.settings.resolution.ToString();

            //���� ��Ʈ�� ���� ���� ���ð��� �ٸ��� true�� ��ȯ
            if (!currentSavedRes.Equals(dropDown.captionText.text))
                return true;
            else
                return false;
        } 
    }

    protected override void Awake()
    {
        if (dropDown != null)
            //�� ����� �ػ� �ݿ� ����
            dropDown.onValueChanged.AddListener(ChangeResolution);
    }

    public override void InitControl()
    {
        if(dropDown == null)
            dropDown = GetComponent<TMP_Dropdown>();

        if(dropDown != null)
            //�� ����� �ػ� �ݿ� ����
            dropDown.onValueChanged.AddListener(ChangeResolution);

        //captionText�� �ʱⰪ���� ���� (��Ȱ��ȭ ���¿��� �ʱ�ȭ ���� �ʱ� ����)
        dropDown.captionText.text = dropDown.options[dropDown.value].text;
    }

    /// <summary>
    /// ���� ���ô�� �ػ󵵸� ������
    /// ���� ����Ǹ� �ػ� �ݿ�
    /// </summary>
    private void ChangeResolution(int value)
    {
        try
        {
            //���ڿ��� �ػ󵵸� �ɰ���.
            var res = GetSettingValue().Split('x');

            int width = System.Convert.ToInt32(res[0]);
            int height = System.Convert.ToInt32(res[1]);

            Debug.Log($"{width}x{height}");

            //���� ������ �ػ󵵿� Ǯ��ũ�� ���ο� ���� �ػ� ����
            Screen.SetResolution(width, height, fullScreenControl.IsFullScreen);

        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);
        }
        
    }

    /// <summary>
    /// �ػ󵵸� ���ڿ��� ��ȯ�Ѵ�.
    /// </summary>
    /// <returns></returns>
    public override string GetSettingValue()
    {
        string res = dropDown.captionText.text.Split(':')[1];
        return res;
    }

    /// <summary>
    /// {0}x{1}�� �������� �ػ󵵸� �޾Ƽ� ��Ӵٿ��� ������
    /// �׿� ���ÿ� ���� �ػ󵵵� ������
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    public override void SetSettingValue(string value)
    {
        //�Ű������� ���Ŀ� �°� ����
        string item = captionFormat + value;

        //DropDown�� ������ Option�� List�� �޾ƿ�
        var options = dropDown.options;
        
        //options ����Ʈ�� ���� �ݺ�
        for(int i = 0; i < options.Count; i++)
        {
            //���� ���� �ݺ����� �ɼ��� �ؽ�Ʈ�� item�� ��ġ�Ѵٸ�
            if(item.Equals(options[i].text))
            {
                //�ش��ϴ� �ε����� value ����
                //���� �ٲ�����Ƿ� �ػ� ����� ����
                dropDown.value = i;

                break;
            }
        }

        return;
    }

    public override void ApplyToSettings(ref Settings set)
    {
        if (set == null)
            return;

        try
        {
            //�ػ� �ؽ�Ʈ�� �����ڸ� �������� �ڸ���.
            var res = GetSettingValue().Split('x');

            if (set.resolution != null)
            {
                set.resolution.width = System.Convert.ToInt32(res[0]);
                set.resolution.height = System.Convert.ToInt32(res[1]);
            }
            else
            {
                set.resolution =
                    new Resolution_S(System.Convert.ToInt32(res[0]), System.Convert.ToInt32(res[1]));
            }
        }
        //�ε��� ������ ��ȯ ���� ���ܸ� �����
        catch(System.Exception ex)
        {
            Debug.LogException(ex);
        }
        
    }

    public override void SetSettingValue(ref Settings set)
    {
        if (set == null)
            return;

        //������ ���� Settings�� �ػ󵵸� {0}x{1} �������� �ٲپ�
        //SetSettingValue(string)�� ȣ��
        SetSettingValue(set.resolution.ToString());
    }
}

