using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wigglebutton : MonoBehaviour
{
    public GameObject gecko;

    public void buttonpressed()
    {
        gecko.GetComponent<wigglescript>().Wiggle();
    }

}
