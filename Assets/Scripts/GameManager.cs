using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System.Text;

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

		//InfoMessage 초기화
		_infoMessage = GameObject.Find("InfoMessage").GetComponent<InfoMessage>();
		//_settings = new Settings();
	}

	void Start()
	{
		//SaveJsonFile(GamePaths.SettingsPath, GamePaths.SettingsFileName, ObjToJsonStringIndented(_settings));
		
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

     
}
/// <summary>
/// 게임에 필요한 경로들이 저장된 정적 클래스
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