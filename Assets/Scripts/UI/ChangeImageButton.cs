using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeImageButton : MonoBehaviour
{
    [SerializeField]
    bool isLeft;
    Button button;
    ConfigController ccontroller;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Start()
    {
        ccontroller = GameObject.FindWithTag("Config").GetComponent<ConfigController>();
        button.onClick.AddListener(() => { ccontroller.ChangeIndex(isLeft); });
    }
}
