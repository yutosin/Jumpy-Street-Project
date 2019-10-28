using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraManager : MonoBehaviour
{
    private Vector3 _goalPos;
    private static float _speed;
    
    public static Camera Cam;

    public static float Speed
    {
        get { return _speed;  }
        set { _speed = Mathf.Clamp(value, .1f, .4f);  }
    }

    public Transform target;
    
    // Start is called before the first frame update
    void Start()
    {
        Cam = Camera.main;
        Speed = .3f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        /*Move the camera slowly along the z axis; speed can be modified at times by the player getting closer to the top
          of the screen*/
        if (!target)
            return;
        _goalPos = Vector3.Lerp(transform.position, target.position, Time.deltaTime * Speed);
        transform.position = new Vector3(transform.position.x, transform.position.y, _goalPos.z);
    }
}