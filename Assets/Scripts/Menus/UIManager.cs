using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public void OnStartButtonClick()
    {
        SceneManager.LoadScene("TerrainStripTesting");
    }

    public void OnHelpButtonClick()
    {
        SceneManager.LoadScene("HelpScreen");
    }

    public void OnQuitButtonClick()
    {
        Application.Quit();
    }
    
    public void OnReturnButtonClick()
    {
        SceneManager.LoadScene("TitleScreen");
    }
}
