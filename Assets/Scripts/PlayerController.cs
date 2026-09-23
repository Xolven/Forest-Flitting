using System;
//using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _movementSpeed;
    [SerializeField] private float _jumpForce;
    [SerializeField] private uint _jumpCount;
    [SerializeField] private float maxCoyoteTime = 0.1f;
    [SerializeField] private float maxWallJumpCoyoteTime = 0.1f;
    [SerializeField] private float defaultGravityScale = 3f;
    [SerializeField] private float wallStickCooldown = 1.2f;
    [SerializeField] private float wallStickMinTime = 0.05f;

    [SerializeField] private LayerMask _groundLayer;

    [SerializeField] private PhysicsMaterial2D checkpointPhyMat;
    [SerializeField] private PhysicsMaterial2D deathPhyMat;

    private SpriteRenderer _rndr;
    private Rigidbody2D _rb;
    private Animator _anim;
    private Transform selfTransform;

    private Vector3 respawnLocation;

    private Vector2 _movement;

    private bool canJump = false;
    private float timeWhenLastOnGround = 0f;
    private uint _currentJumpCount;
    private bool hasPreformedWallJump = false;

    private bool isStuckToWall = false;
    private bool canStickToWall = false;
    private float wallStickLeaveTime;
    private float wallStickEnterTime;
    private Vector3 wallStickDirection;

    private bool IsGrounded() => Physics2D.BoxCast(transform.position - Vector3.up, new Vector2(0.9f, 0.1f), 0, Vector2.zero, 100, _groundLayer);
    private bool IsWallLeft() => Physics2D.BoxCast(transform.position - new Vector3(0.8f, 0.4f, 0), new Vector2(0.1f, 0.9f), 0, Vector2.zero, 100, _groundLayer);
    private bool IsWallRight() => Physics2D.BoxCast(transform.position - new Vector3(-0.8f, 0.4f, 0), new Vector2(0.1f, 0.9f), 0, Vector2.zero, 100, _groundLayer);

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position - Vector3.up, new Vector2(0.9f, 0.1f));
        Gizmos.DrawWireCube(transform.position - new Vector3(0.8f, 0.4f, 0), new Vector2(0.1f, 0.9f));
        Gizmos.DrawWireCube(transform.position - new Vector3(-0.8f, 0.4f, 0), new Vector2(0.1f, 0.9f));
    }

    private void Awake()
    {
        _rndr = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        selfTransform = GetComponent<Transform>();

        respawnLocation = selfTransform.position;
    }

    private void Update()
    {
        _anim.SetBool("IsGrounded", IsGrounded());
        _anim.SetFloat("YVelocity", _rb.linearVelocityY);

        _rb.linearVelocityX = _movement.x * _movementSpeed;

        if (IsGrounded())
        {
            _currentJumpCount = _jumpCount;
            canJump = true;
            timeWhenLastOnGround = Time.time;
        } 
        else if (Time.time - timeWhenLastOnGround > maxCoyoteTime)
        {
            canJump = false;
        }
    }
    private MovementStates currentMovementState;
    private enum MovementStates
    {
        Moving,
        Jumping,
        StuckToWall,
    }

    private void FixedUpdate()
    {
        CheckCanStickToWall();
    }

    private void UnstickToWall()
    {
        isStuckToWall = false;
        _rb.gravityScale = defaultGravityScale;
        _rb.simulated = true;
        wallStickLeaveTime = Time.time;
    }
    private void StickToWall()
    {
        wallStickEnterTime = Time.time;
        currentMovementState = MovementStates.StuckToWall;
        _rb.linearVelocity = Vector3.zero;
        _rb.gravityScale = 0f;
        _rb.simulated = false;
        isStuckToWall = true;
        hasPreformedWallJump = false;
    }

    private void CheckCanStickToWall()
    {
        if (!canStickToWall && Time.time - wallStickLeaveTime > wallStickCooldown)
        {
            canStickToWall = true;
        }
        if (isStuckToWall && currentMovementState != MovementStates.StuckToWall && Time.time - wallStickEnterTime > wallStickMinTime)
        {
            UnstickToWall();
        }


        if (!canStickToWall || IsGrounded() || isStuckToWall || !(Time.time - wallStickLeaveTime > wallStickCooldown))
        {
            return;
        }

        if (IsWallLeft())
        {
            StickToWall();
            wallStickDirection = Vector3.left;
        }
        else if (IsWallRight())
        {
            StickToWall();
            wallStickDirection = Vector3.right;
        }

    }
    // WALL STICK STUFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF
    public void Move(InputAction.CallbackContext ctx)
    {
        _movement = ctx.ReadValue<Vector2>();

        _anim.SetFloat("X", _movement.x);
        if (IsGrounded() && !isStuckToWall)
        {
            currentMovementState = MovementStates.Moving;
        } 
        else if (isStuckToWall && Math.Sign(_movement.x) != Math.Sign(wallStickDirection.x) && _movement.x != 0)
        {
            currentMovementState = MovementStates.Moving;
        }
    }

    private void DoAJump(bool lowerJumpCount = true)
    {
        _rb.linearVelocityY = _jumpForce;
        _anim.SetTrigger("Jump");

        if (lowerJumpCount)
        {
            --_currentJumpCount;
        } 

        selfTransform.position += new Vector3(0, 0.11f, 0); // move a bit up to prevent accidental jump refresh on next update

        currentMovementState = MovementStates.Jumping;
    }

    private void DoAWallJump()
    {
        //selfTransform.position += wallStickDirection * -0.2f;
        hasPreformedWallJump = true;
        DoAJump(false);
    }

    public void Jump(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || !((_currentJumpCount >= 0 && canJump) || (isStuckToWall || (Time.time - wallStickLeaveTime < maxWallJumpCoyoteTime && !hasPreformedWallJump))))
        {
            return;
        } // Jump is valid only if not already preformed and it either A.Meets conditions for a regular jump or B.Meets conditions for a wall jump

        if (isStuckToWall || (Time.time - wallStickLeaveTime < maxWallJumpCoyoteTime && !hasPreformedWallJump)) 
        {
            DoAWallJump();
        }
        else 
        {
            DoAJump();
        }
    }

    public void Deactivate()
    {
        enabled = false;
        _anim.enabled = false;
        _rndr.enabled = false;
        _rb.simulated = false;

        selfTransform.position = respawnLocation;
        Activate();
    }

    public void Activate()
    {
        enabled = true;
        _anim.enabled = true;
        _rndr.enabled = true;
        _rb.simulated = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PhysicsMaterial2D physicsMat = collision.sharedMaterial; // handle enter death and checkpoint triggers
        if (!physicsMat)
        {
            Debug.LogWarning("no phmat2d D:");
            return;
        }
        
        if (physicsMat == deathPhyMat)
        {
            Deactivate();
        } else if (physicsMat == checkpointPhyMat)
        {
            respawnLocation = collision.GetComponent<Transform>().position;
        }
    }
}
