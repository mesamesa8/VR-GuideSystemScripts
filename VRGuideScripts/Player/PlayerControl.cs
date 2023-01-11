using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Userの動作を制御するスクリプト
public class PlayerController : MonoBehaviour
{
    GameObject LeftController;  //VRコントローラ（左）を使用
    Trackpad script;            //VRコントローラのトラックパッドを使用
    public Transform target;    //Userのフレームレートを固定するのに使用

    //十字キーとマウスで操作(矢印キー or "WASDキー"：前後左右移動，マウス：視点回転)
    //カメラをキャラクターの子オブジェクトにするとわかりやすい
    //CharacterControllerが必要

    public float speed = 6.0F;          //歩行速度
    public float gravity = 20.0F;       //重力の大きさ
    public float rotateSpeed = 5.0F;    //回転速度
    public float camRotSpeed = 5.0f;    //視点の上下スピード

    private CharacterController controller;
    private Vector3 moveDirection = Vector3.zero;
    private float h, v;                 //後述
    private float mX, mY;               //後述
    private float lookUpAngle;          //見上げられる角度の範囲

    // Use this for initialization
    void Start()
    {
        Application.targetFrameRate = 30;   //フレームレートを30に固定
        controller = GetComponent<CharacterController>();
        LeftController = GameObject.Find("[CameraRig]");
        script = LeftController.GetComponent<Trackpad>();
    }//Start()

    // Update is called once per frame
    void Update()
    {
        h = Input.GetAxis("Horizontal");    //左右矢印キーの値(-1.0~1.0)
        v = Input.GetAxis("Vertical");      //上下矢印キーの値(-1.0~1.0)
        mX = Input.GetAxis("Mouse X");      //マウスの左右移動量(-1.0~1.0)
        mY = Input.GetAxis("Mouse Y");      //マウスの上下移動量(-1.0~1.0)

        Vector2 TrackPadLeft = script.posleft;  //VRコントローラ（左）のトラックパッドの入力を取得
        //UnityEngine.Debug.Log("LeftPad:" + TrackPadLeft.x + " " + TrackPadLeft.y);

        //カメラのみ上下に回転させる，180-120=60より上下60度まで見ることができる
        lookUpAngle = Camera.main.transform.eulerAngles.x - 180 + camRotSpeed * mY;
        if (Mathf.Abs(lookUpAngle) > 60 && Input.GetMouseButton(0))
            Camera.main.transform.Rotate(new Vector3(camRotSpeed * -1 * mY, 0, 0));
        if (Mathf.Abs(lookUpAngle) == 60)
            lookUpAngle = 70;

        //キャラクターの移動と回転
        moveDirection = speed * new Vector3(h, 0, v);
        var vec = new Vector3(TrackPadLeft.x, 0, TrackPadLeft.y);
        float CameraY = target.transform.rotation.y * 180;
        var result = Quaternion.Euler(0, CameraY, 0) * vec;

        //moveDirection = speed * result;                               //VR使うときはコメントアウト外す，キー入力のときはコメントアウトする
        moveDirection = transform.TransformDirection(moveDirection);    //VR使うときはコメントアウトする，キー入力のときはコメントアウト外す

        if (Input.GetMouseButton(0))                                    //マウス入力で視点移動
            gameObject.transform.Rotate(new Vector3(0, rotateSpeed * mX, 0));

        moveDirection.y -= gravity * Time.deltaTime;                    //Userを宙に浮かせないために重力処理追加

        if (!Input.GetKey("left shift") && !Input.GetKey("right shift"))//Shift押されてない場合に限り，Userの移動を許可（"Shift"+"A"で目的地指定と移動が同時に操作されるのを防ぐ）
            controller.Move(moveDirection * Time.deltaTime);
    }//Update()

}