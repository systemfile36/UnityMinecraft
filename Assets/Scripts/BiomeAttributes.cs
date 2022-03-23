using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 이 클래스는 바이옴에 필요한 정보를 담는다.
/// 일종의 세팅 파일
/// </summary>
[CreateAssetMenu(fileName = "BiomeAttributes", menuName = "MineCraft/BiomeAttributes")]
public class BiomeAttributes : ScriptableObject
{
    public string biomeName;

    //이 밑은 항상 solid인 땅이다. 즉, 지형의 최저 높이
    public int solidHeight;

    //지형의 최대 높이
    public int terHeight;

    //지형 스케일 값, 굴곡
    public float terScale;

    //나무 관련 변수들
    //ForestScale은 숲 하나의 크기
    //ForestThreshold는 그 숲의 출현율에 비례한 값
    //TreeScale과 TreeThreshold은 숲 내의 나무의 배치
    [Header("Attributes of Trees")]
    public float ForestScale = 1.3f;
    public float ForestThreshold = 0.6f;
    public float TreeScale = 15f;
    public float TreeThreshold = 0.8f;

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