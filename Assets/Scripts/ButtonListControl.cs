using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonListControl : MonoBehaviour
{
    [SerializeField]
    private GameObject buttonTemplate;
    
    void Start()
    {    
        for (int i = 0; i < 20; i++)
        {
            GameObject button = Instantiate(buttonTemplate);
            button.SetActive(true);

            button.GetComponent<ButtonListButton>().SetText("Button #" + i);

            button.transform.SetParent(buttonTemplate.transform.parent, false);
        }
    }
}
