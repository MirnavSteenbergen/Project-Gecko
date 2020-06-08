using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseButton : MonoBehaviour
{
    Keyboard kb;
    public PauseMenu pauseMenu;

    private void Awake()
    {

        kb = InputSystem.GetDevice<Keyboard>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (kb.escapeKey.wasPressedThisFrame)
        {
            if (!PauseMenu.gameIsPaused)
                pauseMenu.Pause();
            else
                pauseMenu.Resume();
        }
    }
}
