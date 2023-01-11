using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

//Guideの動作を処理するスクリプト
public class MuseumGuide : MonoBehaviour
{
    /*
    以下，変数
    */
    private GameObject player;      //playerオブジェクト取得

    public Vector3 Goalpos;         //目的地格納

    //inspectorで目的地となるオブジェクトの位置を格納(goalA,Bには目的地のオブジェクトを格納)
    private Transform goal;
    public Transform goalA;
    public Transform goalB;

    //目的地bool（Trueとなった変数を目的地に設定）
    public bool goA;
    public bool goB;

    //エージェントとなるオブジェクトのNavMeshAgent格納用 
    public NavMeshAgent agent;
    public NavMeshPath path;

    private Animator motion;        //Animatorをこの変数で定義
    RaycastHit hit;                 //レイの当たり判定

    public double Distance;         //UserとGuide間の距離
    public double GuideGoalDistance;//Guideと目的地間の距離
    public double UserGoalDistance; //Userと目的地間の距離
    public double Threshold;        //Guideの歩行の速さを遅くする際のUserの歩行の速さの閾値
    public double Threshold2;       //GuideがUserに歩行を促すと判定する，2者間の距離の閾値
    public float StartTurnRadius;   //振り向き判定半径
    public float RotationSpeed;     //振り向き回転速度
    public int randrange;           //待機しない確率 (1 / randrenge)
    public int Nav = 0;             //ナビゲーション状態 0:待機 1:目的地へ移動 2:移動中にUserを待つ
    private Vector3 latestpos;      //前フレームのUserの位置（Userの歩行の速さを計算するために使用）
    private float playerspeed;      //Userの歩行の速さ
    AudioSource[] sounds;           //Guideの案内音声格納

    //guideからみたuserの角度を求めるための変数
    public Vector3 forward;         //Guide正面ベクトル(forward.y=0)
    public Vector3 vector;          //Guide位置 -> User位置　ベクトル(XZ平面上)
    public Vector3 axis = new Vector3(0.0f, 1.0f, 0.0f);//forward,vectorの2ベクトルの法線ベクトル（GuideからみたUserの角度を -180~180°で求めるために使用）
    public float rotationdegree;    //Guide正面ベクトルとGuide位置 -> User位置ベクトル間の角度（-180~180°)

    public bool flag = false;       //"目的地までの移動中" または "目的地の説明中" であるかを判定するフラグ(左の状況であればTrue)
    public bool flag2 = false;      //"UserがGuideの案内に追従していない" ことを判定するフラグ（左の状況であればTrue）


    /*
    以下，関数
    */
    //A,B入力時関数
    public void GotoA()
    {
        //UnityEngine.Debug.Log("A入力");
        StartCoroutine("DefeatA");  //DefeatA コルーチン呼び出し
    }

    public void GotoB()
    {
        //UnityEngine.Debug.Log("B入力");
        StartCoroutine("DefeatB");
    }

    //Defeat~：一定時間処理を停止する処理を含む関数
    //以下，Guideのアニメーション及び動作を制御する関数

    //案内開始アニメーションの時間分処理を一時停止(A~F分)
    IEnumerator DefeatA()
    {
        //案内開始アニメーションを開始
        //4秒待つ
        //UnityEngine.Debug.Log("Aコルーチン");

        yield return new WaitForSeconds(0.5f);  //ここで0.5秒停止(アニメーション再生時間)

        //案内音声再生
        sounds[2].Play();
        sounds[0].Stop();
        sounds[1].Stop();
        sounds[3].Stop();

        motion.SetBool("ExplainTrigger", false);    //説明アニメーション停止
        motion.SetBool("GuideStart", true);         //案内開始アニメーション再生

        //flag設定
        flag = false;
        flag2 = false;

        yield return new WaitForSeconds(4.0f);      //ここで4.0秒停止(アニメーション再生時間)
        //UnityEngine.Debug.Log("4秒経過");
        motion.SetBool("GuideStart", false);        //案内開始アニメーション停止
        motion.SetBool("WalkTrigger", true);        //歩行アニメーション開始

        //目的地Aまで歩行開始
        goA = true;
        goB = false;

        Nav = 1; //ナビゲーション状態を"1:目的地へ移動"に変更
        agent.speed = 1.0f; //Guideの歩行の速さを1.0fに設定
    }

