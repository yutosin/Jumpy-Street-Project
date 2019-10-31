using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum MoveDirection
{
    UP,
    DOWN,
    LEFT,
    RIGHT
}

public class PlayerMovement : MonoBehaviour
{
    public static GameObject playerObject;
    public static bool isInRiver = false;
    public bool IgnoreCars = false;

    private void Awake()
    {
        playerObject = gameObject;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    void LateUpdate()
    {
        Vector3 nextPosition = HandleMovement();
        
        if (nextPosition != Vector3.zero)
            transform.position = new Vector3(nextPosition.x, 1, nextPosition.z);
        
        BoundsCheck();
    }

    private void BoundsCheck()
    {
        //By converting player position to viewport point we can check if it's out of the camera view simply by checking
        //if the x and y values are less than a value outside of the camera; not exactly 0,0 because of player pivot point
        Vector3 viewPos = CameraManager.Cam.WorldToViewportPoint(transform.position);
        if (viewPos.x < -0.015 || viewPos.y < -0.04)
        {
            GameScripts.SharedInstance.deathPanel.SetActive(true);
            Destroy(gameObject);
        }
    }

    private Vector3 HandleMovement()
    {
        //May have a wrapper for keycodes?? like on game manager or something where a key event gets fired and theres
        //a variable attached corresponding to movedirection then we can actually just respond to that??
        
        //also might want to use getAxis code
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            return TerrainStripFactory.SharedInstance.GetNextPosition(MoveDirection.UP);
        }
        if (Input.GetKeyDown(KeyCode.S ) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            return TerrainStripFactory.SharedInstance.GetNextPosition(MoveDirection.DOWN);
        }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            return TerrainStripFactory.SharedInstance.GetNextPosition(MoveDirection.LEFT);
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            return TerrainStripFactory.SharedInstance.GetNextPosition(MoveDirection.RIGHT);
        }
        
        return Vector3.zero;
    }

    private void OnTriggerEnter(Collider collision)
    //Handles the actions involving the player and the different in-game vehicles
    {
        if (collision.gameObject.tag == "Vehicle" && !IgnoreCars)
        //holds the code that tells the game to reset the scene once the player hits or gets hit by a vehicle
        {
            GameScripts.SharedInstance.deathPanel.SetActive(true);
            Destroy(gameObject);
        }

    }

    private void OnDestroy()
    {
        GameScripts.SharedInstance.ScoreManager.UpdateHighScore();
    }
}
