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
    [SerializeField] private LayerMask _groundLayer;

    [SerializeField] private float maxCoyoteTime = 1f;
    [SerializeField] private PhysicsMaterial2D checkpointPhyMat;
    [SerializeField] private PhysicsMaterial2D deathPhyMat;

    private SpriteRenderer _rndr;
    private Rigidbody2D _rb;
    private Animator _anim;

    private Transform selfTransform;
    private bool canJump = false;
    private float timeWhenLastOnGround = 0f;

    private Vector3 respawnLocation;

    private Vector2 _movement;
    private uint _currentJumpCount;

    private bool IsGrounded() => Physics2D.BoxCast(transform.position - Vector3.up, new Vector2(0.9f, 0.1f), 0, Vector2.zero, 100, _groundLayer);

    // WALL STICK STUFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF
    private bool IsWallLeft() => Physics2D.BoxCast(transform.position - new Vector3(0.8f, 0.4f, 0), new Vector2(0.1f, 0.9f), 0, Vector2.zero, 100, _groundLayer);
    private bool IsWallRight() => Physics2D.BoxCast(transform.position - new Vector3(-0.8f, 0.4f, 0), new Vector2(0.1f, 0.9f), 0, Vector2.zero, 100, _groundLayer);

    private bool isStuckToWall = false;
    private bool canStickToWall = false;

    [SerializeField] private float defaultGravityScale = 3f;
    [SerializeField] private float wallStickCooldown = 1f;
    [SerializeField] private float wallStickMinTime = 0.2f;
    private float wallStickLeaveTime;
    private float wallStickEnterTime;
    private Vector3 stuckWallDirection;
    // WALL STICK STUFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF
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
        //if (IsWallLeft())
        //{
        //    Debug.Log("Touch wall left");
        //}
        //if (IsWallRight())
        //{
        //    Debug.Log("Toucehd wall righ");
        //}

        _anim.SetBool("IsGrounded", IsGrounded());
        _anim.SetFloat("YVelocity", _rb.linearVelocityY);

        _rb.linearVelocityX = _movement.x * _movementSpeed;

        // flips spritesssss
        //if (_movement.x > 0)
        //    _rndr.flipX = false;
        //else if (_movement.x < 0)
        //    _rndr.flipX = true;

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

    private void FixedUpdate()// WALL STICK STUFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF
    {
        CheckCanStickToWall();
    }

    private void UnstickToWall()
    {
        //return;
        Debug.Log("unstick check");
        if (Time.time - wallStickEnterTime < wallStickMinTime)
        {
            Debug.Log(Time.time - wallStickEnterTime);
            Debug.Log(wallStickMinTime);
            return;
        }
        Debug.Log("unstuck from wall");
        selfTransform.position += stuckWallDirection * -1.15f;
        _rb.gravityScale = defaultGravityScale;
        _rb.simulated = true;
        canStickToWall = false;
        isStuckToWall = false;
        wallStickLeaveTime = Time.time;
    }
    private void StickToWall()
    {
        //return;
        Debug.Log("stuck to wall");
        _rb.gravityScale = 0;
        _rb.simulated = false;
        //_currentJumpCount = _jumpCount;
        isStuckToWall = true;
        wallStickEnterTime = Time.time;
    }

    private void CheckCanStickToWall()
    {
        //return;
        if (!canStickToWall && Time.time - wallStickLeaveTime > wallStickCooldown)
        {
            canStickToWall = true;
        }
        if (isStuckToWall || !canStickToWall)
        {
            return;
        }

        if (IsWallLeft())
        {
            StickToWall();
            stuckWallDirection = Vector3.left;
        }
        else if (IsWallRight())
        {
            StickToWall();
            stuckWallDirection = Vector3.left;
        }

    }
    // WALL STICK STUFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF
    public void Move(InputAction.CallbackContext ctx)
    {
        if (isStuckToWall)
        {
            UnstickToWall();
        }
        _movement = ctx.ReadValue<Vector2>();

        _anim.SetFloat("X", _movement.x);
    }

    public void Jump(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || ((_currentJumpCount <= 0 || !canJump) && !isStuckToWall)) // only jump if action not preformed + you are allowed to jump
        {
            return;
        }

        if (isStuckToWall)
        {
            UnstickToWall();
        }

        _rb.linearVelocityY = _jumpForce;
        _anim.SetTrigger("Jump");

        --_currentJumpCount;

        selfTransform.position += new Vector3(0,0.1f,0); // move a bit up to prevent accidental jump refresh on next update
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
