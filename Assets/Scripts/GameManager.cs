using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

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
	public InfoMessage InfoMessage { get { return _infoMessage; } set { _infoMessage = value; } }

	/// <summary>
	/// ���� World�� ���� �ν��Ͻ�
	/// </summary>
	private World _world = null;
	public World World { get { return _world; } set { _world = value; } }

	
	/// <summary>
	/// ����� Ŭ���� �����ϴ� ��ųʸ�
	/// �̸��� AudioClip�� ���� ������ ����ȴ�.
	/// </summary>
	private Dictionary<string, AudioClip> audios = new Dictionary<string, AudioClip>();

	/// <summary>
	/// ���� ���� UI�� AudioSource ����
	/// </summary>
	private AudioSource UI_audioSource = null;

	/// <summary>
	/// ���� ���� UI�� AudioSource ����
	/// </summary>
	public AudioSource UIAudioSource { get { return UI_audioSource; } set { UI_audioSource = value; } }
	
	/// <summary>
	/// �ӽ� ����
	/// </summary>
	public AudioClip click;

	//Awake�� GameManager ������Ʈ�� �����Ƿ� �Ҷ����� ����
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

		LoadAudios();

		//_settings = new Settings();
	}

	//�̱����� Start�� �ѹ��� �����
	void Start()
	{
		//SaveJsonFile(GamePaths.SettingsPath, GamePaths.SettingsFileName, ObjToJsonStringIndented(_settings));
		//InfoMessage ������Ʈ�� �����Ҷ��� ������Ʈ�� �޴´�.

		Debug.Log("GameManager.cs Start()");
		//LoadAudios();
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


	
	/// <summary>
	/// audios ��ųʸ��� Resources/Sounds/ �Ʒ��� ��� AudioClip�� �߰�
	/// </summary>
	void LoadAudios()
    {

		//Resources/Sounds/ �Ʒ��� ��� AudioClip�� �ҷ��´�.
		var temp = Resources.LoadAll("Sounds", typeof(AudioClip));

		var alias = Resources.Load("SoundsAlias") as TextAsset;

		//�̸��� JObject�� �޾ƿ´�.
		JObject jAlias = JObject.Parse(alias.text);


		//temp�� AudioClip�� ���� ��ȸ�Ͽ� ��ųʸ��� �߰��Ѵ�.
		foreach(AudioClip clip in temp)
        {
			//�̸��� ���� ������ �߰�

			//�̸��� ������ �̸����� �߰�
			if(jAlias.ContainsKey(clip.name))
            {
				audios.Add(jAlias[clip.name].ToString(), clip);
			}
			else
            {
				audios.Add(clip.name, clip);
            }
        }


	}
	
	/// <summary>
	/// AudioSource�� AudioClip �̹��� �޾Ƽ� �����
	/// </summary>
	/// <param name="source"></param>
	/// <param name="audioName"></param>
	public void PlayAudio(AudioSource source, string audioName)
    {
		if (audios.ContainsKey(audioName) && source != null)
		{
			//AudioSource�� AudioClip�� �Ѱܼ� ����Ѵ�.
			source.PlayOneShot(audios[audioName]);
		}
		else
        {
			Debug.Log($"{audioName} is not exist!");
        }
    }

	/// <summary>
	/// audios ��ųʸ����� AudioClip�� ã�� �����Ѵ�.
	/// </summary>
	/// <param name="audioName">AudioClip�� �̸�</param>
	/// <returns></returns>
	public AudioClip GetAudioClip(string audioName)
    {
		if (audios.ContainsKey(audioName))
			return audios[audioName];
		else
			return null;
    }

	/// <summary>
	/// �������� AudioClip �̸��� �޾Ƽ� AudioClip �迭�� �����Ѵ�.
	/// </summary>
	/// <param name="audioNames">AudioClip�̸��� �迭</param>
	/// <returns></returns>
	public AudioClip[] GetAudioClips(params string[] audioNames)
    {
		if (audioNames == null || audioNames.Length == 0)
			return null;

		AudioClip[] audioClips = new AudioClip[audioNames.Length];

		for (int i = 0; i < audioNames.Length; i++)
        {
			audioClips[i] = GetAudioClip(audioNames[i]);
        }

		return audioClips;
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
	}

	/// <summary>
	/// Settings ������Ʈ�� �޾Ƽ� ���� �������� �����ϰ�, 
	/// �񵿱������� ������ ��, IAsyncResult ��ȯ
	/// </summary>
	public IAsyncResult SaveAndApplySettings(Settings set)
    {
		//���� ������ ���ο� �������� ����
		_settings = set;

		//_settings�� null�̰ų�, ��ȿ�� ���� �ƴ϶��
		if (_settings == null || !_settings.IsValid())
		{
			Debug.Log("Invalid Settings");
			//�⺻ �����ڸ� ���� �⺻ ������ �ʱ�ȭ
			_settings = new Settings();
		}

		Application.targetFrameRate = _settings.targetFrameRate;

		//�񵿱�� ���ο� ������ �����ϱ� ���� Action �븮��
		Action<Settings> save = (_settings) =>
		{
			SaveJsonFile(GamePaths.SettingsPath, GamePaths.SettingsFileName, ObjToJsonStringIndented(set));
		};

		//�񵿱�� ���� ������ �����ϰ� IAsyncResult ��ȯ
		return save.BeginInvoke(_settings, null, null);
	}

	/// <summary>
	/// World�� �񵿱�� �ε��ϰ� AsyncOperation�� return �Ѵ�.
	/// </summary>
	public AsyncOperation LoadWorld()
    {
		//���� Scene�� World�� �ƴ϶��
		if (!SceneManager.GetActiveScene().name.Equals("World"))
		{
			//���� Scene�� �ݰ� World Scene�� �ε��Ѵ�.
			//SceneManager.LoadScene("World", LoadSceneMode.Single);

			return SceneManager.LoadSceneAsync("World", LoadSceneMode.Single);
		}
		else
			return null;
    }

	/// <summary>
	/// mainMenu�� �ε��Ѵ�.
	/// </summary>
	public void LoadMainMenu()
    {
		if(!SceneManager.GetActiveScene().name.Equals("mainMenu"))
        {
			SceneManager.LoadScene("mainMenu", LoadSceneMode.Single);
		}
    }

	/// <summary>
	/// ������ �����Ѵ�.
	/// </summary>
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

	/// <summary>
	/// path�� fName .json ������ �о json string���� ��ȯ
	/// </summary>
	/// <returns></returns>
    public static string LoadJsonString(string path, string fName)
    {
		try
		{
			using (FileStream fStream = new FileStream($"{path}/{fName}", FileMode.Open))
			{
				byte[] bytes = new byte[fStream.Length];
				fStream.Read(bytes, 0, bytes.Length);
				string json = Encoding.UTF8.GetString(bytes);
				return json;
			}

		}
		//������ �� ã���� ���� ���� ó��
		catch (FileNotFoundException e)
		{
			Debug.Log($"There is no File \"{e.FileName}\"");
			return null;
		}
		catch (System.Exception e)
        {
			Debug.Log(e.Message);
			return null;
        }
	}
}
/// <summary>
/// ���ӿ� �ʿ��� ��ε��� ����� ���� Ŭ����
/// </summary>
public static class GamePaths
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

	/// <summary>
	/// World�� ����Ǵ� ���͸��� ���
	/// </summary>
	private static string _savePath = Application.persistentDataPath + "/saves/";

	public static string SavePath
    {
		get { return _savePath; }
    }
}