using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCvSharp;



public class Camcontroll : MonoBehaviour
{
    [SerializeField]
    private int cam_width = 1920;
    [SerializeField]
    private int cam_height = 1080;
    [SerializeField]
    private RawImage cam_displayUI = null;

    private WebCamTexture cam_tex = null;


    private IEnumerator Start()
    {
        if (WebCamTexture.devices.Length == 0)
        {
            Debug.LogFormat("カメラデバイズが見つからない");
            yield break;
        }

        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            Debug.LogFormat("許可がない");
            yield break;
        }

        //とりあえずテクスチャを作る
        WebCamDevice userComeraDevice = WebCamTexture.devices[0];
        cam_tex = new WebCamTexture(userComeraDevice.name, cam_width, cam_height);

        cam_displayUI.texture = cam_tex;

        //撮影開始
        cam_tex.Play();
    }
}//class camera