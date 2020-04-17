using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Levelselect : MonoBehaviour
{
    public int SelectedLevel;

    public void LoadLevelSelected()
    {
        SceneManager.LoadScene(SelectedLevel);
    }
}
