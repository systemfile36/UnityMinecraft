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

    /// <summary>
    /// ������ ������
    /// </summary>
    private Settings settings;

    [Tooltip("Controller of Loading Screen")]
    public LoadingControl loadingControl;

    //�ν����Ϳ��� �� SliderPrefab�� ���� ���� ������ ��
    [Tooltip("SliderControl of Settings Slider")]
    public SliderControl[] settingsSliders;

    //�޴����� �̵��� ���� MenuControl ��ü
    public MenuControl menuControl;
 
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
        //�ʱⰪ ����
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
        //GameManager���� ������, �б⸸ �� ��
        settings = GameManager.Mgr.settings;

        SetSlidersValue(settings);

    }

    /// <summary>
    /// Settings ������Ʈ�� �޾� �� �����̴��� ������
    /// </summary>
    public void SetSlidersValue(Settings set)
    {
        //�� ���ÿ� �°� �����̴� ����
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

        //Sound, �ۼ�Ƽ���� ��Ÿ���Ƿ� 100�� �����ش�.
        slidercontrols["BGMVolume"].slider.value = set.bgmVolume * 100;
        slidercontrols["SEVolume"].slider.value = set.seVolume * 100;
        slidercontrols["StepVolume"].slider.value = set.stepVolume * 100;
        slidercontrols["StepPitch"].slider.value = set.stepPitch * 100;
        slidercontrols["PlaceVolume"].slider.value = set.placedVolume * 100;
        slidercontrols["PlacePitch"].slider.value = set.placedPitch * 100;
        slidercontrols["BreakVolume"].slider.value = set.breakedVolume * 100;
        slidercontrols["BreakPitch"].slider.value = set.breakedPitch * 100;
        slidercontrols["FalledVolume"].slider.value = set.falledVolume * 100;
        slidercontrols["FalledPitch"].slider.value = set.falledPitch * 100;

    }

    /// <summary>
    /// �� ī�װ� ������ �ڷΰ��� ��ư�� ������ ��. 
    /// SettingCategory�� ���ư�
    /// </summary>
    public void OnClickBtnBack()
    {
        //ī���ڸ��� �̵�
        MoveToMenu("SettingCategory");
    }

    /// <summary>
    /// Cancel ��ư�� ������ ��. MainMenu�� ���ư�
    /// </summary>
    public void OnClickCancel()
    {
        menuControl.MoveToMenu(transform.name, "MainMenu");
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

    /// <summary>
    /// Reset ��ư�� ������ ��,
    /// ��� �����̴��� ���� ���� ������ ����
    /// </summary>
    public void OnClickBtnReset()
    {
        SetSlidersValue(settings);
    }

    /// <summary>
    /// Default ��ư�� ������ ��,
    /// ���� ������ �ʱ� �������� �ٲٰ� �����̴� ����
    /// </summary>
    public void OnClickBtnDefault()
    {
        //�⺻ �����ڸ� ���� �ʱⰪ���� ����
        settings = new Settings();

        //�� �����̴� �� ����
        SetSlidersValue(settings);
    }

    /// <summary>
    /// Save ��ư�� Ŭ������ ���� �̺�Ʈ
    /// ���ο� Settings ��ü�� ���� GameManager�� �ѱ��.
    /// </summary>
    public void OnClickSave()
    {
        //���� ������ Settings�� ����
        Settings newSettings = new Settings();

        //�� ������ �°� ���������� �ݿ���
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

        //Sound, �ۼ�Ƽ���� ǥ���ϹǷ� 100���� �������ش�.
        newSettings.bgmVolume = Mathf.Clamp(RoundToFloat(slidercontrols["BGMVolume"].slider.value / 100, 1), 0, 1);
        newSettings.seVolume = Mathf.Clamp(RoundToFloat(slidercontrols["SEVolume"].slider.value / 100, 1), 0, 1);
        newSettings.stepVolume = Mathf.Clamp(RoundToFloat(slidercontrols["StepVolume"].slider.value / 100, 1), 0, 1);
        newSettings.stepPitch = Mathf.Clamp(RoundToFloat(slidercontrols["StepPitch"].slider.value / 100, 1), 0, 1);
        newSettings.placedVolume = Mathf.Clamp(RoundToFloat(slidercontrols["PlaceVolume"].slider.value / 100, 1), 0, 1);
        newSettings.placedPitch = Mathf.Clamp(RoundToFloat(slidercontrols["PlacePitch"].slider.value / 100, 1), 0, 1);
        newSettings.breakedVolume = Mathf.Clamp(RoundToFloat(slidercontrols["BreakVolume"].slider.value / 100, 1), 0, 1);
        newSettings.breakedPitch = Mathf.Clamp(RoundToFloat(slidercontrols["BreakPitch"].slider.value / 100, 1), 0, 1);
        newSettings.falledVolume = Mathf.Clamp(RoundToFloat(slidercontrols["FalledVolume"].slider.value / 100, 1), 0, 1);
        newSettings.falledPitch = Mathf.Clamp(RoundToFloat(slidercontrols["FalledPitch"].slider.value / 100, 1), 0, 1);

        //�ε�Ǵ� ûũ ������ �þ� ������ �ι�� ����
        newSettings.LoadDistanceInChunks = newSettings.ViewDistanceInChunks * 2;

        //GameManager�� ���� ���ο� Settings�� �����ϰ� �ݿ��ϰ�, 
        //IAsyncResult�� LoadingControl ��ü�� �ѱ��.
        loadingControl.LoadingStart(GameManager.Mgr.SaveAndApplySettings(newSettings));
    }


    



    /// <summary>
    /// f�� �Ҽ��� �Ʒ� digits ������ ����� �ݿø�
    /// </summary>
    /// <param name="f"></param>
    /// <param name="digits"></param>
    /// <returns></returns>
    public static float RoundToFloat(float f, int digits)
    {
        return (float)System.Math.Round(f, digits);
    }
}
