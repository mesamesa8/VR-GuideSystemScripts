using UnityEngine;
using System.Collections;

//目的地"A"の入力を認識するスクリプト
public class ButtonA : MonoBehaviour
{
    public void OnButtonClick()
    {
            GameObject.Find("f020_hipoly_81_bones_Tpose").transform.GetComponent<MuseumGuide>().goA = true;
            GameObject.Find("f020_hipoly_81_bones_Tpose").transform.GetComponent<MuseumGuide>().goB = false;
    }
}