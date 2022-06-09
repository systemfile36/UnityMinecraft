using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Structure
{
	/// <summary>
	/// 인덱스에 따라 서로 다른 구조물(식물)의
	/// 묶음 반환
	/// </summary>
	public static Queue<VoxelMod> CreateMajorPlant(int index, Vector3 pos, int minHeight, int maxHeight, int seed = 0)
    {
		//인덱스에 따라 서로 다른 구조물(식물) 반환
		switch(index)
        {
			case 0:
				return CreateTree(pos, minHeight, maxHeight, seed);
			case 1:
				return CreateCactus(pos, minHeight, maxHeight, seed);
			//추후 추가 예정
        }

		//인덱스를 벗어나면 비어있는 Queue반환
		return new Queue<VoxelMod>();
    }


	//나무를 생성하는 부분
	//위치와 modifications 큐와 최대, 최소 높이를 받아서 지정된 높이 만큼 나무를 쌓음
	//나무 블럭들을 저장한 Queue<VoxelMod> 를 리턴함
    public static Queue<VoxelMod> CreateTree(Vector3 pos, int minHeight, int maxHeight, int seed = 0)
	{
		
		//후에 반환할 que
		Queue<VoxelMod> que = new Queue<VoxelMod>();


		//높이를 랜덤으로 하기 위함, 정확한 좌표를 넣어야 함
		//0 ~ 최대높이 사이 값이 height에 들어감
		int height = (int)(maxHeight * Noise.GetPerlin2D(new Vector2(pos.x, pos.z), 2503f, 2f, seed));

		if(height < minHeight)
		{
			height = minHeight;
		}

		//지정된 위치에 나무를 쌓아 올림
		for(int i = 1; i < height; i++)
		{
			que.Enqueue(new VoxelMod(new Vector3(pos.x, pos.y + i, pos.z), 9));
		}

		//나뭇잎을 달아주는 부분
		//단순히 큐브 형태로 만듬
		for(int x = -2; x <= 2; x++)
		{
			for (int y = -3; y <= 1; y++)
			{
				for (int z = -2; z <= 2; z++)
				{
					//중앙은 비우기 위하여
					if(x != 0 || z != 0)
						que.Enqueue(new VoxelMod(new Vector3(pos.x + x, pos.y + height + y, pos.z + z), 11));
				}
			}
		}

		return que;

	}


	public static Queue<VoxelMod> CreateCactus(Vector3 pos, int minHeight, int maxHeight, int seed = 0)
	{

		//후에 반환할 que
		Queue<VoxelMod> que = new Queue<VoxelMod>();


		//높이를 랜덤으로 하기 위함, 정확한 좌표를 넣어야 함
		//0 ~ 최대높이 사이 값이 height에 들어감
		int height = (int)(maxHeight * Noise.GetPerlin2D(new Vector2(pos.x, pos.z), 250f, 3f, seed));

		if (height < minHeight)
		{
			height = minHeight;
		}

		//지정된 위치에 선인장을 쌓아 올림
		for (int i = 1; i < height; i++)
		{
			que.Enqueue(new VoxelMod(new Vector3(pos.x, pos.y + i, pos.z), 12));
		}

		

		return que;

	}

}
