using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 이 클래스는 바이옴에 필요한 정보를 담는다.
/// 일종의 수정가능한 지형 파일의 틀이다.
/// 일종의 세팅 파일
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
    ///이 밑은 항상 solid인 땅이다. 즉, 지형의 최저 높이
    /// </summary>
    public const int solidHeight = 42;

    [Header("Attributes of Biome's Terrain")]
    //지형의 최대 높이 - solidHeight 값
    [Tooltip("Terrain's max height.")]
    public int terHeight;

    //지형 스케일 값, 굴곡
    [Tooltip("Terrain's scale. As this value increases, " +
        "the terrain becomes steep")]
    public float terScale;

    //표면을 덮는 블러과
    //그 아래 3칸 정도를 덮는 블럭
    public byte surfaceBlockId;
    public byte b_surfaceBlockId;

    //식물 관련 변수들
    //PlantId는 설치될 식물의 아이디
    //PlantSetScale은 식물 집합 하나의 크기에 관여(촘촘한 정도)
    //PlantSetThreshold는 식물 집합의 분산도에 관여
    //Plant Scale과 PlantThreshold는 집합 내부의 배치
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

    //나무의 최대 최소 높이
    public int Max_TreeHeight = 12;
    public int Min_TreeHeight = 5;

    //블럭 생성 정보
    public Lode[] lodes;
}

//BiomeAttribute 에셋에서 관리하기 위함
/// <summary>
/// 블럭의 생성 범위, 임계치, 스케일등의 정보를 담는 클래스
/// </summary>
[System.Serializable]
public class Lode
{
    public string nodeName;
    public byte blockID;

    //블럭의 생성 범위(Y 좌표 범위)
    public int minHeight;
    public int maxHeight;

    public float scale;
    public float threshold;
    public float Offset;

}