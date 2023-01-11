using System;
using UnityEngine;
using Valve.VR;
using System.Collections.Generic;

//VRコントローラ（右）の入力を判定するスクリプト
public class RightButton : MonoBehaviour
{
    private SteamVR_Action_Boolean Iui = SteamVR_Actions.default_InteractUI;
    private Boolean Rightinteracrtui;
    public GameObject gameobject;
    public GameObject Guide;
    public GameObject NogestureGuide;

    RaycastHit hit; //レイの当たり判定

    private void Start()
    {
        Guide = GameObject.Find("CapsuleGuide");
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = new Ray(this.transform.position, this.transform.forward);    //レイをコントローラ（右）の先から飛ばす
        if (Physics.Raycast(ray, out hit))
        {
            //結果をGetStateで取得し、interactuiに格納
            //SteamVR_Input_Sources.機器名（Rightコントローラー）
            Rightinteracrtui = Iui.GetState(SteamVR_Input_Sources.RightHand);

            if (Rightinteracrtui == true)                                       //コントローラ（右）のトリガーボタンが入力されたときに衝突判定があれば以下処理
            {

                if (hit.collider.gameObject.name == "ButtonA")                  //入力オブジェクトが案内板の"目的地A"を示すパネルであれば以下処理（B~Fも同様）
                {
                    //Debug.Log("RightControllerでButtonAを押しました");
                    Guide.GetComponent<MuseumGuide>().GotoA();
                }

                if (hit.collider.gameObject.name == "ButtonB")
                {
                    //Debug.Log("RightControllerでButtonBを押しました");
                    Guide.GetComponent<MuseumGuide>().GotoB();
                }

            }
        }
    }
}