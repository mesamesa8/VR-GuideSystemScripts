using System;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

/*
 * トラッキングデータ（座標と角度）を表示するスクリプト
 */
public class TrackerTracking : MonoBehaviour
{
    //トラッカーの位置座標格納用
    public Vector3 TrackerPosision;

    //トラッカーの回転座標格納用（クォータニオン）
    private Quaternion TrackerRotationQ;

    //トラッカーの回転座標格納用（オイラー角）
    private Vector3 TrackerRotation;

    //トラッカーのpose情報を取得するためにtracker1という関数にSteamVR_Actions.default_Poseを固定
    private SteamVR_Action_Pose tracker = SteamVR_Actions.default_Pose;

    // Update is called once per frame
    void Update()
    {
        //位置座標を取得
        TrackerPosision = tracker.GetLocalPosition(SteamVR_Input_Sources.Chest);
        //回転座標をクォータニオンで値を受け取る
        TrackerRotationQ = tracker.GetLocalRotation(SteamVR_Input_Sources.Chest);
        //取得した値をクォータニオン → オイラー角に変換
        TrackerRotation = TrackerRotationQ.eulerAngles;

        //取得したデータを表示（T1D：Tracker1位置，T1R：Tracker1回転）
        Debug.Log("T1D:" + TrackerPosision.x + ", " + TrackerPosision.y + ", " + TrackerPosision.z + "\n" +
                    "T1R:" + TrackerRotation.x + ", " + TrackerRotation.y + ", " + TrackerRotation.z);
    }
}