    IEnumerator DefeatB()
    {
        //"DefeatAにならう"

        //UnityEngine.Debug.Log("Bコルーチン");
        yield return new WaitForSeconds(0.5f);
        sounds[2].Play();
        sounds[0].Stop();
        sounds[1].Stop();
        sounds[3].Stop();
        motion.SetBool("ExplainTrigger", false);
        motion.SetBool("GuideStart", true);
        flag = false;
        flag2 = false;
        yield return new WaitForSeconds(4.0f);
        //UnityEngine.Debug.Log("4秒経過");
        motion.SetBool("GuideStart", false);
        motion.SetBool("WalkTrigger", true);

        //目的地Bまで歩行開始
        goA = false;
        goB = true;

        Nav = 1;
        agent.speed = 1.0f;
    }

    //振り向きの間動作止める
    IEnumerator DefeatRotation()
    {
        yield return new WaitForSeconds(1.08f); //1.08秒停止
        //UnityEngine.Debug.Log("1.08秒経過");
    }

    //Userついて来ない場合の待機処理
    IEnumerator DefeatWait()
    {
        //待機アニメーションを開始
        //waittime秒待つ
        //UnityEngine.Debug.Log("waitコルーチン");
        int rand = UnityEngine.Random.Range(0, randrange);  //GuideがUserに移動を促す際に近づくか近づかないかを乱数で決定（この処理は実際の案内行動の観察を基に実装）
        if (rand == 0)  //GuideがUserに近づいて案内を促す
        {
            yield return new WaitForSeconds(0.1f);  //0.1秒停止
            //UnityEngine.Debug.Log("0.1秒待機");
            motion.SetBool("WalkTrigger", true);    //歩行アニメーション開始
            if (Distance <= Threshold)    //Guide-User間距離がThreshold以下なら次処理
            {
                Nav = 1;                  //ナビゲーション状態を"1:目的地へ移動"に設定
                flag = false;
                flag2 = false;
                agent.speed = 1.0f;         //Guideの移動の速さを1.0fに設定
            }
            else                        //Guide - User間距離がThresholdよりも大きければ次処理
            {
                Nav = 2;                //ナビゲーション状態を"2:移動中にUserを待つ"に設定
                gotoUser();
            }
        }
        else
        {
            yield return new WaitForSeconds(2.5f);
            UnityEngine.Debug.Log("2.5秒待機");
            motion.SetBool("WalkTrigger", true);
            //CalcDistance();
            if (Distance <= Threshold)    //2者距離がThreshold以上なら次処理
            {
                Nav = 1;
                flag = false;
                flag2 = false;
                agent.speed = 1.0f;
            }
            else
            {
                Nav = 2;                //ナビゲーション状態を"2:移動中にUserを待つ"に設定
                gotoUser();             //Userの方へ向かって移動
            }
        }
    }

    //Userの方へ移動する関数
    private void gotoUser()
    {
        //UnityEngine.Debug.Log("Userに向いて歩く");
        if (Distance > Threshold2)  //Guide-User距離が閾値よりも遠いなら以下処理
        {
            //UnityEngine.Debug.Log("Userに向いて歩き中");
        }
        else                        //Guide-User距離が閾値以下なら以下処理
        {
            Nav = 0;                    //ナビゲーション状態を"0:待機"に設定
            CalcDegree();                   //Guideがどれだけの角度振り向くか計算
            StartCoroutine("DefeatUrge");   //Userに移動を促す関数呼び出し
            //UnityEngine.Debug.Log("移動促す");
        }
    }

