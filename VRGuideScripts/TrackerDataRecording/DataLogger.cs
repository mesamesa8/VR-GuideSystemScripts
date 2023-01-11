using System.IO;
using System;
using System.Text;
using UnityEngine;

/*
 * 現実世界の移動の様子をトラッキングデータ（座標データ）としてCSVファイルに保存するスクリプト
 * トラッキングデータは位置情報を記録するデバイスを用いて記録する
 */
public class DataLogger : MonoBehaviour
{
    private StreamWriter sw;
    private FileInfo fi;

    // Use this for initialization
    void Start()
    {
        //UnityEngine.Debug.Log("記録開始");
    }

    // Update is called once per frame
    void Update()
    {
        string str;
        string format = "yyyy-MM-dd-HH-mm-ss";
        string filename = "D:\\Unity\\TrackingData.csv";

        fi = new FileInfo(filename);    //CSVファイル作成
        str = this.gameObject + "," + (transform.position.x) + "," + (transform.position.y) + "," + (transform.position.z); //トラッキングデータ（座標データ）を格納
        sw = fi.AppendText();
        sw.WriteLine(str);  //トラッキングデータを書き込み
        sw.Flush();
        sw.Close();
    }
    
}