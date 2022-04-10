using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ����� â ��Ʈ��
/// </summary>
public class DebugControl : MonoBehaviour
{

    World world;
    Text txt;

	//�ʴ� ������ ����Ʈ
	float fps;
	//������ ����
	float frameSec;

	//������ ����Ʈ ������ ���� ����
	float deltaTime = 0.0f;

	//����� �ؽ�Ʈ ĳ��
	const string debugTxt = "systemfile36 : UnityMinecraft Project\n";

	//������ üũ �ڷ�ƾ�� 0.3�� ���� �����ϱ� ���� ����
	public static readonly WaitForSecondsRealtime _WaitForSecondsRealtime
		= new WaitForSecondsRealtime(0.3f);

	//���� �ʱ�ȭ
	void Awake()
	{
		world = GameObject.Find("World").GetComponent<World>();
		txt = GetComponent<Text>();
	}

	void Update()
    {
		//������ ����Ʈ�� ���� ���� ����
		deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

		string tempTxt = "";
		tempTxt += "FPS : " + fps + "\n"
			+ "Frame Interval : " + frameSec + "ms\n"
			+ "\nX : " + Mathf.FloorToInt(world.player.transform.position.x)
			+ "\nY : " + Mathf.FloorToInt(world.player.transform.position.y)
			+ "\nZ : " + Mathf.FloorToInt(world.player.transform.position.z)
			+ "\nChunk : " + world.playerChunkCoord.x + " " + world.playerChunkCoord.z;


		txt.text = debugTxt + tempTxt;
    }

	//1�ʸ��� fps �ؽ�Ʈ ����
	IEnumerator SetFpsCount()
	{
		while(true)
		{
			//������ ���������� ���� ��������� �ð��� ���
			fps = (int)(1.0f / deltaTime);
			frameSec = deltaTime * 1000.0f;
			yield return _WaitForSecondsRealtime;
		}
	}

	//Ȱ��ȭ �ɶ����� ȣ��Ǵ� �̺�Ʈ
	void OnEnable()
	{
		//Ȱ��ȭ �Ǹ� �ڷ�ƾ�� �����Ѵ�.
		StartCoroutine(SetFpsCount());
	}

	void OnDisable()
	{
		StopCoroutine(SetFpsCount());
	}
}
