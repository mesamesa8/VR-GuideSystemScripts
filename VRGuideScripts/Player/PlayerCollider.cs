using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Userの衝突判定を処理するスクリプト
public class PlayerCollider : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    void Update()
    {
        offset = new Vector3(0, -2, 0);
        this.transform.position = target.position + offset; //Userの衝突判定範囲を調整
    }
}