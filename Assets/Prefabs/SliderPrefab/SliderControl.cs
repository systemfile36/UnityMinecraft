using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;
using UnityEngine.UI;

/// <summary>
/// Slider의 값을 참조해 라벨에 표시
/// </summary>
public class SliderControl : MonoBehaviour
{
    public TextMeshProUGUI label;
    public Slider slider;

    private string defaultStr = "";
    private StringBuilder labelText = new StringBuilder();

    //출력될 값의 형식
    [Tooltip("Format of Value")]
    public string formatStr = "{0:0.00}";

    private void Awake()
    {
        //초기 텍스트 설정
        defaultStr = label.text;

        
        //이벤트 연결
        slider.onValueChanged.AddListener(SetLabelText);

        //초기값 세팅을 위함
        SetLabelText(slider.value);
    }

    /// <summary>
    /// 값이 변할때마다 label에 표시
    /// </summary>
    /// <param name="value"></param>
    public void SetLabelText(float value)
    {
        labelText.Clear();
        labelText.Append(defaultStr);
        labelText.Append($" {string.Format(formatStr, value)}");
        label.text = labelText.ToString();
    }
}
