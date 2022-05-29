using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �� ûũ�� ���� �����Ͱ� ����Ǵ� Ŭ����(Ʋ)
/// </summary>
[System.Serializable]
public class ChunkData
{
    //ûũ�� ��ǥ�� ��Ÿ����. 
    //Vector2Int�� ����ȭ �������� ���������Ƿ� 
    //������ ���� int�� �ϰ�, ���� ������ Vector2Int�� �����Ѵ�.
    int x;
    int y;

    //�� ��ǥ�� ���� ���� Vector2Int�� �Ѵ�.
    public Vector2Int position
    {
        get { return new Vector2Int(x, y); }
        set { x = value.x; y = value.y; }
    }

    //VoxelState�� Serializable�ϰ� �� ������ public�̹Ƿ�
    //�ν����Ϳ��� ��Ÿ���� ������ ����������, ��Ÿ���⿡�� �ʹ� ũ�Ƿ�
    //�ν����Ϳ��� ������ ������ �����.
    [HideInInspector]
    public VoxelState[,,] map = 
        new VoxelState[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];


    //������
    public ChunkData(Vector2Int pos) { position = pos; }

    public ChunkData(int x, int y) { this.x = x; this.y = y; }

    /// <summary>
    /// ���� �����Ѵ�.
    /// </summary>
    void PopulateMap(object obj)
    {
        //y ������ ������ ���� �����μ� �Ʒ����� ���� �����Ǿ� ��
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    //GetVoxel�� �ش� ��ǥ�� �� ���̵� �޾Ƽ� �ʿ� �ִ´�.
                    //�̶� ���� ���� ��ǥ�� �־�� �Կ� �����϶�
                    //map[x, y, z] = new VoxelState(world.GetVoxel(new Vector3(x, y, z) + position));
                }
            }
        }
    }
}
