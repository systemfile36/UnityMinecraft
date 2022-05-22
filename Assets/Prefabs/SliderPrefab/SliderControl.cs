using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;
using UnityEngine.UI;

/// <summary>
/// Slider�� ���� ������ �󺧿� ǥ��
/// </summary>
public class SliderControl : MonoBehaviour
{
    public TextMeshProUGUI label;
    public Slider slider;

    private string defaultStr = "";
    private StringBuilder labelText = new StringBuilder();

    //��µ� ���� ����
    [Tooltip("Format of Value")]
    public string formatStr = "{0:0.00}";

    private void Awake()
    {
        //�ʱ� �ؽ�Ʈ ����
        defaultStr = label.text;

        
        //�̺�Ʈ ����
        slider.onValueChanged.AddListener(SetLabelText);

        //�ʱⰪ ������ ����
        SetLabelText(slider.value);
    }

    /// <summary>
    /// ���� ���Ҷ����� label�� ǥ��
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
