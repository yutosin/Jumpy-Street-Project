using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public Text ScoreText, HighScoreText;
    
    private int Score;
    


    // Start is called before the first frame update
    void Start()
    {
        Score = 0;

        ScoreText.text = Score.ToString();
        int storedHighScore = PlayerPrefs.GetInt("HighScore",0);
        HighScoreText.text = storedHighScore.ToString();

    }

    public void IncrementScore()
    {
        Score++;
        ScoreText.text = Score.ToString();
    }

    public void UpdateHighScore()
    {
        HighScoreText.gameObject.SetActive(true);
        int storedHighScore = PlayerPrefs.GetInt("HighScore",0);
        if(Score > storedHighScore)
        {
            PlayerPrefs.SetInt("HighScore", Score);
            storedHighScore = Score;
        }
        HighScoreText.text = storedHighScore.ToString();
    }
}
