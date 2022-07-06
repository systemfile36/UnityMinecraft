using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FullScreenControl : SettingControl
{
    public Button btn = null;
    public TMP_Text text = null;

    private bool isFullScreen = false;

    /// <summary>
    /// bool ���� ���� ������ �ؽ�Ʈ, 
    /// Ǯ��ũ���� ���ÿ� �����ϴ� ������Ƽ
    /// </summary>
    public bool IsFullScreen
    {
        get
        {
            return isFullScreen;
        }

        set
        {
            isFullScreen = value;

            //����� ������ fullScreen ��� ����
            Screen.fullScreen = isFullScreen;

            SetBtnText();
        }
    }


    private string btnTextFormat = "FullScreen : ";

    protected override void Awake()
    {
        if (btn != null)
            btn.onClick.AddListener(OnClickBtn);
    }

    public override void InitControl()
    {
        if(btn == null)
        {
            btn = GetComponent<Button>();

            if (btn != null)
                btn.onClick.AddListener(OnClickBtn);
        }

        if(text == null)
            //��ư�� �ؽ�Ʈ�� ������
            text = transform.GetChild(0).GetComponent<TMP_Text>();

        //���� ��ũ���� ���´�� ���� ����
        isFullScreen = Screen.fullScreen;

        if (text != null)
            SetBtnText();

    }

    public void OnClickBtn()
    {
        IsFullScreen = !IsFullScreen;
    }

    /// <summary>
    /// IsFullScreen ������ ���� ��ư�� �ؽ�Ʈ ����
    /// </summary>
    private void SetBtnText()
    {
        
        if(IsFullScreen)
        {
            text.text = btnTextFormat + "On";
        }
        else
        {
            text.text = btnTextFormat + "Off";
        }
    }

    public override bool IsChanged
    {
        get
        {
            if (GameManager.Mgr.settings.fullScreen != isFullScreen)
            {
                return true;
            }
            else
                return false;
        }
    }

    public override string GetSettingValue()
    {
        return IsFullScreen.ToString();
    }

    public override void SetSettingValue(string value)
    {
        try
        {

            IsFullScreen = System.Convert.ToBoolean(value);

        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);
        }
        
    }

    public override void ApplyToSettings(ref Settings set)
    {
        if (set == null)
            return;

        set.fullScreen = isFullScreen;
    }

    public override void SetSettingValue(ref Settings set)
    {
        if (set == null)
            return;

        try
        {
            IsFullScreen = set.fullScreen;

        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);
        }
    }
}
