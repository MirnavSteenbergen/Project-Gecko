using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Source: https://answers.unity.com/questions/1224993/how-to-save-variables-between-scenes.html

public class Data
{
    // keep constructor private
    private Data()
    {
    }

    static private Data _instance;
    static public Data instance
    {
        get
        {
            if (_instance == null)
                _instance = new Data();
            return _instance;
        }
    }
    
    public bool showTouchInputUI = true;
}
