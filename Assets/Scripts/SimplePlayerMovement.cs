using UnityEngine;

/// <summary>
/// Simple player movement script for testing the camera follow system.
/// Use arrow keys to move the player and test if camera follows smoothly.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class SimplePlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Player movement speed")]
    public float moveSpeed = 5f;

    [Tooltip("Rotation speed when changing direction")]
    public float rotationSpeed = 10f;

    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;

    void Start()
    {
        // Get or add CharacterController
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            characterController = gameObject.AddComponent<CharacterController>();
            characterController.radius = 0.5f;
            characterController.height = 2f;
            Debug.Log("SimplePlayerMovement: Added CharacterController component");
        }
    }

    void Update()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        // Get input from arrow keys or WASD (WASD may conflict with RTS camera if both are enabled)
        float horizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right arrows
        float vertical = Input.GetAxis("Vertical");     // W/S or Up/Down arrows

        // Calculate movement direction
        moveDirection = new Vector3(horizontal, 0f, vertical);

        // Apply gravity
        if (!characterController.isGrounded)
        {
            moveDirection.y -= 9.81f * Time.deltaTime;
        }

        // Move the character
        if (moveDirection.magnitude > 0.1f)
        {
            // Normalize to prevent faster diagonal movement
            Vector3 horizontalMove = new Vector3(moveDirection.x, 0f, moveDirection.z).normalized;
            
            // Rotate player to face movement direction
            if (horizontalMove != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(horizontalMove);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            // Move with speed
            Vector3 move = horizontalMove * moveSpeed * Time.deltaTime;
            move.y = moveDirection.y; // Preserve vertical velocity
            characterController.Move(move);
        }
    }

    // Display movement instructions
    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 14;
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.UpperLeft;

        string instructions = "Arrow Keys or WASD: Move Player\n(Camera should follow smoothly)";
        GUI.Label(new Rect(10, Screen.height - 60, 400, 50), instructions, style);
    }
}
