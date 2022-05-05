using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

}
