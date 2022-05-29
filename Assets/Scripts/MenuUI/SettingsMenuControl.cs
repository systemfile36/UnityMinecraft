using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// SettingsMenu를 컨트롤함
/// </summary>
public class SettingsMenuControl : MonoBehaviour
{

    /// <summary>
    /// 이름과 오브젝트로 메뉴를 저장하는 딕셔너리
    /// </summary>
    private Dictionary<string, Transform> menus = new Dictionary<string, Transform>();

    /// <summary>
    /// 각 Slider의 SliderControl를 이름으로 저장한다.
    /// </summary>
    private Dictionary<string, SliderControl> slidercontrols = new Dictionary<string, SliderControl>();

    /// <summary>
    /// 가져온 설정값
    /// </summary>
    private Settings settings;

    [Tooltip("Controller of Loading Screen")]
    public LoadingControl loadingControl;

    //인스펙터에서 각 SliderPrefab에 대한 참조 설정할 것
    [Tooltip("SliderControl of Settings Slider")]
    public SliderControl[] settingsSliders;

    //메뉴간의 이동을 위한 MenuControl 객체
    public MenuControl menuControl;
 
    void Awake()
    {
        //MenuTitle은 제외하기 위해 인덱스 1부터 시작해서
        //Child의 Transform 전부 딕셔너리에 추가
        for (int i = 1; i < transform.childCount; i++)
        {
            menus.Add(transform.GetChild(i).name, transform.GetChild(i));
        }

        //배열을 참고로, 이름과 SliderControl을 짝지어 넣는다.
        foreach(SliderControl control in settingsSliders)
        {
            slidercontrols.Add(control.transform.name, control);
        }
 
    }

    private void Start()
    {
        //초기값 설정
        LoadCurrentSettings();
    }

    /// <summary>
    /// 이름을 통해 현재 메뉴를 비활성화 하고 이동할 메뉴 활성화
    /// </summary>
    public void MoveToMenu(string current, string toMove)
    {
        menus[current].gameObject.SetActive(false);
        menus[toMove].gameObject.SetActive(true);
    }

    public void MoveToMenu(string toMove)
    {
        //이동하고자 하는 것만 활성화 하고 나머지는 비활성화 한다.
        foreach(Transform t in menus.Values)
        {
            t.gameObject.SetActive(false);
        }
        menus[toMove].gameObject.SetActive(true);
    }

    /// <summary>
    /// 현재 설정값을 통해, 각 슬라이더의 초기값을 지정한다.
    /// </summary>
    public void LoadCurrentSettings()
    {
        //GameManager에서 가져옴, 읽기만 할 것
        settings = GameManager.Mgr.settings;

        SetSlidersValue(settings);

    }

    /// <summary>
    /// Settings 오브젝트를 받아 각 슬라이더에 세팅함
    /// </summary>
    public void SetSlidersValue(Settings set)
    {
        //각 세팅에 맞게 슬라이더 세팅
        slidercontrols["TargetFrameRate"].slider.value = set.targetFrameRate;
        slidercontrols["ViewDistance"].slider.value = set.ViewDistanceInChunks;
        slidercontrols["RotationSpeed"].slider.value = set.RotationSpeed;
        slidercontrols["MoveSpeed"].slider.value = set.MoveSpeed;
        slidercontrols["SprintSpeed"].slider.value = set.SprintSpeed;
        slidercontrols["JumpHeight"].slider.value = set.JumpHeight;
        slidercontrols["GroundOffset"].slider.value = set.GroundedOffset;
        slidercontrols["GroundRadius"].slider.value = set.GroundedRadius;
        slidercontrols["TopClamp"].slider.value = set.TopClamp;
        slidercontrols["BottomClamp"].slider.value = set.BottomClamp;
        slidercontrols["CheckInterval"].slider.value = set.checkInterval;
        slidercontrols["Reach"].slider.value = set.reach;
        slidercontrols["EditDelay"].slider.value = set.EditDelay;
        slidercontrols["ColliderSideOffset"].slider.value = set.pWidthSideOffset;
        slidercontrols["InvalidRate"].slider.value = set.pInvalidRate;
    }

    /// <summary>
    /// 각 카테고리 내에서 뒤로가기 버튼이 눌렸을 때. 
    /// SettingCategory로 돌아감
    /// </summary>
    public void OnClickBtnBack()
    {
        //카테코리로 이동
        MoveToMenu("SettingCategory");
    }

