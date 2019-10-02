using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static Camera Cam;
    public static float speed;
    
    // Start is called before the first frame update
    void Start()
    {
        Cam = Camera.main;
        speed = 2f;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        /*Move the camera slowly along the z axis; speed can be modified at times by the player getting closer to the top
          of the screen*/
        Vector3 tempPos = transform.position;
        tempPos.z += speed * Time.fixedDeltaTime;
        transform.position = tempPos;
    }
}