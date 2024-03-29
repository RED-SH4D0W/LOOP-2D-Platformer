using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI_EndGame : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI distance;
    [SerializeField] private TextMeshProUGUI coins;

    void Start()
    {
        GameManager manager = GameManager.instance;

        Time.timeScale = 0;

        if(manager.distance <=0)
            distance.text = "Distance: 0";
        else
            distance.text = "Distance: " + manager.distance.ToString("#,#") + "M";

        if(manager.coins <=0)
            coins.text = "Coins: 0";
        else
        coins.text = "Coins: " + manager.coins.ToString("#,#");
    }
}