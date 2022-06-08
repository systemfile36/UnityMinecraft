using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using TMPro;

public class WorldNodeControl : MonoBehaviour
{
    /// <summary>
    /// �� ����� WorldName
    /// </summary>
    public string WorldName
    { get { return worldName.text; } }

    private int seed;

    /// <summary>
    /// �� ����� seed
    /// </summary>
    public int Seed
    { get { return seed; } }

    
    public Image icon;

    public TMP_Text worldName;

    public TMP_Text worldInfo;

    /// <summary>
    /// WorldNode�� Button ������Ʈ�� ���� ����
    /// </summary>
    public Button button;

    /// <summary>
    /// WorldNode�� text�� ������
    /// </summary>
    public void SetWorldText(string worldName, int seed, string date = null, GameMode gameMode = GameMode.Debug)
    {
        this.worldName.text = worldName;

        if (date == null)
            date = "1970/01/01 00:00";

        this.seed = seed;

        //���� �̸�, ��¥, ���Ӹ��, seed ������ �ؽ�Ʈ ����
        worldInfo.text = $"{worldName}, ({date})\n{gameMode.ToString()}, {seed}";
    }

    /// <summary>
    /// WorldNode�� Ŭ�� �̺�Ʈ ����
    /// </summary>
    /// <param name="call"></param>
    public void SetOnClick(UnityEngine.Events.UnityAction call)
    {
        button.onClick.AddListener(call);
    }

    void OnDestroy()
    {
        if(button != null)
            button.onClick.RemoveAllListeners();
    }
}
