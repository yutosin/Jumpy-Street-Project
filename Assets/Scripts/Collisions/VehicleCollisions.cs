using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VehicleCollisions : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OnCollisionEnter(Collision collision)
        //Handles the actions involving the player and the different in-game vehicles
    {
        if(collision.gameObject.tag == "Vehicle")
            //holds the code that tells the game to reset the scene once the player hits or gets hit by a vehicle
        {
            SceneManager.LoadScene("TerrainStripTesting");
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
