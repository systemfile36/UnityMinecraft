using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System.Text;

/// <summary>
/// ������ �����ϴ� �̱��� Ŭ����
/// </summary>
public class GameManager : MonoBehaviour
{
	//Ȥ�� �� ������ �����ϱ� ���� private�� ����
	//������Ƽ�� ����
	private static GameManager instance = null;

	/// <summary>
	/// ���� Ŭ����
	/// </summary>
	[SerializeField] private Settings _settings = null;

	/// <summary>
	/// �ȳ� �޽��� Ŭ����
	/// </summary>
	private InfoMessage _infoMessage = null;

	void Awake()
	{
		if (instance == null)
		{
			//ó�� �����Ǹ� static ������ �ڽ� ����
			instance = this;

			//Scene�� �ٲ� �ı����� �ʴ´�.
			DontDestroyOnLoad(this.gameObject);
		}
		else
		{
			//�̹� �ν��Ͻ��� �����ϸ� ���� ������ ������Ʈ ����
			Destroy(this.gameObject);
		}

		//���� �ε�
		LoadSettings();

		//InfoMessage �ʱ�ȭ
		_infoMessage = GameObject.Find("InfoMessage").GetComponent<InfoMessage>();
		//_settings = new Settings();
	}

	void Start()
	{
		//SaveJsonFile(GamePaths.SettingsPath, GamePaths.SettingsFileName, ObjToJsonStringIndented(_settings));
		
	}

	/// <summary>
	/// GameManager�� static �ν��Ͻ��� ��ȯ
	/// </summary>
	public static GameManager Mgr
	{
		get
		{
			if (instance == null)
				return null;

			return instance;
		}
	}

	/// <summary>
	/// Settings�� �ν��Ͻ��� ��ȯ
	/// </summary>
	public Settings settings
	{
		get
		{
			if (_settings == null)
				return null;

			return _settings;
		}
	}

	/// <summary>
	/// InfoMessage�� ����� �޽��� ť�� str�� �ִ´�.
	/// </summary>
	public void PrintInfo(string str)
    {
		_infoMessage.AddMessages(str);
    }

	public void LoadSettings()
	{
		//���Ϸ� ���� ���� ������ �ҷ��´�.
		_settings = LoadJsonFile<Settings>(GamePaths.SettingsPath, GamePaths.SettingsFileName);

		//_settings�� null�̰ų�, ��ȿ�� ���� �ƴ϶��
		if (_settings == null || !_settings.IsValid())
        {
			Debug.Log("Invalid Settings");
			//�⺻ �����ڸ� ���� �⺻ ������ �ʱ�ȭ
			_settings = new Settings();
		}

		Application.targetFrameRate = _settings.targetFrameRate;
		Random.InitState(_settings.seed);
	}

	public void QuitGame()
    {
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#endif

		Application.Quit();
	}

	/// <summary>
	/// object�� Json ������ string���� ��ȯ
	/// </summary>
	public static string ObjToJsonString(object obj)
	{
		return JsonConvert.SerializeObject(obj);
	}

	public static string ObjToJsonStringIndented(object obj)
	{
		return JsonConvert.SerializeObject(obj, Formatting.Indented);
	}

	/// <summary>
	/// Json������ string�� T Ÿ������ ��ȯ
	/// </summary>
	public static T JsonStringToobj<T>(string json)
	{
		return JsonConvert.DeserializeObject<T>(json);
	}

	/// <summary>
	/// Json������ string�� path�� fName���� ����
	/// fName�� Ȯ���� ���� ����
	/// </summary>
	public static void SaveJsonFile(string path, string fName, string json)
	{
		//File�� Ȯ���� �ݱ� ���� using�� ���
		using (FileStream fStream = new FileStream($"{path}/{fName}", FileMode.Create))
		{
			//���ڵ� ���� ������ ���� System.Text.Encoding ���
			byte[] bytes = Encoding.UTF8.GetBytes(json);
			fStream.Write(bytes, 0, bytes.Length);
		}
	}

	/// <summary>
	/// path�� fName json������ �о T Ÿ���� ������Ʈ�� ��ȯ
	/// fName�� Ȯ���� ���� ����
	/// </summary>
	public static T LoadJsonFile<T>(string path, string fName) 
		where T : class //T�� �������� ����
	{
        try
        {
			using (FileStream fStream = new FileStream($"{path}/{fName}", FileMode.Open))
			{
				byte[] bytes = new byte[fStream.Length];
				fStream.Read(bytes, 0, bytes.Length);
				string json = Encoding.UTF8.GetString(bytes);
				return JsonConvert.DeserializeObject<T>(json);
			}
			
		}
		//������ �� ã���� ���� ���� ó��
		catch(FileNotFoundException e)
        {
			Debug.Log($"There is no File \"{e.FileName}\"");

        }
		//������ȭ ���� ��
		catch(JsonException e)
        {
			Debug.Log(e.Message);
        }

		//���� �߻� �� null ��ȯ
		return null;
	}

     
}
/// <summary>
/// ���ӿ� �ʿ��� ��ε��� ����� ���� Ŭ����
/// </summary>
public class GamePaths
{
	private static string _settingsPath = Application.dataPath;
	public static string SettingsPath 
	{ 
		get { return _settingsPath; } 
	}

	private static string _settingsFileName = "settings.json";
	public static string SettingsFileName 
	{ 
		get { return _settingsFileName; } 
	}
}