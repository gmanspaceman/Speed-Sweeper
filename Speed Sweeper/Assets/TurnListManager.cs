using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurnListManager : MonoBehaviour
{
    [SerializeField]
    private GameObject labelTemplate;

    private List<GameObject> labelList = new List<GameObject>();
    // Start is called before the first frame update
    public void AddPlayerToTurnList(string s)
    {
        GameObject label = Instantiate(labelTemplate);
        label.SetActive(true);
        label.GetComponent<Text>().text = s;
        label.transform.SetParent(labelTemplate.transform.parent, false);
        
        labelList.Add(label);
    }
    public void ClearnTurnList()
    {
        foreach (GameObject d in labelList)
        {
            Destroy(d);
        }
    }


}
