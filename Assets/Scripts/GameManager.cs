using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

}
