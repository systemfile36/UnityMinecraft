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


	void Awake()
	{
		if(instance == null)
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

	}

    void Start()
    {
		SaveJsonFile(GamePaths.SettingsPath, GamePaths.SettingsFileName, ObjToJsonString(_settings));
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
	/// object�� Json ������ string���� ��ȯ
	/// </summary>
	public static string ObjToJsonString(object obj)
    {
		return JsonConvert.SerializeObject(obj);
    }

	/// <summary>
	/// Json������ string�� T Ÿ������ ��ȯ
	/// </summary>
	public static T JsonStringToobj<T>(string json)
    {
		return (T)JsonConvert.DeserializeObject<T>(json);
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