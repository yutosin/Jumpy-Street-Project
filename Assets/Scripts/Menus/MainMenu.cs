using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public Button StartButton;
    public Button HelpButton;
    public Button QuitButton;

    // Start is called before the first frame update
    void Start()
    {
        
    }

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

    // Update is called once per frame
    void Update()
    {
        
    }
}
