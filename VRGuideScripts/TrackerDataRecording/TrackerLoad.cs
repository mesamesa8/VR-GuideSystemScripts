using System.Collections.Generic;
using System.IO;
using UnityEngine;

/*
 * 記録したトラッキングデータ（座標データ）をゲーム空間上で再現するスクリプト
 */
public class TrackerLoad : MonoBehaviour
{
    private List<string[]> csvDatas = new List<string[]>(); // CSVの中身を入れるリスト
    private Vector3 _vector; //再生するtrackerの座標
    public TextAsset tracker; //再生するcsvテキスト
    private int count = 1;

    //fps計測
    int frameCount;
    float nextTime;

    void Start()
    {
        Application.targetFrameRate = 30;
        nextTime = Time.time + 1; //fps計測

        StringReader reader = new StringReader(tracker.text);　//読み込み
        while (reader.Peek() > -1)
        {
            string line = reader.ReadLine();　//1行ずつ読み込み
            csvDatas.Add(line.Split(',')); // リストに入れる

        }

    }

    // Update is called once per frame
    void Update()
    {
        _vector = new Vector3(float.Parse(csvDatas[count][1]), float.Parse(csvDatas[count][2]), float.Parse(csvDatas[count][3])); //Listに入った座標をfloatとして読み込み
        this.transform.position = _vector;  //記録した座標データを可視化
        count = count + 1;  

        //fps計測
        frameCount++;

        if (Time.time >= nextTime)
        {
            // 1秒経ったらFPSを表示
            UnityEngine.Debug.Log("FPS : " + frameCount);
            frameCount = 0;
            nextTime += 1;
        }
    }
}