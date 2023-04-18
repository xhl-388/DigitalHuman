using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainController : MonoBehaviour
{
    ChatGPT chatGPT = null;
    BaiduTTS baiduTTS = null;
    [SerializeField]
    private Transform textInputT;
    TextInput textInput = null;

    private void Awake()
    {
        chatGPT = GetComponent<ChatGPT>();
        baiduTTS = GetComponent<BaiduTTS>();
        textInput = textInputT.GetComponent<TextInput>();
        textInput.onEndEdit += UserInput;
    }
    public void UserInput(string text)
    {
        chatGPT.StartProcess(text);
        Debug.LogFormat("InputText: {0}", text);
    }
}
