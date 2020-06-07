using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchInputUI : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (!Data.instance.showTouchInputUI)
        {
            gameObject.SetActive(false);
        }
    }

    void SetTouchInputUIVisible(bool value)
    {
        gameObject.SetActive(value);
    }
}
