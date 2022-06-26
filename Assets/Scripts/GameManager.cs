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
/// 게임을 관리하는 싱글톤 클래스
/// </summary>
public class GameManager : MonoBehaviour
{
	//혹시 모를 수정을 방지하기 위해 private로 선언
	//프로퍼티로 참조
	private static GameManager instance = null;

	/// <summary>
	/// 설정 클래스
	/// </summary>
	[SerializeField] private Settings _settings = null;

	/// <summary>
	/// 안내 메시지 클래스
	/// </summary>
	private InfoMessage _infoMessage = null;
	public InfoMessage InfoMessage { get { return _infoMessage; } set { _infoMessage = value; } }

	/// <summary>
	/// 현재 World에 대한 인스턴스
	/// </summary>
	private World _world = null;
	public World World { get { return _world; } set { _world = value; } }

	
	/// <summary>
	/// 오디오 클립을 저장하는 딕셔너리
	/// 이름과 AudioClip에 대한 참조로 저장된다.
	/// </summary>
	private Dictionary<string, AudioClip> audios = new Dictionary<string, AudioClip>();

	/// <summary>
	/// 현재 씬의 UI의 AudioSource 참조
	/// </summary>
	private AudioSource UI_audioSource = null;

	/// <summary>
	/// 현재 씬의 UI의 AudioSource 참조
	/// </summary>
	public AudioSource UIAudioSource { get { return UI_audioSource; } set { UI_audioSource = value; } }
	
	/// <summary>
	/// 임시 변수
	/// </summary>
	public AudioClip click;

	//Awake는 GameManager 오브젝트가 생성되려 할때마다 실행
	void Awake()
	{
		if (instance == null)
		{
			//처음 생성되면 static 변수에 자신 대입
			instance = this;

			//Scene이 바뀌어도 파괴되지 않는다.
			DontDestroyOnLoad(this.gameObject);

		}
		else
		{
			//이미 인스턴스가 존재하면 새로 생성된 오브젝트 삭제
			Destroy(this.gameObject);
		}

		//설정 로드
		LoadSettings();

		LoadAudios();

		//_settings = new Settings();
	}

	//싱글톤의 Start는 한번만 실행됨
	void Start()
	{
		//SaveJsonFile(GamePaths.SettingsPath, GamePaths.SettingsFileName, ObjToJsonStringIndented(_settings));
		//InfoMessage 오브젝트가 존재할때만 컴포넌트를 받는다.

		Debug.Log("GameManager.cs Start()");
		//LoadAudios();
	}


