using UnityEngine.SceneManagement;
using Unity.Netcode;
using UnityEngine;
using System;

public class PlayerMovement : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Rigidbody2D rb2D;
    
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float sprintSpeed = 15f;
    [SerializeField] private float jumpForce = 25f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    
    private Vector2 perviousMovementInput;
    private bool isFacingRight = true;
    public NetworkVariable<bool> IsFacingRight = new NetworkVariable<bool>(true);
    private bool isGrounded;
    private bool isSprinting;
    private bool isMapSelectionScene;

    [SerializeField] private Animator anim;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            return;
        }
        inputReader.MoveEvent += HandleMove;
        inputReader.JumpEvent += HandleJump;
        inputReader.SprintEvent += HandleSprint;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner)
        {
            return;
        }
        inputReader.MoveEvent -= HandleMove;
        inputReader.JumpEvent -= HandleJump;
        inputReader.SprintEvent -= HandleSprint;
    }
    
    private void HandleJump(bool isPressed)
    {
        if (isPressed)
        {
            JumpPlayer();
        }
        else
        {
            CutJumpShort();
        }
    }

    private void HandleSprint(bool isPressed)
    {
        if (isPressed)
        {
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
        }
    }

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    
    private void FixedUpdate()
    {
        if (!IsOwner)
        {
            return;
        }
        
        isMapSelectionScene = SceneManager.GetActiveScene().name == "MapSelection";
        if (isMapSelectionScene)
        {
            transform.position = Vector3.zero;
            rb2D.gravityScale = 0;
        }
        else
        {
            rb2D.gravityScale = 4;
        }
        
        if (isMapSelectionScene)
        {
            rb2D.linearVelocity = Vector2.zero;
            return;
        }

        if (rb2D.linearVelocity.y < 0)
        {
            rb2D.linearVelocity += Physics2D.gravity.y * fallMultiplier * Vector2.up * Time.deltaTime;
        }

        anim.SetFloat("X", Math.Abs(rb2D.linearVelocity.x));
        anim.SetFloat("Y", rb2D.linearVelocity.y);

        CheckGrounded();
        MovePlayer();
        Flip();
    }

    private void MovePlayer()
    {
        if (isSprinting)
        {
            rb2D.linearVelocity = new Vector2(perviousMovementInput.x * sprintSpeed, rb2D.linearVelocity.y);
        }
        else
        {
            rb2D.linearVelocity = new Vector2(perviousMovementInput.x * moveSpeed, rb2D.linearVelocity.y);
        }
    }
    
    void Flip()
    {
        if (perviousMovementInput.x > 0 && !IsFacingRight.Value)
        {
            transform.localScale = new Vector3(1, 1, 1);
            FlipServerRpc(true);
        }
            
        else if (perviousMovementInput.x < 0 && IsFacingRight.Value)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            FlipServerRpc(false);
        }
    }
    
    [ServerRpc]
    private void FlipServerRpc(bool facingRight)
    {
        IsFacingRight.Value = facingRight;
        transform.localScale = new Vector3(facingRight ? 1 : -1, 1, 1);
    }
    
    private void JumpPlayer()
    {
        if (isGrounded)
        {
            rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, jumpForce);
            anim.SetBool("IsJumping", true);
        }
    }

    private void CutJumpShort()
    {
        if (rb2D.linearVelocity.y > 0)
        {
            rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, rb2D.linearVelocity.y * 0.5f);
            anim.SetBool("IsJumping", false);
        }
    }
    
    private void CheckGrounded()
    {
        Collider2D[] hitColliders = new Collider2D[3];
        int numColliders = Physics2D.OverlapCircleNonAlloc(
            groundCheck.position, 
            groundCheckRadius, 
            hitColliders, 
            groundLayer
        );

        isGrounded = false;

        for (int i = 0; i < numColliders; i++)
        {
            if (hitColliders[i].gameObject != gameObject)
            {
                isGrounded = true;
                break;
            }
        }
    }

    private void HandleMove(Vector2 movementInput)
    {
        perviousMovementInput = movementInput;
    }
}
