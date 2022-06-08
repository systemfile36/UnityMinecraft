using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using TMPro;

public class WorldNodeControl : MonoBehaviour
{
    /// <summary>
    /// 이 노드의 WorldName
    /// </summary>
    public string WorldName
    { get { return worldName.text; } }

    private int seed;

    /// <summary>
    /// 이 노드의 seed
    /// </summary>
    public int Seed
    { get { return seed; } }

    
    public Image icon;

    public TMP_Text worldName;

    public TMP_Text worldInfo;

    /// <summary>
    /// WorldNode의 Button 컴포넌트에 대한 참조
    /// </summary>
    public Button button;

    /// <summary>
    /// WorldNode의 text를 설정함
    /// </summary>
    public void SetWorldText(string worldName, int seed, string date = null, GameMode gameMode = GameMode.Debug)
    {
        this.worldName.text = worldName;

        if (date == null)
            date = "1970/01/01 00:00";

        this.seed = seed;

        //월드 이름, 날짜, 게임모드, seed 순으로 텍스트 설정
        worldInfo.text = $"{worldName}, ({date})\n{gameMode.ToString()}, {seed}";
    }

    /// <summary>
    /// WorldNode에 클릭 이벤트 연결
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
