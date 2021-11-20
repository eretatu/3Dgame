using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemyController : MonoBehaviour
{
    DamageState damageState;
    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        damageState = new DamageState();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player")) 
        { 
            if (other.transform.root.gameObject.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsTag("launch"))
            {
                //this.gameObject.transform.position += new Vector3(0, 1, 0);
                animator.CrossFadeInFixedTime("damage", 0);
                Debug.Log("吹っ飛び");
            }
            else if (other.transform.root.gameObject.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsTag("Rebellion"))
            {
                Debug.Log("ノックバック");
            }

        }

    }
}
