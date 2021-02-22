using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public float MovementSpeed;
    public float RotationSpeed;
    public float JumpHeight;
    public float Gravity;
    public float SelectionDistance;
    public CharacterController Controller;
    public ChunkManager ChunkManager;

    private Vector3 movement;
    private Vector3 velocity;
    private Vector3 jumpVelocity;
    private Vector3 rotation;

    public void OnMove(InputAction.CallbackContext context)
    {
        var v = context.ReadValue<Vector2>();
        movement.x = v.x;
        movement.z = v.y;
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if(Controller.isGrounded)
        {
            jumpVelocity.y = Mathf.Sqrt(JumpHeight * -Gravity * 2f);
        }
    }
    public void OnLook(InputAction.CallbackContext context)
    {
        var v = context.ReadValue<Vector2>();
        rotation.x = -v.y;
        rotation.y = v.x;
    }
    public void OnFire(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            var target = GetTarget();
            ChunkManager[target] = 0;
        }
    }
    public void OnActivate(InputAction.CallbackContext context)
    {

    }

    private Vector3Int GetTarget()
    {
        var pos = Mouse.current.position.ReadValue();
        var ray = Camera.main.ScreenPointToRay(pos);
        var hit = Physics.Raycast(ray, out var info, SelectionDistance);
        Debug.Log($"{hit}, {info.point}, {info.normal}");
        var coord = ChunkManager.GetCoordinateFromHit(info.point, info.normal);
        Debug.Log(coord);
        return coord;
    }

    private void FixedUpdate()
    {
        if (Controller.isGrounded && jumpVelocity.y < 0f) jumpVelocity.y = 0f; // stops falling when we hit ground.
        if (Controller.isGrounded) velocity = movement; // only move horizontally when we touch ground.
        Controller.Move(transform.TransformDirection(velocity) * MovementSpeed * Time.fixedDeltaTime);
        
        var g = jumpVelocity.y >= 0f ? Gravity : 2f * Gravity;
        jumpVelocity.y += g * Time.fixedDeltaTime;
        Controller.Move(jumpVelocity * Time.fixedDeltaTime);

        var trotate = transform.eulerAngles + rotation * Time.fixedDeltaTime * RotationSpeed;
        transform.eulerAngles = trotate;
    }
}
