﻿using System.Collections;
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
    public static GameObject playerObject;
    public static bool isInRiver = false;

    private void Awake()
    {
        playerObject = gameObject;
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
            Destroy(gameObject);
        }
        
        //To do: modify this to somehow keep consistent speed on camera when close to bottom of screen?
//        if (viewPos.y > .6f)
//        {
//            CameraManager.speed = 2f;
//        }
//        else
//        {
//            CameraManager.speed = 1f;
//        }
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
}
