using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �� Ŭ������ ���̿ȿ� �ʿ��� ������ ��´�.
/// ������ ���������� ���� ������ Ʋ�̴�.
/// ������ ���� ����
/// </summary>
[CreateAssetMenu(fileName = "BiomeAttributes", menuName = "MineCraft/BiomeAttributes")]
public class BiomeAttributes : ScriptableObject
{
    public string biomeName;

    [Header("Attributes of Biome's Distribution")]
    [Tooltip("Biome's Scale. As this value increases, " +
        "the area of this Biome narrow")]
    public float biomeScale;
    [Tooltip("Biome's Offset. Add to PerlinNoise's arguments" +
        "this value should be diffrent for each Object that use PerlinNoise")]
    public float biomeOffset;

    /// <summary>
    ///�� ���� �׻� solid�� ���̴�. ��, ������ ���� ����
    /// </summary>
    public const int solidHeight = 42;

    [Header("Attributes of Biome's Terrain")]
    //������ �ִ� ���� - solidHeight ��
    [Tooltip("Terrain's max height.")]
    public int terHeight;

    //���� ������ ��, ����
    [Tooltip("Terrain's scale. As this value increases, " +
        "the terrain becomes steep")]
    public float terScale;

    //ǥ���� ���� ����
    //�� �Ʒ� 3ĭ ������ ���� ��
    public byte surfaceBlockId;
    public byte b_surfaceBlockId;

    //�Ĺ� ���� ������
    //PlantId�� ��ġ�� �Ĺ��� ���̵�
    //PlantSetScale�� �Ĺ� ���� �ϳ��� ũ�⿡ ����(������ ����)
    //PlantSetThreshold�� �Ĺ� ������ �л굵�� ����
    //Plant Scale�� PlantThreshold�� ���� ������ ��ġ
    [Header("Attributes of PlantSet")]
    [Tooltip("Plant's ID. Default Value is 0(tree)")]
    public int PlantId = 0;
    [Tooltip("PlantSet's Scale. As this value increases, " +
        "the Area of each PlantSet widens")]
    public float PlantSetScale = 1.3f;
    [Tooltip("PlantSet's Threshold. As this value increases, " +
        "PlantSet will be generated frequently")]
    public float PlantSetThreshold = 0.6f;
    [Tooltip("Plant's Scale. As this value increases, " +
        "the density of Plants will increase")]
    public float PlantScale = 15f;
    [Tooltip("Plant's Threshold. As this value increases, " +
        "the Plant will be generated frequently")]
    public float PlantThreshold = 0.8f;
    public bool isCreatePlants = true;

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