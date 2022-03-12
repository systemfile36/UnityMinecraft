using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noise
{
	/// <summary>
	/// ��ǥ�� ������� 2D �޸� ����� ��ȯ
	/// 0.0 ~ 1.0
	/// </summary>
	/// <param name="pos">���� ��ǥ</param>
	/// <param name="offset">�� ��ȭ�� ���� ������</param>
	/// <param name="scale">������</param>
	/// <returns></returns>
    public static float GetPerlin2D(Vector2 pos, float offset, float scale)
	{
		return Mathf.PerlinNoise((pos.x + 0.01f) / VoxelData.ChunkWidth * scale + offset, 
			(pos.y + 0.01f) / VoxelData.ChunkWidth * scale + offset);
	}

	/// <summary>
	/// ��ǥ�� �Ӱ�ġ�� �޾� ����� �Ӱ�ġ�� �Ѿ����� ���θ� ��ȯ
	/// </summary>
	/// <param name="pos">���� ��ǥ</param>
	/// <param name="offset">������</param>
	/// <param name="scale">������</param>
	/// <param name="threshold">�Ӱ�ġ</param>
	/// <returns></returns>
	public static bool GetPerlin3D(Vector3 pos, float offset, float scale, float threshold)
	{
		float x = (pos.x + offset + 0.01f) * scale;
		float y = (pos.y + offset + 0.01f) * scale;
		float z = (pos.z + offset + 0.01f) * scale;


		float AB = Mathf.PerlinNoise(x, y);
		float AC = Mathf.PerlinNoise(x, z);

		float BA = Mathf.PerlinNoise(y, x);
		float BC = Mathf.PerlinNoise(y, z);

		float CA = Mathf.PerlinNoise(z, x);
		float CB = Mathf.PerlinNoise(z, y);


		if (((AB + AC + BA + BC + CA + CB) / 6f) > threshold)
			return true;
		else
			return false;
	}
}
