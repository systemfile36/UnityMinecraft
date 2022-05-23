using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MenuControl : MonoBehaviour
{

    /// <summary>
    /// 이름과 Transform으로 메뉴를 저장하는 딕셔너리
    /// </summary>
    private Dictionary<string, Transform> menus = new Dictionary<string, Transform>();

    void Awake()
    {
        //자식의 Transform을 이름과 연결지어 딕셔너리에추가
        for(int i = 0; i < transform.childCount; i++)
        {
            menus.Add(transform.GetChild(i).name, transform.GetChild(i)); 
        }
    }

    /// <summary>
    /// 이름을 통해 현재 메뉴를 비활성화 하고 이동할 메뉴 활성화
    /// </summary>
    public void MoveToMenu(string current, string toMove)
    {
        menus[current].gameObject.SetActive(false);
        menus[toMove].gameObject.SetActive(true);    
    }

}
