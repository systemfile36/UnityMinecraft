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

    private Settings settings;

    //인스펙터에서 각 SliderPrefab에 대한 참조 설정할 것
    [Tooltip("SliderControl of Settings Slider")]
    public SliderControl[] settingsSliders;
 
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
        settings = GameManager.Mgr.settings;

        //각 세팅에 맞게 슬라이더 세팅
        slidercontrols["TargetFrameRate"].slider.value = settings.targetFrameRate;
        slidercontrols["ViewDistance"].slider.value = settings.ViewDistanceInChunks;
        slidercontrols["RotationSpeed"].slider.value = settings.RotationSpeed;
        slidercontrols["MoveSpeed"].slider.value = settings.MoveSpeed;
        slidercontrols["JumpHeight"].slider.value = settings.JumpHeight;
        slidercontrols["GroundOffset"].slider.value = settings.GroundedOffset;
        slidercontrols["GroundRadius"].slider.value = settings.GroundedRadius;
        slidercontrols["TopClamp"].slider.value = settings.TopClamp;
        slidercontrols["BottomClamp"].slider.value = settings.BottomClamp;
        slidercontrols["CheckInterval"].slider.value = settings.checkInterval;
        slidercontrols["Reach"].slider.value = settings.reach;
        slidercontrols["EditDelay"].slider.value = settings.EditDelay;
        slidercontrols["ColliderSideOffset"].slider.value = settings.pWidthSideOffset;
        slidercontrols["InvalidRate"].slider.value = settings.pInvalidRate;

    }

    public void SetSlidersValue(Settings settings)
    {
        //각 세팅에 맞게 슬라이더 세팅
        slidercontrols["TargetFrameRate"].slider.value = settings.targetFrameRate;
        slidercontrols["ViewDistance"].slider.value = settings.ViewDistanceInChunks;
        slidercontrols["RotationSpeed"].slider.value = settings.RotationSpeed;
        slidercontrols["MoveSpeed"].slider.value = settings.MoveSpeed;
        slidercontrols["JumpHeight"].slider.value = settings.JumpHeight;
        slidercontrols["GroundOffset"].slider.value = settings.GroundedOffset;
        slidercontrols["GroundRadius"].slider.value = settings.GroundedRadius;
        slidercontrols["TopClamp"].slider.value = settings.TopClamp;
        slidercontrols["BottomClamp"].slider.value = settings.BottomClamp;
        slidercontrols["CheckInterval"].slider.value = settings.checkInterval;
        slidercontrols["Reach"].slider.value = settings.reach;
        slidercontrols["EditDelay"].slider.value = settings.EditDelay;
        slidercontrols["ColliderSideOffset"].slider.value = settings.pWidthSideOffset;
        slidercontrols["InvalidRate"].slider.value = settings.pInvalidRate;
    }

    /// <summary>
    /// 뒤로가기 버튼이 눌렸을 때
    /// </summary>
    public void OnClickBtnBack()
    {
        MoveToMenu("SettingCategory");
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


}
