using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
Controla as cameras no canto direito inferior do UI
*/
public class CameraControl : MonoBehaviour
{
    void Start()
    {
        Button[] buttons = GetComponentsInChildren<Button>();
        for(int i = 0; i < buttons.Length; i++)
        {
            int buttonIndex = i;
            buttons[i].onClick.AddListener(() => SwitchCamera(buttonIndex));
        }
    }
    void SwitchCamera(int camera)
    {
        if(camera < -1 || camera >= transform.GetChild(0).childCount) return;

        for(int i = 0; i < transform.GetChild(0).childCount; i++)
        {
            transform.GetChild(0).GetChild(i).GetComponent<Image>().color = Color.gray;
            transform.GetChild(2).GetChild(i).gameObject.SetActive(false);
        }

        transform.GetChild(0).GetChild(camera).GetComponent<Image>().color = Color.black;
        transform.GetChild(2).GetChild(camera).gameObject.SetActive(true);
    }
}