    //GuideがUserに移動を促す関数
    IEnumerator DefeatUrge()
    {
        motion.SetBool("UrgeTrigger", true);    //移動を促すアニメーション開始

        //移動を促す音声
        sounds[1].Play();
        sounds[0].Stop();
        sounds[2].Stop();
        sounds[3].Stop();

        yield return new WaitForSeconds(3.5f);  //3.5秒停止（アニメーション時間）
        motion.SetBool("UrgeTrigger", false);   //移動を促すアニメーション停止
        flag = false;
        flag2 = false;
        Nav = 1;                                //ナビゲーション状態を"1:目的地へ移動"に設定
        agent.speed = 1.0f;                     //Guideの移動の速さを1.0f（初期速さ）に設定
    }

    //歩行中にGuide-User間距離を確認するアニメーション実行関数（0.5秒間処理を停止するために必要）
    IEnumerator DefeatUpRotation()
    {
        yield return new WaitForSeconds(0.5f);  //0.5秒停止
        motion.SetBool("DistanceCheckTrigger", false);
    }

    //以下，角度や距離を求める関数
    private void AngleRotate()      //Guideの正面角度ををUserの方へ向ける関数
    {
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation((player.transform.position - this.transform.position).normalized), Time.deltaTime * RotationSpeed);
    }
    
    private void DistanceCheck()    //Guide-User距離確認
    {
        //UnityEngine.Debug.Log("3秒毎に呼び出される");
        CalcDegree();       //Guide-User間距離算出
        CalcGoalDistance(); //Guide-目的地間距離算出

        if (rotationdegree < -90 || rotationdegree > 90)    //Guide正面ベクトルから±90°の範囲にUserがいなければ2者間距離を確認する処理
        {
            motion.SetBool("DistanceCheckTrigger", true);   //距離を確認する振り向きアニメーション開始

            if (Distance <= Threshold)      //2者距離がThreshold以上なら次処理
            {
                flag = false;               //UserがGuideに追従していると判定
            }
            else if (GuideGoalDistance < UserGoalDistance || playerspeed < 0.05)    //Guide-目的地間距離がUser-目的地間距離よりも小さい　かつ　Userの歩行の速さが0.05より小さいとUserは歩行停止していると判定し，以下の処理
            {
                Nav = 0;                    //ナビゲーション状態を"0:待機"に変更
                //UnityEngine.Debug.Log("Distance遠い");
                motion.SetBool("WalkTrigger", false);   //歩行アニメーション停止
                flag2 = true;                           //
                StartCoroutine("DefeatWait");           //案内に追従しないUserを待つコルーチン呼び出し
            }
            StartCoroutine("DefeatUpRotation");     //距離確認アニメーション停止
        }
    }

    private void CalcDistance()     //Guide-User間の距離計算（X,Z軸要素のみで計算）
    {
        Distance = Math.Sqrt(((this.transform.position.x - player.transform.position.x) * (this.transform.position.x - player.transform.position.x)) + ((this.transform.position.z - player.transform.position.z) * (this.transform.position.z - player.transform.position.z)));    //Guide-User距離
        //UnityEngine.Debug.Log(Distance);
    }

    private void CalcGoalDistance() //Guide-目的地間距離，User-目的地間距離を計算（X,Z軸要素のみで計算）
    {
        GuideGoalDistance = Math.Sqrt(((this.transform.position.x - agent.destination.x) * (this.transform.position.x - agent.destination.x)) + ((this.transform.position.z - agent.destination.z) * (this.transform.position.z - agent.destination.y)));           //Guide-Goal距離
        UserGoalDistance = Math.Sqrt(((player.transform.position.x - agent.destination.x) * (player.transform.position.x - agent.destination.x)) + ((player.transform.position.z - agent.destination.z) * (player.transform.position.z - agent.destination.y)));    //User-Goal距離
    }
    
    public void CalcDegree()        //Guide正面ベクトルとGuide位置-User位置のベクトルの相対角度角度を求める(X,Z軸のみで計算)
    {
        forward = transform.forward;
        forward.y = 0;
        vector = (player.transform.position - this.transform.position).normalized;
        vector.y = 0;
        rotationdegree = Vector3.SignedAngle(forward, vector, axis);
        GetComponent<Animator>().SetFloat("rotationdegree", rotationdegree);
    }

    // Use this for initialization(初期設定)
    void Start()
    {
        Application.targetFrameRate = 30;       //フレームレートを30に固定

        //エージェントのNaveMeshAgentを取得する
        agent = GetComponent<NavMeshAgent>();

        //Player（Userのキャラクタ）取得
        player = GameObject.Find("PlayerCollider");

        //motionにAnimatorコンポーネント設定
        motion = gameObject.GetComponent<Animator>();
        motion.SetBool("GuideStart", false);
        motion.SetBool("WalkTrigger", false);
        motion.SetBool("ExplainTrigger", false);
        motion.SetBool("DistanceCheckTrigger", false);
        motion.SetBool("UrgeTrigger", false);

        //初期目的地をGuide自身に設定（動かない）
        agent.destination = this.transform.position;    
        Goalpos = agent.destination;

        latestpos = this.transform.position;    //前フレームを自身の位置に設定

        //Guide音声設定
        sounds = GetComponents<AudioSource>();
        sounds[3].Play();
        sounds[0].Stop();
        sounds[1].Stop();
        sounds[2].Stop();
    }

    // Update is called once per frame
    void Update()
    {
        //VRコントローラの先からレイ（直線）を飛ばす
        Ray ray = new Ray(this.transform.position, this.transform.forward);                             //レイとばす
        UnityEngine.Debug.DrawRay(this.transform.position, this.transform.forward * 6, Color.blue, 3f); //レイ表示

        CalcDistance();                         //2者距離求める

        /*
        //Nav（ナビゲーション状態）確認
        if (Nav == 0) UnityEngine.Debug.Log("Nav:0(待機中)");
        else if (Nav == 1) UnityEngine.Debug.Log("Nav:1(移動中)");
        else UnityEngine.Debug.Log("Nav:2(User待機中)");
        */

        //目的地となる座標を設定する(キーボード入力の場合)
        //例："A"を入力すると"A"オブジェクトに向かって移動
        if ((Input.GetKey("left shift") || Input.GetKey("right shift")) && Input.GetKey(KeyCode.A))
        {
            GotoA();
        }
        if ((Input.GetKey("left shift") || Input.GetKey("right shift")) && Input.GetKey(KeyCode.B))
        {
            GotoB();
        }

        if (Nav == 0)                                       //Guide:その場で待機
        {
            agent.destination = this.transform.position;    //目的地を自身(Guideの現在地)にして停止（これだけだと少しだけ動いてしまうので次の行のコードと合わせる）
            agent.speed = 0.0f;                             //Guideの移動の速さを0.0fにして移動停止
        }
        else if (Nav == 1)                                  //Guide:目的地に向かって移動
        {
            //例：goA == trueなら"A"オブジェクトに向かって移動（B~Fについてもこれに倣う）
            if (goA == true)
            {
                agent.destination = goalA.position;
            }
            if (goB == true)
            {
                agent.destination = goalB.position;
            }
        }
        else                                                //Guide:Userを待機（目的地までの移動中），後にUserに向かって移動
        {
            agent.destination = player.transform.position;  //目的地をUserに設定
            agent.speed = 1.0f;                             //移動の速さを初期速度1.0fに設定
            gotoUser();                                     //Userの方へ向かって移動
        }
        
        playerspeed = ((player.transform.position - latestpos) / Time.deltaTime).magnitude; //Userの移動速度を計算
        latestpos = player.transform.position;                                              //Userの前フレームの位置として格納（上の行で使用）

        if (playerspeed < 0.5)                              //Userの移動の速さが閾値0.5よりも小さければ移動が遅いと判定，以下処理
        {
            if (agent.speed > 0.4f) agent.speed = agent.speed - 0.1f;   //Guideの移動の速さをフレームごとに0.1f遅くする（最も遅い移動速さを0.4fとする）
        }
        else if (playerspeed > 1.0)                         //Userの移動の速さが閾値1.0よりも速ければ移動が遅いと判定，以下処理
        {
            if (Distance < 1.0)                             //さらにGuideとUserの距離が1.0(m)以下であればUserはGuideの案内に追従していると判定し，以下処理
            {
                if (agent.speed < 1.0f) agent.speed = agent.speed + 0.1f;//Guideの移動の速さをフレームごとに0.1f速くする（最も速い移動速さを1.0fとする）
            }
        }

        if (Goalpos != agent.destination)                   //目的地が決まっているなら
        {
            if (!agent.pathPending && agent.remainingDistance < StartTurnRadius)    //目的地に近づいたら
            {
                //UnityEngine.Debug.Log("目標物到着");

                //Guideの移動回転止める
                Nav = 0;
                motion.SetBool("WalkTrigger", false);

                CalcDegree();                               //Guideがどれだけの角度振り向くか計算
                AngleRotate();                              //Guideの正面をPlayerの方に向ける

                if (flag2 == false)
                {
                    if (Physics.Raycast(ray, out hit))
                    {
                        //UnityEngine.Debug.Log("Rayがhit");
                        //UnityEngine.Debug.Log(hit.collider.gameObject.name);

                        if (hit.collider.gameObject.name == "PlayerCollider")   //Rayが当たったオブジェクトのtagがPlayerだったら
                        {
                            UnityEngine.Debug.Log("RayがPlayerに当たった");
                            if (Distance < 3)                                   //GuideとUserの距離が3以下ならUserは目的地に到着したと判定，以下処理
                            {
                                motion.SetBool("ExplainTrigger", true);         //目的地の説明アニメーション再生

                                //説明音声再生
                                sounds[0].Play();
                                sounds[1].Stop();
                                sounds[2].Stop();
                                sounds[3].Stop();

                                Goalpos = agent.destination;                    //目的地を現在地に設定（説明中はその場から動かない）

                                //UnityEngine.Debug.Log("説明開始");
                            }
                        }
                    }
                }
                return;
            }
            else if (flag == false)                            //目的地に到着していないなら
            {
                //歩行アニメーション再生
                motion.SetBool("ExplainTrigger", false);
                motion.SetBool("WalkTrigger", true);

                flag = true;

                //DistanceCheck()を3秒後に呼び出す。(3秒ごとにUser-Guide距離を確認)
                Invoke("DistanceCheck", 3f);
            }
        }
        else                                                    //新しい目的地が指定されていないなら
        {
            AngleRotate();                                      //GuideをPlayerの方に向ける（目的地が指定されるのを待機）
        }
    }
}

//拡張
#if UNITY_EDITOR
            [CustomEditor(typeof(MuseumGuide))]
            public class MuseumGuideEditor : Editor
            {
                
                public override void OnInspectorGUI()
                {
                    MuseumGuide myTarget = (MuseumGuide)target;

                    //float入力
                    myTarget.StartTurnRadius = EditorGUILayout.FloatField("Start Turn Radius",myTarget.StartTurnRadius);
                    myTarget.RotationSpeed = EditorGUILayout.FloatField("Rotation Speed",myTarget.RotationSpeed);

                    if(GUILayout.Button("Save Asset")){
                        EditorUtility.SetDirty(myTarget);
                        AssetDatabase.SaveAssets();
                    }
                    DrawDefaultInspector();
                }
            }
#endif