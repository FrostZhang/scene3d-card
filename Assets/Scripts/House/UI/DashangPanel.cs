using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DashangPanel : MonoBehaviour
{
    public static DashangPanel instance;
    public Button bj;
    private void Awake()
    {
        instance = this;
        bj.onClick.AddListener(() => gameObject.SetActive(false));
        gameObject.SetActive(false);
    }
}