	/// <summary>
	/// GameManager의 static 인스턴스를 반환
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
	/// Settings의 인스턴스를 반환
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
	/// InfoMessage의 출력할 메시지 큐에 str을 넣는다.
	/// </summary>
	public void PrintInfo(string str)
    {
		_infoMessage.AddMessages(str);
    }


	
	/// <summary>
	/// audios 딕셔너리에 Resources/Sounds/ 아래의 모든 AudioClip들 추가
	/// </summary>
	void LoadAudios()
    {

		//Resources/Sounds/ 아래의 모든 AudioClip을 불러온다.
		var temp = Resources.LoadAll("Sounds", typeof(AudioClip));

		var alias = Resources.Load("SoundsAlias") as TextAsset;

		//이명을 JObject로 받아온다.
		JObject jAlias = JObject.Parse(alias.text);


		//temp를 AudioClip에 대해 순회하여 딕셔너리에 추가한다.
		foreach(AudioClip clip in temp)
        {
			//이름과 참조 쌍으로 추가

			//이명이 있으면 이명으로 추가
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
	/// AudioSource와 AudioClip 이믈을 받아서 재생함
	/// </summary>
	/// <param name="source"></param>
	/// <param name="audioName"></param>
	public void PlayAudio(AudioSource source, string audioName)
    {
		if (audios.ContainsKey(audioName) && source != null)
		{
			//AudioSource에 AudioClip을 넘겨서 재생한다.
			source.PlayOneShot(audios[audioName]);
		}
		else
        {
			Debug.Log($"{audioName} is not exist!");
        }
    }

	/// <summary>
	/// audios 딕셔너리에서 AudioClip을 찾아 리턴한다.
	/// </summary>
	/// <param name="audioName">AudioClip의 이름</param>
	/// <returns></returns>
	public AudioClip GetAudioClip(string audioName)
    {
		if (audios.ContainsKey(audioName))
			return audios[audioName];
		else
			return null;
    }

	/// <summary>
	/// 여러개의 AudioClip 이름을 받아서 AudioClip 배열을 리턴한다.
	/// </summary>
	/// <param name="audioNames">AudioClip이름의 배열</param>
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
		//파일로 부터 세팅 정보를 불러온다.
		_settings = LoadJsonFile<Settings>(GamePaths.SettingsPath, GamePaths.SettingsFileName);

		//_settings가 null이거나, 유효한 값이 아니라면
		if (_settings == null || !_settings.IsValid())
        {
			Debug.Log("Invalid Settings");
			//기본 생성자를 통해 기본 값으로 초기화
			_settings = new Settings();
		}

		Application.targetFrameRate = _settings.targetFrameRate;
	}

	/// <summary>
	/// Settings 오브젝트를 받아서 현재 설정으로 지정하고, 
	/// 비동기적으로 저장한 후, IAsyncResult 반환
	/// </summary>
	public IAsyncResult SaveAndApplySettings(Settings set)
    {
		//현재 설정을 새로운 세팅으로 설정
		_settings = set;

		//_settings가 null이거나, 유효한 값이 아니라면
		if (_settings == null || !_settings.IsValid())
		{
			Debug.Log("Invalid Settings");
			//기본 생성자를 통해 기본 값으로 초기화
			_settings = new Settings();
		}

		Application.targetFrameRate = _settings.targetFrameRate;

		//비동기로 새로운 세팅을 저장하기 위한 Action 대리자
		Action<Settings> save = (_settings) =>
		{
			SaveJsonFile(GamePaths.SettingsPath, GamePaths.SettingsFileName, ObjToJsonStringIndented(set));
		};

		//비동기로 현재 세팅을 저장하고 IAsyncResult 반환
		return save.BeginInvoke(_settings, null, null);
	}

	/// <summary>
	/// World를 비동기로 로드하고 AsyncOperation을 return 한다.
	/// </summary>
	public AsyncOperation LoadWorld()
    {
		//현재 Scene이 World가 아니라면
		if (!SceneManager.GetActiveScene().name.Equals("World"))
		{
			//현재 Scene을 닫고 World Scene을 로드한다.
			//SceneManager.LoadScene("World", LoadSceneMode.Single);

			return SceneManager.LoadSceneAsync("World", LoadSceneMode.Single);
		}
		else
			return null;
    }

	/// <summary>
	/// mainMenu를 로드한다.
	/// </summary>
	public void LoadMainMenu()
    {
		if(!SceneManager.GetActiveScene().name.Equals("mainMenu"))
        {
			SceneManager.LoadScene("mainMenu", LoadSceneMode.Single);
		}
    }

	/// <summary>
	/// 게임을 종료한다.
	/// </summary>
	public void QuitGame()
    {
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#endif

		Application.Quit();
	}

	/// <summary>
	/// object를 Json 형식의 string으로 반환
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
	/// Json형식의 string을 T 타입으로 반환
	/// </summary>
	public static T JsonStringToobj<T>(string json)
	{
		return JsonConvert.DeserializeObject<T>(json);
	}

	/// <summary>
	/// Json형식의 string을 path에 fName으로 저장
	/// fName은 확장자 포함 권장
	/// </summary>
	public static void SaveJsonFile(string path, string fName, string json)
	{
		//File을 확실히 닫기 위해 using문 사용
		using (FileStream fStream = new FileStream($"{path}/{fName}", FileMode.Create))
		{
			//인코딩 문제 예방을 위해 System.Text.Encoding 사용
			byte[] bytes = Encoding.UTF8.GetBytes(json);
			fStream.Write(bytes, 0, bytes.Length);
		}
	}

	/// <summary>
	/// path의 fName json파일을 읽어서 T 타입의 오브젝트로 반환
	/// fName은 확장자 포함 권장
	/// </summary>
	public static T LoadJsonFile<T>(string path, string fName) 
		where T : class //T는 참조형만 가능
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
		//파일을 못 찾았을 때의 예외 처리
		catch(FileNotFoundException e)
        {
			Debug.Log($"There is no File \"{e.FileName}\"");

        }
		//역직렬화 실패 시
		catch(JsonException e)
        {
			Debug.Log(e.Message);
        }

		//예외 발생 시 null 반환
		return null;
	}

	/// <summary>
	/// path의 fName .json 파일을 읽어서 json string으로 반환
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
		//파일을 못 찾았을 때의 예외 처리
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
/// 게임에 필요한 경로들이 저장된 정적 클래스
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
	/// World가 저장되는 디렉터리의 경로
	/// </summary>
	private static string _savePath = Application.persistentDataPath + "/saves/";

	public static string SavePath
    {
		get { return _savePath; }
    }
}