using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScripts : MonoBehaviour
{
    private static GameScripts _sharedInstance;
    public static GameScripts SharedInstance
    {
        get { return _sharedInstance; }
    }

    public TerrainStripFactory Factory;
    public ScoreManager ScoreManager;
    public GameObject deathPanel;
    
    private void Awake()
    {
        if (_sharedInstance != null)
        {
            Destroy(gameObject);
            return;
        }
		
        _sharedInstance = this;
    }
}
