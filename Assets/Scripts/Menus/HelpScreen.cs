using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HelpScreen : MonoBehaviour
{
    public Button Returnbutton;
    public Button QuitGame;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OnReturnButtonClick()
    {
        SceneManager.LoadScene("TitleScreen");
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
