using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float jogSpeed;
    public float sprintSpeed;

    public float groundDrag;

    public TextMeshProUGUI speed;

    [Header("Keybinds")]
    public KeyCode jogkey = KeyCode.LeftControl;
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    private Vector3 lastPos;

    Rigidbody rb;

    public MovementState state;

    public enum MovementState
    {
        Walking,
        Jogging,
        Sprinting
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        lastPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();

        // handle drag

        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
        var currentPos = transform.position;
        var velocity = (currentPos - lastPos) / Time.fixedDeltaTime;
        speed.text = "Speed: " + velocity.magnitude.ToString("0.00");

        lastPos = currentPos;
    }

    private void MyInput() 
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    private void StateHandler()
    {
        // Mode - jogging
        if (Input.GetKey(jogkey))
        {
            state = MovementState.Jogging;
            moveSpeed = jogSpeed;
        }
        // Mode - sprinting
        else if (Input.GetKey(sprintKey))
        {
            state = MovementState.Sprinting;
            moveSpeed = sprintSpeed;
        }
        // Mode - walking
        else
        {
            state = MovementState.Walking;
            moveSpeed = walkSpeed;
        }
    }

    private void MovePlayer()
    { 
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // limit velocity if needed
        if (flatVel.magnitude > moveSpeed)
        { 
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }
}
