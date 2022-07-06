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
    /// bool 값을 통해 변수와 텍스트, 
    /// 풀스크린을 동시에 설정하는 프로퍼티
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

            //변경된 값으로 fullScreen 모드 변경
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
            //버튼의 텍스트를 가져옴
            text = transform.GetChild(0).GetComponent<TMP_Text>();

        //현재 스크린의 상태대로 상태 설정
        isFullScreen = Screen.fullScreen;

        if (text != null)
            SetBtnText();

    }

    public void OnClickBtn()
    {
        IsFullScreen = !IsFullScreen;
    }

    /// <summary>
    /// IsFullScreen 변수에 따라 버튼의 텍스트 설정
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
