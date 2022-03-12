using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �� Ŭ������ ���̿ȿ� �ʿ��� ������ ��´�.
/// ������ ���� ����
/// </summary>
[CreateAssetMenu(fileName = "BiomeAttributes", menuName = "MineCraft/BiomeAttributes")]
public class BiomeAttributes : ScriptableObject
{
    public string biomeName;

    //�� ���� �׻� solid�� ���̴�. ��, ������ ���� ����
    public int solidHeight;

    //������ �ִ� ����
    public int terHeight;

    //���� ������ ��, ����
    public float terScale;

    //�� ���� ����
    public Lode[] lodes;
}

//BiomeAttribute ���¿��� �����ϱ� ����
/// <summary>
/// ���� ���� ����, �Ӱ�ġ, �����ϵ��� ������ ��� Ŭ����
/// </summary>
[System.Serializable]
public class Lode
{
    public string nodeName;
    public byte blockID;

    //���� ���� ����(Y ��ǥ ����)
    public int minHeight;
    public int maxHeight;

    public float scale;
    public float threshold;
    public float Offset;

}