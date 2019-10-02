using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum MoveDirection
{
    UP,
    DOWN,
    LEFT,
    RIGHT
}

public class PlayerMovement : MonoBehaviour
{
    public TerrainStripFactory stripManager;
    
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
            Destroy(gameObject);
        }
        
        //Might want to have camera speed move on a gradient?? depends on playtesting
        
        //check if the player is getting close to the top of the screen and speed up camera speed and terrain strip gen
        //in response
        if (viewPos.y > .6f)
        {
            CameraManager.speed = 2f;
            TerrainStripFactory.GenSpeed = .35f;
        }
        else
        {
            CameraManager.speed = 1f;
            TerrainStripFactory.GenSpeed = 1;
        }
    }

    private Vector3 HandleMovement()
    {
        //May have a wrapper for keycodes?? like on game manager or something where a key event gets fired and theres
        //a variable attached corresponding to movedirection then we can actually just respond to that??
        
        //also might want to use getAxis code
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            return stripManager.GetNextPosition(MoveDirection.UP);
        }
        if (Input.GetKeyDown(KeyCode.S ) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            return stripManager.GetNextPosition(MoveDirection.DOWN);
        }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            return stripManager.GetNextPosition(MoveDirection.LEFT);
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            return stripManager.GetNextPosition(MoveDirection.RIGHT);
        }
        
        return Vector3.zero;
    }
}
