using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DupePlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    [SerializeField] public float walkSpeed;
    [SerializeField] public float sprintSpeed;

    [SerializeField] public float groundDrag;

    [Header("Jumping")]
    [SerializeField] public float jumpForce;
    [SerializeField] public float jumpCooldown;
    [SerializeField] public float airMultiplier;
    bool readyToJump;

    [Header("Crouching")]
    [SerializeField] public float crouchSpeed;
    [SerializeField] public float crouchYScale;
    private float startYScale;

    [Header("Keybinds")]
    [SerializeField] public KeyCode jumpKey = KeyCode.Space;
    [SerializeField] public KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    [SerializeField] public float playerHeight;
    [SerializeField] public LayerMask whatIsGround;
    bool grounded;

    [Header("Slope Handling")]
    [SerializeField] public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;
    
    [Header("Orientation and State")]
    [SerializeField] private DupePlayerStats stats;
    [SerializeField] public Transform orientation;

    float horizontalInput;
    float verticalInput;
    

    Vector3 moveDirection;

    Rigidbody rb;

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
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;
        startYScale = transform.localScale.y;
        stats = GetComponent<DupePlayerStats>();
    }




    private void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        //Debug.DrawRay(transform.position, Vector3.down * (playerHeight * 0.5f + 0.2f), grounded ? Color.green : Color.red);

        MyInput();
        SpeedControl();
        StateHandler();

        if (grounded)
            {rb.drag = groundDrag;}
        else
            {rb.drag = 0;}
    }




    private void FixedUpdate()
    {
        MovePlayer();
    }




    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if(Input.GetKey(jumpKey) && readyToJump && grounded && stats.CanJump())
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }




    private void StateHandler()
    {
        if (readyToJump == false) // Player is in the middle of a jump
        {
            if (state != MovementState.jumping) // Ensure penalty is applied only once per jump
            {
                stats.ApplyJumpPenalty(); // Deduct stamina for jumping
            }
            state = MovementState.jumping;
        }
        else if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }
        else if (grounded && Input.GetKey(sprintKey) && stats.CanSprint())
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
            stats.SetSprinting(true); // Notify stats that sprinting is active
        }
        else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
            stats.SetSprinting(false); // Notify stats that sprinting is inactive
        }
        else
        {
            state = MovementState.air;
            stats.SetSprinting(false); // Ensure stamina regenerates when not sprinting
        }
    }




    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 50f, ForceMode.Force);

            if (rb.velocity.y > 0){
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);}

            float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
            rb.drag = Mathf.Lerp(groundDrag, 5f, slopeAngle / maxSlopeAngle);
        }

        else if(grounded){
            rb.drag = groundDrag;
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);}

        else if(!grounded){
            rb.drag = 0f;
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);}
    }




    private void SpeedControl()
    {
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed){
                rb.velocity = rb.velocity.normalized * moveSpeed;}
        }

        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }




    private void Jump()
    {
        exitingSlope = true;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }




    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }




    private bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }




    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }
}