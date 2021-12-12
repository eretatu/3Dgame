using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitCollider : MonoBehaviour
{

    
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var test = other.transform.root.gameObject.GetComponent<PlayerCommon>().actionData;
            switch(test.actionType) 
            {
                case ActionData.ActionType.launch:
                    Debug.Log(test.actionType);
                    break;
                case ActionData.ActionType.Rebellion:
                    Debug.Log(test.actionType);
                    break;
            }

        }

    }
}
