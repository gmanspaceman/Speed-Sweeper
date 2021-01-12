using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SoloGameManager : MonoBehaviour
{
    public GameObject continueGame;

    // Start is called before the first frame update
    void Awake()
    {
        continueGame.SetActive(PlayerPrefs.HasKey("LocalSave"));
    }

    
}
