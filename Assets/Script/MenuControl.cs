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
        //初めは閉じておく
        canvas.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && canvas.enabled == false)
        {
            Time.timeScale = 0;  // 時間停止
            Debug.Log("ポーズ中");
            canvas.enabled = true;
        }
        else if(Input.GetKeyDown(KeyCode.Escape) && canvas.enabled ==true)
        {
            Time.timeScale = 1;  // 再開
            Debug.Log("再開");
            canvas.enabled = false;
        }


        
    }
}


