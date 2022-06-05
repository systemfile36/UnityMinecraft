using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �� ûũ�� ���� �����Ͱ� ����Ǵ� Ŭ����(Ʋ)
/// </summary>
[System.Serializable]
public class ChunkData
{
    //ûũ�� ��� ��ǥ�� ��Ÿ����. 
    //Vector2Int�� ����ȭ �ϱ� �����Ƿ� ���� ��ǥ�� �Ѵ�.
    int x;
    int y;

    //�� ��ǥ�� ���� ������ Vector2Int�� �Ѵ�.
    /// <summary>
    /// ûũ�� ��� ��ǥ
    /// </summary>
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
    public ChunkData(Vector2Int pos) { position = pos;}

    public ChunkData(int x, int y) { this.x = x; this.y = y; }

    //���ϼ� �˻�, ��ǥ�� ������ ����.
    public override bool Equals(object obj)
    {
        if(!(obj is ChunkData)) 
            return false;

        ChunkData data = (ChunkData)obj; 

        return x == data.x && y == data.y;
    }

    //HashSet���� ����ϱ� ���� �ؽ��ڵ� �����Լ�
    public override int GetHashCode()
    {
        //���� �÷ο찡 �߻��ص� �׳� �����Ѵٴ� ���̴�.
        unchecked
        {
            int hash = 7;

            hash = hash * 23 + x;
            hash = hash * 83 + y;

            return hash;
        }
    }

    /// <summary>
    /// ���� �����Ѵ�.
    /// </summary>
    public void PopulateMap()
    {
        //ûũ�� ��� ��ǥ�� ���� ��ǥ�� ��ȯ�Ѵ�.
        Vector2Int tempPos = new Vector2Int(x * 16, y * 16);

        //y ������ ������ ���� �����μ� �Ʒ����� ���� �����Ǿ� ��
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    //GetVoxel�� �ش� ��ǥ�� �� ���̵� �޾Ƽ� �ʿ� �ִ´�.
                    //���� ��ȸ���� �� ��ǥ��
                    //���� ûũ�� ���� ���� ��ǥ�� ���ؼ� ���� ��ǥ�� �ѱ��.
                    map[x, y, z] = new VoxelState(GameManager.Mgr.World.GetVoxel(new Vector3(x + tempPos.x, y, z + tempPos.y)));
                }
            }
        }

        //����� ��Ͽ� �߰��Ѵ�.
        GameManager.Mgr.World.worldData.AddToChanged(this);
    }
}
