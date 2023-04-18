using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using LitJson;
using UnityEngine.Video;

public class RemoteFace : MonoBehaviour
{
    string REMOTE_NAME;
    byte[] targetImage;
    [SerializeField]
    Transform targetVideoUI;
    VideoPlayer vPlayer;

    private void Awake()
    {
        REMOTE_NAME = File.ReadAllLines("./Assets/remote_host.txt")[0];
        targetImage = File.ReadAllBytes("./Assets/Resources/happy.png");
        vPlayer = targetVideoUI.GetComponent<VideoPlayer>();
    }

    IEnumerator _GetFaceData(byte[] wav)
    {
        Hashtable hashtable = new Hashtable();
        hashtable.Add("wav", Convert.ToBase64String(wav));
        hashtable.Add("pic", Convert.ToBase64String(targetImage));
        string data = JsonMapper.ToJson(hashtable);

        Debug.Log(data);

        using(UnityWebRequest request = new UnityWebRequest(REMOTE_NAME,"POST"))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(data));
            request.downloadHandler = new DownloadHandlerBuffer();

            request.timeout = 3600;
            request.chunkedTransfer = false;
            request.useHttpContinue = false;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string t = request.downloadHandler.text;
                string mp4Str = (string)JsonMapper.ToObject(t)["mp4"];
                byte[] mp4Data = Convert.FromBase64String(mp4Str);
                File.WriteAllBytes("./Assets/Resources/test.mp4", mp4Data);
                vPlayer.clip = Resources.Load<VideoClip>("test");
                vPlayer.Play();
            }else
            {
                Debug.LogError("Error: " + request.error);
            }
        }
    }

    public void GetFaceData(byte[] wav)
    {
        StartCoroutine(_GetFaceData(wav));
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Q))
    //    {
    //        vPlayer.clip = Resources.Load<VideoClip>("test");
    //        vPlayer.Play();
    //    }
    //}
}
