using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

/// <summary>
/// 디버그 창 컨트롤
/// </summary>
public class DebugControl : MonoBehaviour
{

    World world;
	StarterAssets.FirstPersonController player;
    Text txt;

	VoxelState currentSelected;

	VoxelState currentPlaceGuide;

	//초당 프레임 레이트
	int fps = 0;
	//프레임 간격
	float frameSec = 0;

	//프레임 레이트 측정을 위한 변수
	float deltaTime = 0.0f;

	//디버그 텍스트 캐싱
	const string debugTxt = "systemfile36 : UnityMinecraft Project\n";

	//최적화를 위한 StringBuilder
	StringBuilder strBuilder = new StringBuilder();

	int min_fps = int.MaxValue;

	//프레임 체크 코루틴을 0.3초 마다 실행하기 위한 변수
	public static readonly WaitForSecondsRealtime _WaitForSecondsRealtime
		= new WaitForSecondsRealtime(0.1f);

	//참조 초기화
	void Awake()
	{
		world = GameObject.Find("World").GetComponent<World>();
		player = GameObject.FindGameObjectWithTag("Player").GetComponent<StarterAssets.FirstPersonController>();
		txt = GetComponent<Text>();
	}

	void Update()
    {
		//프레임 레이트를 위한 변수 갱신
		deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

    }

	//0.2초마다 fps 텍스트 갱신
	//텍스트가 표출될때만 관련 정보를 참조하면 된다.
	IEnumerator SetFpsCount()
	{
		while(true)
		{
			//마지막 프레임으로 부터 현재까지의 시간을 사용
			fps = (int)(1.0f / deltaTime);
			frameSec = deltaTime * 1000.0f;

			//최저 프레임
			if(fps > 10 && fps < min_fps)
			{
				min_fps = fps;
			}

			currentSelected = world.GetVoxelState(player.selectGuide.position);
			currentPlaceGuide = world.GetVoxelState(player.placeGuide.position);

			strBuilder.Clear();
			//StringBuilder를 통한 최적화
			strBuilder.Append(debugTxt);
			strBuilder.Append($"FPS : {fps} \n");
			strBuilder.Append($"Min_FPS : {min_fps}\n");
			strBuilder.Append($"FrameSec : {frameSec}\n");
			strBuilder.AppendFormat("X : {0}, Y : {1}, Z : {2} \n\n", Mathf.FloorToInt(player.transform.position.x),
				Mathf.FloorToInt(world.player.transform.position.y), Mathf.FloorToInt(player.transform.position.z));
			strBuilder.AppendFormat("Chunk : {0} {1}\n", world.playerChunkCoord.x, world.playerChunkCoord.z);
			strBuilder.Append($"Selected_Position {player.selectGuide.position}\n");
			strBuilder.Append($"Selected_Light {currentSelected.globalLightWeight}\n");
			strBuilder.Append($"PlaceGuide_Light {currentPlaceGuide.globalLightWeight}\n");
			strBuilder.Append($"Current Loaded Chunks {GameManager.Mgr.World.worldData.chunks.Count}");


			txt.text = strBuilder.ToString();

			yield return _WaitForSecondsRealtime;
		}
	}

	//활성화 될때마다 호출되는 이벤트
	void OnEnable()
	{
		//활성화 되면 코루틴을 시작한다.
		StartCoroutine(SetFpsCount());
		//deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
	}

	void OnDisable()
	{
		StopCoroutine(SetFpsCount());
	}
}
