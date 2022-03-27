using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Structure
{
	//나무를 생성하는 부분
	//위치와 modifications 큐와 최대, 최소 높이를 받아서 지정된 높이 만큼 나무를 쌓음
	//나무 블럭들을 저장한 Queue<VoxelMod> 를 리턴함
    public static Queue<VoxelMod> CreateTree(Vector3 pos, int minHeight, int maxHeight)
	{
		
		//후에 반환할 que
		Queue<VoxelMod> que = new Queue<VoxelMod>();


		//높이를 랜덤으로 하기 위함, 정확한 좌표를 넣어야 함
		//0 ~ 최대높이 사이 값이 height에 들어감
		int height = (int)(maxHeight * Noise.GetPerlin2D(new Vector2(pos.x, pos.z), 250f, 3f));

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
}
