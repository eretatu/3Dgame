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
    public string Attack_1;
    public string Attack_2;
    //àÍéûï€ä«ópópïœêî
    private string Temp;
    private void Start()
    {

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
