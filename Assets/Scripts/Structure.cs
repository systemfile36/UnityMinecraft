using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Structure
{
	//������ �����ϴ� �κ�
	//��ġ�� modifications ť�� �ִ�, �ּ� ���̸� �޾Ƽ� ������ ���� ��ŭ ������ ����
	//���� ������ ������ Queue<VoxelMod> �� ������
    public static Queue<VoxelMod> CreateTree(Vector3 pos, int minHeight, int maxHeight)
	{
		
		//�Ŀ� ��ȯ�� que
		Queue<VoxelMod> que = new Queue<VoxelMod>();


		//���̸� �������� �ϱ� ����, ��Ȯ�� ��ǥ�� �־�� ��
		//0 ~ �ִ���� ���� ���� height�� ��
		int height = (int)(maxHeight * Noise.GetPerlin2D(new Vector2(pos.x, pos.z), 250f, 3f));

		if(height < minHeight)
		{
			height = minHeight;
		}

		//������ ��ġ�� ������ �׾� �ø�
		for(int i = 1; i < height; i++)
		{
			que.Enqueue(new VoxelMod(new Vector3(pos.x, pos.y + i, pos.z), 9));
		}

		//�������� �޾��ִ� �κ�
		//�ܼ��� ť�� ���·� ����
		for(int x = -2; x <= 2; x++)
		{
			for (int y = -3; y <= 1; y++)
			{
				for (int z = -2; z <= 2; z++)
				{
					//�߾��� ���� ���Ͽ�
					if(x != 0 || z != 0)
						que.Enqueue(new VoxelMod(new Vector3(pos.x + x, pos.y + height + y, pos.z + z), 11));
				}
			}
		}

		return que;

	}
}
