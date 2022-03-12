using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noise
{
	/// <summary>
	/// 좌표를 기반으로 2D 펄린 노이즈를 반환
	/// 0.0 ~ 1.0
	/// </summary>
	/// <param name="pos">샘플 좌표</param>
	/// <param name="offset">값 변화를 위한 오프셋</param>
	/// <param name="scale">스케일</param>
	/// <returns></returns>
    public static float GetPerlin2D(Vector2 pos, float offset, float scale)
	{
		return Mathf.PerlinNoise((pos.x + 0.01f) / VoxelData.ChunkWidth * scale + offset, 
			(pos.y + 0.01f) / VoxelData.ChunkWidth * scale + offset);
	}

	/// <summary>
	/// 좌표와 임계치를 받아 노이즈가 임계치를 넘었는지 여부를 반환
	/// </summary>
	/// <param name="pos">샘플 좌표</param>
	/// <param name="offset">오프셋</param>
	/// <param name="scale">스케일</param>
	/// <param name="threshold">임계치</param>
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
