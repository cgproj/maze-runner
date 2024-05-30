using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField, Min(0f)]
    float baseMovementSpeed = 4f, rotationSpeed = 180f, baseMouseSensitivity = 5f;

    [SerializeField]
    float startingVerticalEyeAngle = 10f;

    float movementSpeed, mouseSensitivity;

    CharacterController characterController;

    Transform eye;

    Vector2 eyeAngles;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            Debug.LogError("CharacterController not found on Player.");
        }

        if (transform.childCount > 0)
        {
            eye = transform.GetChild(0);
        }
        else
        {
            Debug.LogError("Eye (Camera) not found as child of Player.");
        }

        // Adjust speed and sensitivity based on difficulty
        int difficulty = PlayerPrefs.GetInt("masterDifficulty", 1);
        switch (difficulty)
        {
            case 0: // Easy
                movementSpeed = baseMovementSpeed * 1.25f;
                mouseSensitivity = baseMouseSensitivity * 1.25f;
                break;
            case 1: // Medium
                movementSpeed = baseMovementSpeed;
                mouseSensitivity = baseMouseSensitivity;
                break;
            case 2: // Hard
                movementSpeed = baseMovementSpeed * 0.75f;
                mouseSensitivity = baseMouseSensitivity * 0.75f;
                break;
            default:
                movementSpeed = baseMovementSpeed;
                mouseSensitivity = baseMouseSensitivity;
                break;
        }

    }


    public void StartNewGame(Vector3 position)
    {
        eyeAngles.x = Random.Range(0f, 360f);
        eyeAngles.y = startingVerticalEyeAngle;
        characterController.enabled = false;
        transform.localPosition = position;
        characterController.enabled = true;
    }

    public Vector3 Move()
    {
        UpdateEyeAngles();
        UpdatePosition();
        return transform.localPosition;
    }


    void UpdateEyeAngles()
    {
        float rotationDelta = rotationSpeed * Time.deltaTime;
        if (mouseSensitivity > 0f)
        {
            float mouseDelta = rotationDelta * mouseSensitivity;
            eyeAngles.x += mouseDelta * Input.GetAxis("Mouse X");
            eyeAngles.y -= mouseDelta * Input.GetAxis("Mouse Y");
        }

        if (eyeAngles.x > 360f)
        {
            eyeAngles.x -= 360f;
        }
        else if (eyeAngles.x < 0f)
        {
            eyeAngles.x += 360f;
        }
        eyeAngles.y = Mathf.Clamp(eyeAngles.y, -45f, 45f);
        eye.localRotation = Quaternion.Euler(eyeAngles.y, eyeAngles.x, 0f);
    }

    void UpdatePosition()
    {
        var movement = new Vector2(
            Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")
        );
        float sqrMagnitude = movement.sqrMagnitude;
        if (sqrMagnitude > 1f)
        {
            movement /= Mathf.Sqrt(sqrMagnitude);
        }
        movement *= movementSpeed;

        var forward = new Vector2(
            Mathf.Sin(eyeAngles.x * Mathf.Deg2Rad),
            Mathf.Cos(eyeAngles.x * Mathf.Deg2Rad)
        );
        var right = new Vector2(forward.y, -forward.x);

        movement = right * movement.x + forward * movement.y;
        characterController.SimpleMove(new Vector3(movement.x, 0f, movement.y));
    }

}
