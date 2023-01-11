using System;
using UnityEngine;
using Valve.VR;
using System.Collections.Generic;

//VRコントローラ（左）の入力を判定するスクリプト
public class LeftButton : MonoBehaviour
{
    private SteamVR_Action_Boolean Iui = SteamVR_Actions.default_InteractUI;
    private Boolean Leftinteracrtui;
    public GameObject gameobject;

    // Update is called once per frame
    void Update()
    {
        //結果をGetStateで取得し、interactuiに格納
        //SteamVR_Input_Sources.機器名（leftコントローラー）
        Leftinteracrtui = Iui.GetState(SteamVR_Input_Sources.LeftHand);
        if (Leftinteracrtui == true)
        {
            Debug.Log("LeftControllerが押されました");
        }
    }
}
