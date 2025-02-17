using Unity.Netcode;
using UnityEngine;

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
    private bool isGrounded;
    private bool isSprinting;

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

    
    private void FixedUpdate()
    {
        if (!IsOwner)
        {
            return;
        }

        if (rb2D.linearVelocity.y < 0)
        {
            rb2D.linearVelocity += Physics2D.gravity.y * fallMultiplier * Vector2.up * Time.deltaTime;
        }
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
        if (perviousMovementInput.x > 0 && !isFacingRight)
        {
            transform.localScale = new Vector3(1, 1, 1);
            isFacingRight = true;
        }
            
        else if (perviousMovementInput.x < 0 && isFacingRight)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            isFacingRight = false;
        }
    }
    
    private void JumpPlayer()
    {
        if (isGrounded)
        {
            rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, jumpForce);
        }
    }

    private void CutJumpShort()
    {
        if (rb2D.linearVelocity.y > 0)
        {
            rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, rb2D.linearVelocity.y * 0.5f);
        }
    }
    
    private void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void HandleMove(Vector2 movementInput)
    {
        perviousMovementInput = movementInput;
    }
}
