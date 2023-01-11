using System;
using UnityEngine;
using Valve.VR;

//VRコントローラのトラックパッド入力を処理する関数
public class Trackpad : MonoBehaviour
{
    //パッドのどこを触っているのかを取得するためのTrackPadという関数にSteamVR_Actions.default_TrackPadを固定
    private SteamVR_Action_Vector2 TrackPad = SteamVR_Actions.default_TrackPad;

    //パッドの何処に触れているかを2次元データで格納するための変数
    public Vector2 posleft;

    //Update
    void Update()
    {
        //結果をGetLastAxisで取得してposleftに格納
        //SteamVR_Input_Sources.機器名（ここは左コントローラ）
        posleft = TrackPad.GetLastAxis(SteamVR_Input_Sources.LeftHand); //コントローラ（左）のトラックパッドの何処に触れているかを2次元データで取得
        //posleftの中身を確認
        //Debug.Log(posleft.x + " " + posleft.y);
    }
}