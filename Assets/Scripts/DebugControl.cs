using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 디버그 창 컨트롤
/// </summary>
public class DebugControl : MonoBehaviour
{

    World world;
    Text txt;

	//초당 프레임 레이트
	float fps;
	//프레임 간격
	float frameSec;

	//프레임 레이트 측정을 위한 변수
	float deltaTime = 0.0f;

	//디버그 텍스트 캐싱
	const string debugTxt = "systemfile36 : UnityMinecraft Project\n";

	//프레임 체크 코루틴을 0.3초 마다 실행하기 위한 변수
	public static readonly WaitForSecondsRealtime _WaitForSecondsRealtime
		= new WaitForSecondsRealtime(0.3f);

	//참조 초기화
	void Awake()
	{
		world = GameObject.Find("World").GetComponent<World>();
		txt = GetComponent<Text>();
	}

	void Update()
    {
		//프레임 레이트를 위한 변수 갱신
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

	//1초마다 fps 텍스트 갱신
	IEnumerator SetFpsCount()
	{
		while(true)
		{
			//마지막 프레임으로 부터 현재까지의 시간을 사용
			fps = (int)(1.0f / deltaTime);
			frameSec = deltaTime * 1000.0f;
			yield return _WaitForSecondsRealtime;
		}
	}

	//활성화 될때마다 호출되는 이벤트
	void OnEnable()
	{
		//활성화 되면 코루틴을 시작한다.
		StartCoroutine(SetFpsCount());
	}

	void OnDisable()
	{
		StopCoroutine(SetFpsCount());
	}
}
