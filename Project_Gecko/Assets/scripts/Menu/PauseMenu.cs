using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool gameIsPaused = false;

    [Header("Touch Input UI")]
    public GameObject touchInputUI;
    public Text UIswitchButtonText;

    private void Awake()
    {
        
    }

    private void Start()
    {
        if (!Data.instance.showTouchInputUI)
        {
            touchInputUI.SetActive(false);
        }
    }

    private void Update()
    {
        
    }

    public void Pause()
    {
        gameObject.SetActive(true);
        Time.timeScale = 0f;
        gameIsPaused = true;

        UIswitchButtonText.text = (Data.instance.showTouchInputUI ? "Hide" : "Show") + " Touch Input Overlay";
    }

    public void Resume()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1f;
        gameIsPaused = false;
    }

    public void ToggleTouchOverlay()
    {
        bool showOverlay = !Data.instance.showTouchInputUI;
        Data.instance.showTouchInputUI = showOverlay;
        touchInputUI.SetActive(showOverlay);
        UIswitchButtonText.text = (showOverlay ? "Hide" : "Show") + " Touch Input Overlay";
    }

    public void GoBackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
        Time.timeScale = 1f;
        gameIsPaused = false;
    }
}
