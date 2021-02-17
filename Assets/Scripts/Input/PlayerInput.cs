using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public float MovementSpeed;
    public float RotationSpeed;
    public CharacterController Controller;

    private Vector3 movement;
    private Vector3 rotation;

    public void OnMove(InputValue value)
    {
        var v = value.Get<Vector2>();
        movement.x = v.x;
        movement.z = v.y;
    }

    public void OnLook(InputValue value)
    {
        var v = value.Get<Vector2>();
        rotation.x = -v.y;
        rotation.y = v.x;
    }

    private void FixedUpdate()
    {
        if (Controller.enabled)
        {
            Controller.SimpleMove(transform.TransformDirection(movement) * MovementSpeed);
        }
        else
        {
            transform.Translate(movement * MovementSpeed * Time.deltaTime);
        }
        var trotate = transform.eulerAngles + rotation * Time.deltaTime * RotationSpeed;
        transform.eulerAngles = trotate;
    }
}
