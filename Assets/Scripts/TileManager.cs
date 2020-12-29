using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    //[SerializeField]
    //private TextMeshPro text;
    public GameObject sphere;
    public GameObject clicked;
    public GameObject unclicked;

    public void Start()
    {
        //text = new TextMeshPro();
        //text.text = "0";
    }
    public void updateNeighbor(int i)
    {
        //text = new TextMeshPro();
        //text.text = i.ToString();
    }
    public void showSphere(bool b)
    {
        sphere.SetActive(b);
    }

    public void DisplayedClickedState(bool b)
    {
        clicked.SetActive(b);
        unclicked.SetActive(!b);

    }
}
