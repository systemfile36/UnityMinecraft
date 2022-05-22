using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// SettingsMenu�� ��Ʈ����
/// </summary>
public class SettingsMenuControl : MonoBehaviour
{

    /// <summary>
    /// �̸��� ������Ʈ�� �޴��� �����ϴ� ��ųʸ�
    /// </summary>
    private Dictionary<string, Transform> menus = new Dictionary<string, Transform>();

    /// <summary>
    /// �� Slider�� SliderControl�� �̸����� �����Ѵ�.
    /// </summary>
    private Dictionary<string, SliderControl> slidercontrols = new Dictionary<string, SliderControl>();

    private Settings settings;

    //�ν����Ϳ��� �� SliderPrefab�� ���� ���� ������ ��
    [Tooltip("SliderControl of Settings Slider")]
    public SliderControl[] settingsSliders;
 
    void Awake()
    {
        //MenuTitle�� �����ϱ� ���� �ε��� 1���� �����ؼ�
        //Child�� Transform ���� ��ųʸ��� �߰�
        for (int i = 1; i < transform.childCount; i++)
        {
            menus.Add(transform.GetChild(i).name, transform.GetChild(i));
        }

        //�迭�� �����, �̸��� SliderControl�� ¦���� �ִ´�.
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
    /// �̸��� ���� ���� �޴��� ��Ȱ��ȭ �ϰ� �̵��� �޴� Ȱ��ȭ
    /// </summary>
    public void MoveToMenu(string current, string toMove)
    {
        menus[current].gameObject.SetActive(false);
        menus[toMove].gameObject.SetActive(true);
    }

    public void MoveToMenu(string toMove)
    {
        //�̵��ϰ��� �ϴ� �͸� Ȱ��ȭ �ϰ� �������� ��Ȱ��ȭ �Ѵ�.
        foreach(Transform t in menus.Values)
        {
            t.gameObject.SetActive(false);
        }
        menus[toMove].gameObject.SetActive(true);
    }

    /// <summary>
    /// ���� �������� ����, �� �����̴��� �ʱⰪ�� �����Ѵ�.
    /// </summary>
    public void LoadCurrentSettings()
    {
        settings = GameManager.Mgr.settings;

        //�� ���ÿ� �°� �����̴� ����
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
        //�� ���ÿ� �°� �����̴� ����
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
    /// �ڷΰ��� ��ư�� ������ ��
    /// </summary>
    public void OnClickBtnBack()
    {
        MoveToMenu("SettingCategory");
    }

    /// <summary>
    /// Category ���� ��ư�� ������ ��, 
    /// ��ư�� �̸��� ���� �޴� �̵�
    /// </summary>
    public void OnClickBtnCategories()
    {
        //���� ���õ� ������Ʈ�� �̸��� �޾ƿ�
        string btn = EventSystem.current.currentSelectedGameObject.name;

        //�տ� btn �����ڸ� ������ ������ ���ڿ�
        btn = btn.Substring(3);

        MoveToMenu(btn);
    }


}
