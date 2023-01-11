using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//目的地をAのアルファベットで表示するスクリプト
public class A : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        //オブジェクトの色をシアンに変更
        GetComponent<Renderer>().material.color = Color.cyan;
    }

    // Update is called once per frame
    void Update()
    {

        if ((Input.GetKey("left shift") || Input.GetKey("right shift")) && Input.GetKey(KeyCode.A)) //目的地が指定されたとき
        {
            //オブジェクトの色を赤に変更する
            GetComponent<Renderer>().material.color = Color.magenta;
        }

        if ((Input.GetKey("left shift") || Input.GetKey("right shift")) && Input.GetKey(KeyCode.B)) //別の目的地が指定されたとき
        {
            //オブジェクトの色をシアンに変更
            GetComponent<Renderer>().material.color = Color.cyan;
        }
    }
}