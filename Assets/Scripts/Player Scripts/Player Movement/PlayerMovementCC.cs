using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementCC : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    [SerializeField] public float walkSpeed;
    [SerializeField] public float sprintSpeed;

    [Header("Jumping")]
    [SerializeField] public float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundCheckDistance = 0.4f;
    private bool isGrounded;

    [Header("Crouching")]
    [SerializeField] public float crouchSpeed;
    [SerializeField] public float crouchYScale = 0.5f;
    private float originalYScale;

    [Header("Keybinds")]
    [SerializeField] public KeyCode jumpKey = KeyCode.Space;
    [SerializeField] public KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    [SerializeField] public Transform groundCheck;
    [SerializeField] public LayerMask whatIsGround;

    [Header("Slope Handling")]
    [SerializeField] public float maxSlopeAngle = 45f;
    private bool onSlope;
    private Vector3 slopeNormal;

    [Header("Orientation and State")]
    [SerializeField] private DupePlayerStats stats;
    [SerializeField] public Transform orientation;

    private Vector3 moveDirection;
    private Vector3 velocity;

    private CharacterController controller;

    [SerializeField] public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        air,
        jumping
    }

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        originalYScale = transform.localScale.y;
        stats = GetComponent<DupePlayerStats>();
    }

    private void Update()
    {
        CCGroundCheck();
        CCMyInput();
        CCMovePlayer();
        CCApplyGravity();
        CCStateHandler();
    }

    private void CCGroundCheck()
    {
        // Check if grounded
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckDistance, whatIsGround);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Reset vertical velocity when grounded
        }

        onSlope = CCOnSlope();
    }

    private void CCMyInput()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // Jump logic
        if (Input.GetKey(jumpKey) && isGrounded && stats.CanJump())
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            stats.ApplyJumpPenalty();
        }

        // Crouch logic
        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            moveSpeed = crouchSpeed;
        }

        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, originalYScale, transform.localScale.z);
        }
    }

    private void CCMovePlayer()
    {
        if (onSlope)
        {
            Vector3 slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, slopeNormal);
            controller.Move(slopeMoveDirection.normalized * moveSpeed * Time.deltaTime);
        }
        else
        {
            controller.Move(moveDirection.normalized * moveSpeed * Time.deltaTime);
        }

        // Sprinting
        if (Input.GetKey(sprintKey) && isGrounded && stats.CanSprint())
        {
            moveSpeed = sprintSpeed;
            stats.SetSprinting(true);
        }
        else if (isGrounded)
        {
            moveSpeed = walkSpeed;
            stats.SetSprinting(false);
        }
    }

    private void CCApplyGravity()
    {
        if (!isGrounded)
        {
            velocity.y += gravity * Time.deltaTime; // Apply gravity
        }

        controller.Move(velocity * Time.deltaTime); // Apply vertical movement
    }

    private void CCStateHandler()
    {
        if (!isGrounded)
        {
            state = MovementState.air;
        }
        else if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
        }
        else if (Input.GetKey(sprintKey) && stats.CanSprint())
        {
            state = MovementState.sprinting;
        }
        else
        {
            state = MovementState.walking;
        }
    }

    private bool CCOnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, controller.height / 2 + 0.5f))
        {
            float angle = Vector3.Angle(Vector3.up, hit.normal);
            if (angle < maxSlopeAngle && angle > 0)
            {
                slopeNormal = hit.normal;
                return true;
            }
        }

        return false;
    }
}
