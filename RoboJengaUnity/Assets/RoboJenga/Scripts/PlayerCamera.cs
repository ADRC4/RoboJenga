using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    float _speed = 0.05f;
    float _lookSpeed = 1.0f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        Move();
    }

    void Move()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        float z = Input.GetAxisRaw("Forward");
        Vector3 velocity = new Vector3(x, y, z).normalized * _speed;

        transform.position += transform.rotation * velocity;

        float yaw = Input.GetAxis("Mouse X") * _lookSpeed;
        float pitch = Input.GetAxis("Mouse Y") * _lookSpeed;

        transform.Rotate(Vector3.up, yaw, Space.World);
        transform.Rotate(Vector3.right, pitch, Space.Self);
    }
}
