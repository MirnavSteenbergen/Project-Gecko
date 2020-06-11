using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlowerCounter : MonoBehaviour
{
    public static int flowerCount;
    public Text Text;

    void Start()
    {
        flowerCount = PlayerPrefs.GetInt("Flowers");
    }

    // Update is called once per frame
    void Update()
    {
        Text.text = "" + flowerCount;
    }

    public static void addFlower ()
    {
        flowerCount += 1;
        PlayerPrefs.SetInt("Flowers", flowerCount);
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.DeleteAll();
    }

}