    /// <summary>
    /// Cancel 버튼이 눌렸을 때. MainMenu로 돌아감
    /// </summary>
    public void OnClickCancel()
    {
        menuControl.MoveToMenu(transform.name, "MainMenu");
    }

    /// <summary>
    /// Category 내의 버튼이 눌렸을 때, 
    /// 버튼의 이름에 따라 메뉴 이동
    /// </summary>
    public void OnClickBtnCategories()
    {
        //현재 선택된 오브젝트의 이름을 받아옴
        string btn = EventSystem.current.currentSelectedGameObject.name;

        //앞에 btn 세글자를 제외한 나머지 문자열
        btn = btn.Substring(3);

        MoveToMenu(btn);
    }

    /// <summary>
    /// Reset 버튼이 눌렸을 떄,
    /// 모든 슬라이더의 값을 변경 전으로 돌림
    /// </summary>
    public void OnClickBtnReset()
    {
        SetSlidersValue(settings);
    }

    /// <summary>
    /// Default 버튼이 눌렸을 때,
    /// 현재 설정을 초기 설정으로 바꾸고 슬라이더 세팅
    /// </summary>
    public void OnClickBtnDefault()
    {
        //기본 생성자를 통해 초기값으로 변경
        settings = new Settings();

        //각 슬러이더 값 세팅
        SetSlidersValue(settings);
    }

    /// <summary>
    /// Save 버튼을 클릭했을 때의 이벤트
    /// 새로운 Settings 객체를 만들어서 GameManager에 넘긴다.
    /// </summary>
    public void OnClickSave()
    {
        //새로 설정될 Settings를 정함
        Settings newSettings = new Settings();

        //각 설정에 맞게 설정값들을 반영함
        newSettings.targetFrameRate = Mathf.FloorToInt(slidercontrols["TargetFrameRate"].slider.value);
        newSettings.ViewDistanceInChunks = Mathf.FloorToInt(slidercontrols["ViewDistance"].slider.value);
        newSettings.RotationSpeed = RoundToFloat(slidercontrols["RotationSpeed"].slider.value, 1);
        newSettings.MoveSpeed = RoundToFloat(slidercontrols["MoveSpeed"].slider.value, 1);
        newSettings.SprintSpeed = RoundToFloat(slidercontrols["SprintSpeed"].slider.value, 1);
        newSettings.JumpHeight = RoundToFloat(slidercontrols["JumpHeight"].slider.value, 1);
        newSettings.GroundedOffset = RoundToFloat(slidercontrols["GroundOffset"].slider.value, 2);
        newSettings.GroundedRadius = RoundToFloat(slidercontrols["GroundRadius"].slider.value, 2);
        newSettings.TopClamp = Mathf.FloorToInt(slidercontrols["TopClamp"].slider.value);
        newSettings.BottomClamp = Mathf.FloorToInt(slidercontrols["BottomClamp"].slider.value);
        newSettings.checkInterval = RoundToFloat(slidercontrols["CheckInterval"].slider.value, 2);
        newSettings.reach = RoundToFloat(slidercontrols["Reach"].slider.value, 1);
        newSettings.EditDelay = RoundToFloat(slidercontrols["EditDelay"].slider.value, 2);
        newSettings.pWidthSideOffset = RoundToFloat(slidercontrols["ColliderSideOffset"].slider.value, 2);
        newSettings.pInvalidRate = RoundToFloat(slidercontrols["InvalidRate"].slider.value, 2);

        //로드되는 청크 범위를 시야 범위의 두배로 설정
        newSettings.LoadDistanceInChunks = newSettings.ViewDistanceInChunks * 2;

        //GameManager를 통해 새로운 Settings로 설정하고 반영하고, 
        //IAsyncResult를 LoadingControl 객체에 넘긴다.
        loadingControl.LoadingStart(GameManager.Mgr.SaveAndApplySettings(newSettings));
    }


    



    /// <summary>
    /// f를 소수점 아래 digits 까지만 남기고 반올림
    /// </summary>
    /// <param name="f"></param>
    /// <param name="digits"></param>
    /// <returns></returns>
    public static float RoundToFloat(float f, int digits)
    {
        return (float)System.Math.Round(f, digits);
    }
}
