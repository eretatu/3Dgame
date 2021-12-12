using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class AnimationSecect : MonoBehaviour
{


    [SerializeField]
    private ActionDataBase _ActionDataBase;
    [SerializeField]
    private GameObject BaseButton;
    private ActionData Temp, _Attack_1, _Attack_2, _Attack_3;
    
    public ActionData Attack_1 
    {
        get { return _Attack_1; }
        set { _Attack_1 = value; }
    }
    public ActionData Attack_2
    {
        get { return _Attack_2; }
        set { _Attack_2 = value; }
    }
    public ActionData Attack_3
    {
        get { return _Attack_3; }
        set { _Attack_3 = value; }
    }

    private void Start()
    {
        //èâä˙ãZê›íË
        for(int i = 0; i < _ActionDataBase.ActionList.Count; i++) 
        {
            Attack_1 = _ActionDataBase.ActionList[0];
            Attack_2 = _ActionDataBase.ActionList[1];
            Attack_3 = _ActionDataBase.ActionList[0];
        }
        animationSerect();

    }


    public void animationSerect()
    {
        BaseButton.gameObject.SetActive(false);


        foreach (var Value in _ActionDataBase.ActionList)
        {
            GameObject buttonObject = Instantiate(BaseButton, BaseButton.transform.parent);
            buttonObject.transform.Find("text").GetComponent<TextMeshProUGUI>().text = Value.ActionName;
            buttonObject.GetComponent<Button>().onClick.AddListener(() => 
            {
                Temp = Value;
            });
            buttonObject.SetActive(true);
        }
        
    }

    public void animationDecision_1() 
    {
        Attack_1 = Temp;
    }
    public void animationDecision_2()
    {
        Attack_2 = Temp;
    }





}
