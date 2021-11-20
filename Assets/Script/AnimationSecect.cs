using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class AnimationSecect : MonoBehaviour
{

    [SerializeField] GameObject[] animaUnitButton;
    public string Attack_1, Attack_2, Attack_3;
    //àÍéûï€ä«ópópïœêî
    private string Temp;
    private void Start()
    {
        Attack_1 = "JAB";
        Attack_2 = "RHK";
        Attack_3 = "MC2_SAMK";
        animationSerect();

    }


    public void animationSerect()
    {
        foreach (var Value in animaUnitButton)
        {
            Value.GetComponent<Button>().onClick.AddListener(() =>
            {
                Temp =  Value.name;
            });
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
