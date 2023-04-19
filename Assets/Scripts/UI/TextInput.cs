using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TextInput : MonoBehaviour
{
    [HideInInspector]
    public UnityEvent<string> OnEndEdit = new UnityEvent<string>();

    TMP_InputField inputField;

    private void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
    }
    public void OnEndEditStr(string text)
    {
        OnEndEdit?.Invoke(inputField.text);
        inputField.text = "";
    }
}
