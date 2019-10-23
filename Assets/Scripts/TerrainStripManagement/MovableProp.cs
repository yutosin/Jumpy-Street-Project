using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableProp : MonoBehaviour
{
    private float _speed = 0;
    private Vector3 _direction = Vector3.zero;

    public float Speed
    {
        get { return _speed; }
        set { _speed = value; }
    }

    public Vector3 Direction
    {
        get { return _direction; }
        set { _direction = value; }
    }

    public MovablePropType Type;

    private void BoundsCheck()
    {
        //Vector3 viewPos = CameraManager.Cam.WorldToViewportPoint(transform.position);
        if (_direction.x < 0 && transform.position.x < -9.5f)
        {
            Vector3 newPos = new Vector3(9.5f, transform.position.y, transform.position.z);
            transform.position = newPos;
        }
        else if (_direction.x > 0 && transform.position.x > 9.5f)
        {
            Vector3 newPos = new Vector3(-9.5f, transform.position.y, transform.position.z);
            transform.position = newPos;
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += _direction * _speed * Time.deltaTime;
        BoundsCheck();
    }
}
