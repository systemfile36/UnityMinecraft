using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Resolution의 DropDown을 컨트롤 한다.
/// </summary>
public class ResDropDownController : SettingControl
{
    private TMP_Dropdown dropDown;

    public FullScreenControl fullScreenControl;

    /// <summary>
    /// 해상도 DropDown에 표시되는 텍스트의 형식
    /// {0}x{1} 앞에 붙는다.
    /// </summary>
    private string captionFormat = "Resolution : ";


    /// <summary>
    /// 값이 마지막으로 저장된 후 변경되었는 지 여부
    /// </summary>
    public override bool IsChanged 
    { 
        get 
        {
            string currentSavedRes = GameManager.Mgr.settings.resolution.ToString();

            //현재 컨트롤 값이 현재 세팅값과 다르면 true를 반환
            if (!currentSavedRes.Equals(dropDown.captionText.text))
                return true;
            else
                return false;
        } 
    }

    protected override void Awake()
    {
        if (dropDown != null)
            //값 변경시 해상도 반영 연결
            dropDown.onValueChanged.AddListener(ChangeResolution);
    }

    public override void InitControl()
    {
        if(dropDown == null)
            dropDown = GetComponent<TMP_Dropdown>();

        if(dropDown != null)
            //값 변경시 해상도 반영 연결
            dropDown.onValueChanged.AddListener(ChangeResolution);

        //captionText를 초기값으로 설정 (비활성화 상태에선 초기화 되지 않기 때문)
        dropDown.captionText.text = dropDown.options[dropDown.value].text;
    }

    /// <summary>
    /// 현재 세팅대로 해상도를 변경함
    /// 값이 변경되면 해상도 반영
    /// </summary>
    private void ChangeResolution(int value)
    {
        try
        {
            //문자열인 해상도를 쪼갠다.
            var res = GetSettingValue().Split('x');

            int width = System.Convert.ToInt32(res[0]);
            int height = System.Convert.ToInt32(res[1]);

            Debug.Log($"{width}x{height}");

            //현재 설정된 해상도와 풀스크린 여부에 따라 해상도 결정
            Screen.SetResolution(width, height, fullScreenControl.IsFullScreen);

        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);
        }
        
    }

    /// <summary>
    /// 해상도를 문자열로 반환한다.
    /// </summary>
    /// <returns></returns>
    public override string GetSettingValue()
    {
        string res = dropDown.captionText.text.Split(':')[1];
        return res;
    }

    /// <summary>
    /// {0}x{1}의 형식으로 해상도를 받아서 드롭다운을 세팅함
    /// 그와 동시에 현재 해상도도 설정함
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    public override void SetSettingValue(string value)
    {
        //매개변수를 형식에 맞게 변형
        string item = captionFormat + value;

        //DropDown에 설정된 Option의 List를 받아옴
        var options = dropDown.options;
        
        //options 리스트에 대해 반복
        for(int i = 0; i < options.Count; i++)
        {
            //만약 현재 반복중인 옵션의 텍스트가 item과 일치한다면
            if(item.Equals(options[i].text))
            {
                //해당하는 인덱스로 value 설정
                //값이 바뀌었으므로 해상도 변경될 것임
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
            //해상도 텍스트를 구분자를 기준으로 자른다.
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
        //인덱스 범위나 변환 시의 예외를 대비함
        catch(System.Exception ex)
        {
            Debug.LogException(ex);
        }
        
    }

    public override void SetSettingValue(ref Settings set)
    {
        if (set == null)
            return;

        //참조로 받은 Settings의 해상도를 {0}x{1} 형식으로 바꾸어
        //SetSettingValue(string)을 호출
        SetSettingValue(set.resolution.ToString());
    }
}

