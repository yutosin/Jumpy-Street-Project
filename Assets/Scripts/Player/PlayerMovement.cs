using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public TerrainStripFactory stripManager;
    
    // Update is called once per frame
    void LateUpdate()
    {
        //Repeated code; clean up
        if (Input.GetKeyDown(KeyCode.W))
        {
            Vector3 nextPosition = stripManager.GetNextPosition(MoveDirection.UP);
            transform.position = new Vector3(nextPosition.x, 1, nextPosition.z);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            Vector3 nextPosition = stripManager.GetNextPosition(MoveDirection.DOWN);
            transform.position = new Vector3(nextPosition.x, 1, nextPosition.z);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            Vector3 nextPosition = stripManager.GetNextPosition(MoveDirection.LEFT);
            transform.position = new Vector3(nextPosition.x, 1, nextPosition.z);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            Vector3 nextPosition = stripManager.GetNextPosition(MoveDirection.RIGHT);
            transform.position = new Vector3(nextPosition.x, 1, nextPosition.z);
        }
        
        Vector3 viewPos = CameraManager.Cam.WorldToViewportPoint(transform.position);
        if (viewPos.x < -0.015 || viewPos.y < -0.04)
        {
            Destroy(gameObject);
        }

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

    private void Update()
    {
//        Vector3 viewPos = CameraManager.Cam.WorldToViewportPoint(transform.position);
//        if (viewPos.x < -0.015 || viewPos.y < -0.04)
//        {
//            Destroy(gameObject);
//        }
//
//        if (viewPos.y > .6f)
//        {
//            CameraManager.speed = 2f;
//            TerrainStripFactory.GenSpeed = .5f;
//        }
//        else
//        {
//            CameraManager.speed = .5f;
//            TerrainStripFactory.GenSpeed = 1;
//        }
    }
}
