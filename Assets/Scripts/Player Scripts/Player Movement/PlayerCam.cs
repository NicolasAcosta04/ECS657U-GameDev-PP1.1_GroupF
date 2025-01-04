using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensX;
    public float sensY;

    public Transform orientation;

    float xRotation;
    float yRotation;

    public bool isPaused = false; // Property to determine if the game is paused

    // Start is called before the first frame update
    void Start()
    {
        if (GeneralSettings.Instance != null)
        {
            sensX = GeneralSettings.Instance.sensX;
            sensY = GeneralSettings.Instance.sensY;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isPaused) return; // Stop processing input if the game is paused

        // Get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensY;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Rotate cam and orientation
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public void UpdateSensitivity(float newSensX, float newSensY)
    {
        sensX = newSensX;
        sensY = newSensY;
    }
}
