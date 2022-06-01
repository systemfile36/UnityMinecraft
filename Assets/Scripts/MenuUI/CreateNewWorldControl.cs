using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CreateNewWorldControl : MonoBehaviour
{
    public MenuControl menuControl;

    public TMP_InputField tbName;
    public TMP_InputField tbSeed;

    private GameMode gameMode = GameMode.Debug;
    public Button btnGameMode;

    void Awake()
    {
        //Seed 창에 기본적으로 랜덤한 시드를 넣음
        tbSeed.text = Random.Range(int.MinValue, int.MaxValue).ToString();
    }

    /// <summary>
    /// 게임 모드 변경 버튼 순환, 추후 작성
    /// </summary>
    public void OnClickBtnGameMode()
    {
        
    }

    public void OnClickCreate()
    {
        //월드의 이름을 반영
        World.worldName = tbName.text;

        //만약 시드 값이 int 최대값보다 크다면
        //tbSeed의 텍스트를 비우고 포커스 이동
        long seed = long.Parse(tbSeed.text);
        if(seed > int.MaxValue)
        {
            tbSeed.text = "";
            tbSeed.Select();
            return;
        }

        //시드 값 설정
        World.seed = (int)seed;

        //월드 생성으로 넘어간다.
        GameManager.Mgr.LoadWorld();
    }

    public void OnClickCancel()
    {
        menuControl.MoveToMenu(transform.name, "MainMenu");
    }
}
