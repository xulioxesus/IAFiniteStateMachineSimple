using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 180f;

    private InputSystem_Actions inputActions;
    private Vector2 moveInput;

    void Awake()
    {
        // Create an instance of the generated Input Actions class
        inputActions = new InputSystem_Actions();
    }

    void OnEnable()
    {
        // Enable the Player action map
        inputActions.Player.Enable();
    }

    void OnDisable()
    {
        // Disable the Player action map when the script is disabled
        inputActions.Player.Disable();
    }

    void Update()
    {
        // Read the Move action value (Vector2: x = A/D, y = W/S)
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();

        HandleMovement();
    }

    void HandleMovement()
    {
        // Rotate with A/D (horizontal input)
        if (Mathf.Abs(moveInput.x) > 0.01f)
        {
            transform.Rotate(0f, moveInput.x * rotationSpeed * Time.deltaTime, 0f);
        }

        // Move forward/backward with W/S (vertical input)
        if (Mathf.Abs(moveInput.y) > 0.01f)
        {
            transform.position += transform.forward * moveInput.y * moveSpeed * Time.deltaTime;
        }
    }
}
