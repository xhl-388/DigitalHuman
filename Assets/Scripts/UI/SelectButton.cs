using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectButton : MonoBehaviour
{
    ConfigController ccontroller;
    Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Start()
    {
        ccontroller = GameObject.FindWithTag("Config").GetComponent<ConfigController>();
        button.onClick.AddListener(ccontroller.OnSelectOver);
    }
}
