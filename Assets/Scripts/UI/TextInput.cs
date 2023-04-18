using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextInput : MonoBehaviour
{
    [HideInInspector]
    public Action<string> onEndEdit;

    TMP_InputField inputField;

    private void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
    }
    public void OnEndEdit(string text)
    {
        onEndEdit?.Invoke(inputField.text);
        inputField.text = "";
    }
}
