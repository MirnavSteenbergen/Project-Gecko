using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTouchControls : MonoBehaviour
{
    public FixedJoystick joystick;
    private Player player;
    public bool useTouchControls = false;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (useTouchControls)
        {
            float hor = joystick.Horizontal;
            float ver = joystick.Vertical;
            if (hor > 0) hor = 1;
            if (hor < 0) hor = -1;
            if (ver > 0) ver = 1;
            if (ver < 0) ver = -1;

            player.inputMove = new Vector2(hor, ver);
        }
    }

    public void LetGoButtunDown()
    {
        player.inputLetGo = true;
    }

    public void LetGoButtunReleased()
    {
        player.inputLetGo = false;
    }
}
