using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class CreateNewWorldControl : MonoBehaviour
{
    public MenuControl menuControl;

    public TMP_InputField tbName;
    public TMP_InputField tbSeed;

    private GameMode gameMode = GameMode.Debug;
    public Button btnGameMode;

    public LoadingControl loadingControl;

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

    /// <summary>
    /// Create 버튼을 눌렀을 때
    /// </summary>
    public void OnClickCreate()
    {
        string worldPath = GamePaths.SavePath + "/" + tbName.text + "/";
        //만들 World 이름의 디렉터리가 이미 있다면
        if (Directory.Exists(worldPath))
        {
            menuControl.msgBoxControl.MsgBoxAsync("Warning", $"{tbName.text} is already exists!\n" +
                $"Do you want to Overwrite ?", () => 
                {
                    //존재하는 디렉터리 재귀 삭제
                    Directory.Delete(worldPath, true);

                    //월드 씬 로드
                    CreateNewWorld();
                });
        }
        else
        {
            CreateNewWorld();
        }

        
        
    }

    /// <summary>
    /// 이름과 시드를 설정하고 World 씬을 로드한다.
    /// </summary>
    void CreateNewWorld()
    {
        //월드의 이름을 반영
        World.worldName = tbName.text;

        //만약 시드 값이 int 최대값보다 크다면
        //tbSeed의 텍스트를 비우고 포커스 이동
        long seed = long.Parse(tbSeed.text);
        if (seed > int.MaxValue)
        {
            tbSeed.text = "";
            tbSeed.Select();
            return;
        }

        //시드 값 설정
        World.seed = (int)seed;

        //World 씬을 비동기적으로 로드하고 로딩창을 띄운다.
        loadingControl.LoadingStart(GameManager.Mgr.LoadWorld());
    }

    public void OnClickCancel()
    {
        menuControl.MoveToMenu(transform.name, "MainMenu");
    }
}
