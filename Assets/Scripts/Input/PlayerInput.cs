using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public float MovementSpeed;
    public float RotationSpeed;
    public float JumpHeight;
    public float gravity;
    public CharacterController Controller;

    private Vector3 movement;
    private Vector3 velocity;
    private Vector3 jumpVelocity;
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
    public void OnJump(InputValue value)
    {
        if(Controller.isGrounded)
        {
            jumpVelocity.y = Mathf.Sqrt(JumpHeight * -gravity * 2f);
        }
    }

    private void FixedUpdate()
    {
        if (Controller.isGrounded && jumpVelocity.y < 0f) jumpVelocity.y = 0f; // stops falling when we hit ground.
        if (Controller.isGrounded) velocity = movement; // only move horizontally when we touch ground.
        Controller.Move(transform.TransformDirection(velocity) * MovementSpeed * Time.fixedDeltaTime);
        var g = jumpVelocity.y >= 0f ? gravity : 2f * gravity;
        jumpVelocity.y += g * Time.fixedDeltaTime;
        Controller.Move(jumpVelocity * Time.fixedDeltaTime);

        var trotate = transform.eulerAngles + rotation * Time.fixedDeltaTime * RotationSpeed;
        transform.eulerAngles = trotate;
    }
}
