using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

/// <summary>
/// ����� â ��Ʈ��
/// </summary>
public class DebugControl : MonoBehaviour
{

    World world;
	StarterAssets.FirstPersonController player;
    Text txt;

	VoxelState currentSelected;

	VoxelState currentPlaceGuide;

	//�ʴ� ������ ����Ʈ
	int fps = 0;
	//������ ����
	float frameSec = 0;

	//������ ����Ʈ ������ ���� ����
	float deltaTime = 0.0f;

	//����� �ؽ�Ʈ ĳ��
	const string debugTxt = "systemfile36 : UnityMinecraft Project\n";

	//����ȭ�� ���� StringBuilder
	StringBuilder strBuilder = new StringBuilder();

	int min_fps = int.MaxValue;

	//������ üũ �ڷ�ƾ�� 0.3�� ���� �����ϱ� ���� ����
	public static readonly WaitForSecondsRealtime _WaitForSecondsRealtime
		= new WaitForSecondsRealtime(0.1f);

	//���� �ʱ�ȭ
	void Awake()
	{
		world = GameObject.Find("World").GetComponent<World>();
		player = GameObject.FindGameObjectWithTag("Player").GetComponent<StarterAssets.FirstPersonController>();
		txt = GetComponent<Text>();
	}

	void Update()
    {
		//������ ����Ʈ�� ���� ���� ����
		deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

    }

	//0.2�ʸ��� fps �ؽ�Ʈ ����
	//�ؽ�Ʈ�� ǥ��ɶ��� ���� ������ �����ϸ� �ȴ�.
	IEnumerator SetFpsCount()
	{
		while(true)
		{
			//������ ���������� ���� ��������� �ð��� ���
			fps = (int)(1.0f / deltaTime);
			frameSec = deltaTime * 1000.0f;

			//���� ������
			if(fps > 10 && fps < min_fps)
			{
				min_fps = fps;
			}

			currentSelected = world.GetVoxelState(player.selectGuide.position);
			currentPlaceGuide = world.GetVoxelState(player.placeGuide.position);

			strBuilder.Clear();
			//StringBuilder�� ���� ����ȭ
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

	//Ȱ��ȭ �ɶ����� ȣ��Ǵ� �̺�Ʈ
	void OnEnable()
	{
		//Ȱ��ȭ �Ǹ� �ڷ�ƾ�� �����Ѵ�.
		StartCoroutine(SetFpsCount());
		//deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
	}

	void OnDisable()
	{
		StopCoroutine(SetFpsCount());
	}
}
