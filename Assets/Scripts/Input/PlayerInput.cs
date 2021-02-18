using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public float MovementSpeed;
    public float RotationSpeed;
    public float JumpForce;
    public float gravity;
    public CharacterController Controller;

    private Vector3 velocity;
    private Vector3 jumpVelocity;
    private Vector3 rotation;

    public void OnMove(InputValue value)
    {
        var v = value.Get<Vector2>();
        velocity.x = v.x;
        velocity.z = v.y;
    }
    public void OnLook(InputValue value)
    {
        var v = value.Get<Vector2>();
        rotation.x = -v.y;
        rotation.y = v.x;
    }
    public void OnJump(InputValue value)
    {
        if(Controller.isGrounded)
        {
            jumpVelocity.y = JumpForce * -gravity;
        }
    }

    private void FixedUpdate()
    {
        if (Controller.isGrounded && jumpVelocity.y < 0f) jumpVelocity.y = 0f;
        Controller.Move(transform.TransformDirection(velocity) * MovementSpeed * Time.fixedDeltaTime);
        jumpVelocity.y += gravity * Time.fixedDeltaTime;
        Controller.Move(jumpVelocity * Time.fixedDeltaTime);

        var trotate = transform.eulerAngles + rotation * Time.fixedDeltaTime * RotationSpeed;
        transform.eulerAngles = trotate;
    }
}
