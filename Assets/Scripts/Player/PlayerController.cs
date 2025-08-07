using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using System;
public class PlayerController : MonoBehaviour
{
    [Header("PlayerComponent References")]
    [SerializeField] Rigidbody2D rb;
    [SerializeField] SpriteRenderer renderer;
    [SerializeField] Animator animator;

    [Header("Movement Settings")]
    [SerializeField] float speed;
    [SerializeField] float jumpingPower;

    [Header("Grounding")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Transform groundCheck;

    private float _horizontal;
    private bool _isMoving;
    private bool _isGrounded;

    public void Move(InputAction.CallbackContext context)
    {
        _horizontal = context.ReadValue<Vector2>().x;
        animator.Play("Walk", 0);
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);
        }

    }


    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(_horizontal * speed, rb.linearVelocity.y);
        if (_horizontal != 0)
        {
            renderer.flipX = _horizontal < 0;
        }
        _isMoving = IsMoving();
        _isGrounded = IsGrounded();
        animator.Play(AnimationSelector(), 0);
    }

    private string AnimationSelector()
    {
        if (_isGrounded && !_isMoving)
        {
            return "Idle";
        }

        if (_isGrounded && _isMoving)
        {
            return "Walk";
        }

        return "Idle";
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCapsule(groundCheck.position, new Vector2(0.5f, 1f), CapsuleDirection2D.Horizontal, 0, groundLayer);
    }

    private bool IsMoving()
    {
        return rb.linearVelocity.y != 0 && rb.linearVelocity.x != 0;
    }

}
