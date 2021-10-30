using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuControl : MonoBehaviour
{
    [SerializeField]
    Canvas canvas;
    // Start is called before the first frame update
    void Start()
    {
        //���߂͕��Ă���
        canvas.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && canvas.enabled == false)
        {
            Time.timeScale = 0;  // ���Ԓ�~
            Debug.Log("�|�[�Y��");
            canvas.enabled = true;
        }
        else if(Input.GetKeyDown(KeyCode.Escape) && canvas.enabled ==true)
        {
            Time.timeScale = 1;  // �ĊJ
            Debug.Log("�ĊJ");
            canvas.enabled = false;
        }


        
    }
}


