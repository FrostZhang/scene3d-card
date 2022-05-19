using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonList : MonoBehaviour
{
    public Button prefab;
    List<Button> buttons;
    System.Action<string> callback;

    private static ButtonList instance;
    Button self;
    public static ButtonList Instance
    {
        get { return instance; }
    }

    void Awake()
    {
        if (instance == null)
            instance = this;
        self = GetComponent<Button>();
        self.onClick.AddListener(() => Show(false, null));
        buttons = new List<Button>(100);
        Show(false, null);
    }

    void OnEnable()
    {
        prefab.gameObject.SetActive(true);
        var p1 = Instantiate(prefab, prefab.transform.parent);
        p1.GetComponentInChildren<Text>().text = string.Empty;
        p1.onClick.AddListener(() => callback?.Invoke(string.Empty));
        buttons.Add(p1);
        foreach (var item in House.Instance.HomeassistantEntities)
        {
            var p = Instantiate(prefab, prefab.transform.parent);
            var t = p.GetComponentInChildren<Text>().text = item;
            p.onClick.AddListener(() => callback?.Invoke(t));
            buttons.Add(p);
        }
        prefab.gameObject.SetActive(false);
    }

    void OnDisable()
    {
        foreach (var item in buttons)
        {
            Destroy(item.gameObject);
        }
        buttons.Clear();
    }

    public void Show(bool show, System.Action<string> back)
    {
        this.callback = back;
        if (show)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
