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
    /// ������ ������
    /// </summary>
    private Settings settings;

    [Tooltip("Controller of Loading Screen")]
    public LoadingControl loadingControl;

    [Tooltip("Setting Control Array")]
    public SettingControl[] settingControls;

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
    /// Settings ������Ʈ�� �޾� �� ���� ��Ʈ�ѿ� ������
    /// </summary>
    public void SetSlidersValue(Settings set)
    {
        //�� settingControl�� set�� �Ѱ� �� ����
        foreach(var settingControl in settingControls)
        {
            //������Ʈ�� �޾ƿ��� �ϱ� ���� InitControl ȣ��
            settingControl.InitControl();
            settingControl.SetSettingValue(ref set);
        }
        
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

        
        //�� settingControl�� newSettings�� �Ѱܼ� newSettings�� �ݿ�
        foreach(var settingControl in settingControls)
        {
            settingControl.ApplyToSettings(ref newSettings);
        }

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
