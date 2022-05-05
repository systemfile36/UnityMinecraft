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


	void Awake()
	{
		if(instance == null)
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

	}

    void Start()
    {
		SaveJsonFile(GamePaths.SettingsPath, GamePaths.SettingsFileName, ObjToJsonString(_settings));
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
	/// object를 Json 형식의 string으로 반환
	/// </summary>
	public static string ObjToJsonString(object obj)
    {
		return JsonConvert.SerializeObject(obj);
    }

	/// <summary>
	/// Json형식의 string을 T 타입으로 반환
	/// </summary>
	public static T JsonStringToobj<T>(string json)
    {
		return (T)JsonConvert.DeserializeObject<T>(json);
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