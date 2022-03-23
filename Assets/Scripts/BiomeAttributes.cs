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

    //���� ���� ������
    //ForestScale�� �� �ϳ��� ũ��
    //ForestThreshold�� �� ���� �������� ����� ��
    //TreeScale�� TreeThreshold�� �� ���� ������ ��ġ
    [Header("Attributes of Trees")]
    public float ForestScale = 1.3f;
    public float ForestThreshold = 0.6f;
    public float TreeScale = 15f;
    public float TreeThreshold = 0.8f;

    //������ �ִ� �ּ� ����
    public int Max_TreeHeight = 12;
    public int Min_TreeHeight = 5;

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