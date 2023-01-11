using UnityEngine;
using System.Collections;

//目的地"B"の入力を認識するスクリプト
public class ButtonB : MonoBehaviour
{
    public void OnButtonClick()
    {
        GameObject.Find("f020_hipoly_81_bones_Tpose").transform.GetComponent<MuseumGuide>().goB = true;
        GameObject.Find("f020_hipoly_81_bones_Tpose").transform.GetComponent<MuseumGuide>().goA = false;
    }
}