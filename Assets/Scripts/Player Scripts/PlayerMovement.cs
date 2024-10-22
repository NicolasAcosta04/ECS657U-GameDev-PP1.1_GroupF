using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Speed")]
    public TextMeshProUGUI speed;
    public float depletedSpeed;
    public float crouchingSpeed;
    public float walkingSpeed;
    public float sprintSpeed;
    public float groundDrag;

    private float moveSpeed;

    [Header("Keybinds")]
    public KeyCode crouchingKey = KeyCode.LeftControl;
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("The Sprinting State")]
    public UnityEngine.UI.Slider staminaBar;
    public int sprintConsumptionTime = 1;
    private float sprintTimer = 0;
    private int staminaPoint = 1;

    [Header("The Depleted State")]
    public int depletedTimeframe = 8;
    private float depletedTimer = 0;

    [Header("Dehydrated")]
    public UnityEngine.UI.Slider thirstBar;
    public bool stopStaminaRegeneration = false;

    [Header("Stamina Regeneration")]
    public int staminaDelay = 2;
    private float staminaTimer = 0;

    [Header("Transform Orientation")]
    public Transform orientation;
    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;
    private Vector3 lastPos;
    Rigidbody rb;

    [Header("Movement States")]
    public MovementState state;
    public enum MovementState
    {
        Depleted,
        Crouching,
        Walking,
        Sprinting
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        lastPos = transform.position;
        staminaBar.value = 10;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    private void StateHandler()
    {
        // Mode - Crouching
        if (Input.GetKey(crouchingKey) && staminaBar.value != 0)
        {
            state = MovementState.Crouching;
            moveSpeed = crouchingSpeed;
        }

        // Mode - Sprinting
        else if (Input.GetKey(sprintKey) && staminaBar.value != 0)
        {
            state = MovementState.Sprinting;
            moveSpeed = sprintSpeed;
            sprintTimer += Time.deltaTime;

            if (sprintTimer >= sprintConsumptionTime)
            {
                DecreaseStaminaValue();
                sprintTimer = 0;
            }
        }

        // Mode - Depleted
        else if (staminaBar.value == 0)
        {
            state = MovementState.Depleted;
            moveSpeed = depletedSpeed;
            depletedTimer += Time.deltaTime;

            if (depletedTimer >= depletedTimeframe)
            {
                IncreaseStaminaValue();
                depletedTimer = 0;
            }
        }

        // Mode - Walking
        else
        {
            state = MovementState.Walking;
            moveSpeed = walkingSpeed;
        }

        // Dehydrated
        if (thirstBar.value == 0)
        {
            stopStaminaRegeneration = true;
        }

        // Out of Dehydration
        if (thirstBar.value > 0)
        {
            stopStaminaRegeneration = false;
        }

        // Stamina Regeneration
        if (state != MovementState.Sprinting && state != MovementState.Depleted && stopStaminaRegeneration == false)
        {
            StaminaRegeneration();
        }
    }

    private void StaminaRegeneration()
    {
        staminaTimer += Time.deltaTime;

        // Stamina Regeneration Delay
        if (staminaTimer >= staminaDelay)
        {
            IncreaseStaminaValue();
            staminaTimer = 0;
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

        // Handle Drag
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;

        // Speed Variable User Interface
        var currentPos = transform.position;
        var velocity = (currentPos - lastPos) / Time.fixedDeltaTime;
        speed.text = "Speed: " + velocity.magnitude.ToString("0.00");
        lastPos = currentPos;
    }
    private void IncreaseStaminaValue()
    {
        staminaBar.value += staminaPoint;
    }

    private void DecreaseStaminaValue()
    {
        staminaBar.value -= staminaPoint;
    }
}
