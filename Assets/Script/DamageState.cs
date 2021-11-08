using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 
public class DamageState : MonoBehaviour
{
    AttackType attackType;
    private void Start()
    {
        attackType = new AttackType();
    }

    public void OnTriggerEnter(Collider other)
    {
        

        switch (attackType.AtType) 
        {
            case AttackType.Type.none:
                Debug.Log("何もしない");
                break;
            case AttackType.Type.Attacklaunch:
                Debug.Log("吹っ飛び");
                break;

            case AttackType.Type.AttackRebellion:
                Debug.Log("ノックバック");
                break;


        }
    }
    
}
