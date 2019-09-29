using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public TerrainStripFactory stripManager;
    // Update is called once per frame
    void LateUpdate()
    {
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
    }
}
