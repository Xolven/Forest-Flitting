using System;
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

    private bool isStuckOnWall = false;

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
    [SerializeField] private float wallStickRefreshDuration = 1f;
    private float wallStickLeaveTime;
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
        //CheckWallStick();
    }

    private void LeaveWall()
    {
        _rb.gravityScale = defaultGravityScale;
        canStickToWall = false;
        isStuckOnWall = false;
        wallStickLeaveTime = Time.time;
    }
    private void StickToWall(int direction)
    {
        _rb.gravityScale = 0;
        _currentJumpCount = _jumpCount;
        isStuckOnWall = true;
    }
    private void CheckWallStick()
    {
        /* Need to make jumpting have a "wall jump mode"
         * AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
         * 
         */
    }
    // WALL STICK STUFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF
    public void Move(InputAction.CallbackContext ctx)
    {
        _movement = ctx.ReadValue<Vector2>();

        _anim.SetFloat("X", _movement.x);
    }

    public void Jump(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || _currentJumpCount <= 0 || (!canJump))
        {
            return;
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
        PhysicsMaterial2D physicsMat = collision.sharedMaterial;
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
