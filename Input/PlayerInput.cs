using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public float MovementSpeed;
    public float RotationSpeed;

    private Vector3 movement = new Vector3();
    private Vector3 rotation = new Vector3();

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

    private void Update()
    {
        transform.Translate(movement * Time.deltaTime * MovementSpeed);
        transform.eulerAngles += rotation * Time.deltaTime * RotationSpeed;
    }
}
