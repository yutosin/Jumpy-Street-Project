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

    [SerializeField] private Animator _animator;

    private bool _canMove = false;

    private Vector3 _nextPosition;

    /*
     * Just to explain my train of thought. I decided to make the nextPosition varaible into a class varaible
     * as to commit the value to the objects "memory" where as the bool sort of as a safe proof to prevent the player
     * from moving to another cell until the coroutine finishes.*/

    void LateUpdate()
    {

        //Vector3 nextPosition;// = HandleMovement();

        PlayerMove();

        if(_canMove)
        {
            //nextPosition = HandleMovement();

            if (_nextPosition != Vector3.zero)
            {
                transform.position = new Vector3(_nextPosition.x, 1, _nextPosition.z);
                _canMove = false;
            }
        }
        
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

    private void PlayerMove() // This script checks if the player has decided to move and will start the coroutine.
    {
        //Vector3 vector = Vector3.zero;

        if(Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            _animator.SetTrigger("jumped");
            transform.rotation = Quaternion.Euler(-90, 0, 0);
            StartCoroutine(Delay(60, MoveDirection.UP));
        }
        if(Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            _animator.SetTrigger("jumped");
            transform.rotation = Quaternion.Euler(-90, -90, 0);
            StartCoroutine(Delay(60, MoveDirection.LEFT));
        }
        if(Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            _animator.SetTrigger("jumped");
            transform.rotation = Quaternion.Euler(-90, 90, 0);
            StartCoroutine(Delay(60, MoveDirection.RIGHT));
        }
        if(Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            _animator.SetTrigger("jumped");
            transform.rotation = Quaternion.Euler(-90, -180, 0);
            StartCoroutine(Delay(60, MoveDirection.DOWN));
        }
    }

    private Vector3 HandleMovement(MoveDirection direction) // Gave this script a parameter for a switch case to determine the math.
    {
        //May have a wrapper for keycodes?? like on game manager or something where a key event gets fired and theres
        //a variable attached corresponding to movedirection then we can actually just respond to that??
        
        //also might want to use getAxis code


        switch(direction)
        {
            case MoveDirection.UP:
                return stripManager.GetNextPosition(direction);
            case MoveDirection.LEFT:
                return stripManager.GetNextPosition(direction);
            case MoveDirection.RIGHT:
                return stripManager.GetNextPosition(direction);
            case MoveDirection.DOWN:
                return stripManager.GetNextPosition(direction);
        }

        return Vector3.zero;

        /*
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            _animator.SetTrigger("jumped");
            transform.rotation = Quaternion.Euler(-90, 0, 0);
            return stripManager.GetNextPosition(MoveDirection.UP);
        }
        if (Input.GetKeyDown(KeyCode.S ) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            _animator.SetTrigger("jumped");
            transform.rotation = Quaternion.Euler(-90, -180, 0);
            return stripManager.GetNextPosition(MoveDirection.DOWN);
        }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            _animator.SetTrigger("jumped");
            transform.rotation = Quaternion.Euler(-90, -90, 0);
            return stripManager.GetNextPosition(MoveDirection.LEFT);
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            _animator.SetTrigger("jumped");
            transform.rotation = Quaternion.Euler(-90, 90, 0);
            return stripManager.GetNextPosition(MoveDirection.RIGHT);
        }
        
        return Vector3.zero;
        */
    }
    private IEnumerator Delay(float waitTime, MoveDirection direction)
    {
        while(waitTime > 0)
        {
            //waitTime -= 0.1f; Changed around the values a bit, seeing as there are 60 frames for the jumping animation.
            waitTime -= 1;
            yield return null;
        }

        // At the end of the coroutine, the HandleMovement script will be called, as a coroutine cannot return the vector;
        // hence the reasoning for committing the value to the script's "memory".

        _canMove = true;
        _nextPosition = HandleMovement(direction);
    }
}
