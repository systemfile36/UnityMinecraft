using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Structure
{
	/// <summary>
	/// �ε����� ���� ���� �ٸ� ������(�Ĺ�)��
	/// ���� ��ȯ
	/// </summary>
	public static Queue<VoxelMod> CreateMajorPlant(int index, Vector3 pos, int minHeight, int maxHeight, int seed = 0)
    {
		//�ε����� ���� ���� �ٸ� ������(�Ĺ�) ��ȯ
		switch(index)
        {
			case 0:
				return CreateTree(pos, minHeight, maxHeight, seed);
			case 1:
				return CreateCactus(pos, minHeight, maxHeight, seed);
			//���� �߰� ����
        }

		//�ε����� ����� ����ִ� Queue��ȯ
		return new Queue<VoxelMod>();
    }


	//������ �����ϴ� �κ�
	//��ġ�� modifications ť�� �ִ�, �ּ� ���̸� �޾Ƽ� ������ ���� ��ŭ ������ ����
	//���� ������ ������ Queue<VoxelMod> �� ������
    public static Queue<VoxelMod> CreateTree(Vector3 pos, int minHeight, int maxHeight, int seed = 0)
	{
		
		//�Ŀ� ��ȯ�� que
		Queue<VoxelMod> que = new Queue<VoxelMod>();


		//���̸� �������� �ϱ� ����, ��Ȯ�� ��ǥ�� �־�� ��
		//0 ~ �ִ���� ���� ���� height�� ��
		int height = (int)(maxHeight * Noise.GetPerlin2D(new Vector2(pos.x, pos.z), 2503f, 2f, seed));

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


	public static Queue<VoxelMod> CreateCactus(Vector3 pos, int minHeight, int maxHeight, int seed = 0)
	{

		//�Ŀ� ��ȯ�� que
		Queue<VoxelMod> que = new Queue<VoxelMod>();


		//���̸� �������� �ϱ� ����, ��Ȯ�� ��ǥ�� �־�� ��
		//0 ~ �ִ���� ���� ���� height�� ��
		int height = (int)(maxHeight * Noise.GetPerlin2D(new Vector2(pos.x, pos.z), 250f, 3f, seed));

		if (height < minHeight)
		{
			height = minHeight;
		}

		//������ ��ġ�� �������� �׾� �ø�
		for (int i = 1; i < height; i++)
		{
			que.Enqueue(new VoxelMod(new Vector3(pos.x, pos.y + i, pos.z), 12));
		}

		

		return que;

	}

}
