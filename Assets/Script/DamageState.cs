using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DamageState : MonoBehaviour
{
    AttackType attackType = new AttackType();
     void Start()
    {
    }

    private void Update()
    {
        //Debug.Log(attackType.AtType);
    }

    public void OnTriggerEnter(Collider other)
    {

        
        switch (attackType.AtType) 
        {
            case AttackType.Type.none:
                Debug.Log("�������Ȃ�");
                break;
            case AttackType.Type.Attacklaunch:
                Debug.Log("�������");
                break;

            case AttackType.Type.AttackRebellion:
                Debug.Log("�m�b�N�o�b�N");
                break;


        }
        
    }

}
