using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Movement speed in units per second
    public float moveSpeed = 5f;

    // If true the player will face movement direction
    public bool faceMovementDirection = true;

    // If true the script will only respond to arrow-key input when the GameObject is tagged "Player"
    public bool onlyControlIfPlayerTag = true;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogWarning("PlayerController requires a Rigidbody. Please add one to the Player GameObject.");
        }
        else
        {
            // Improve collision behavior to avoid tunneling through terrain
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.freezeRotation = true; // prevent physics from tipping the player over
            rb.useGravity = true;
        }
    }

    void FixedUpdate()
    {
        // If configured, ignore input for any object not tagged "Player".
        if (onlyControlIfPlayerTag && !gameObject.CompareTag("Player"))
        {
            return;
        }

        // Read arrow keys only
        float x = 0f;
        float z = 0f;

        if (Input.GetKey(KeyCode.LeftArrow))  x = -1f;
        else if (Input.GetKey(KeyCode.RightArrow)) x = 1f;

        if (Input.GetKey(KeyCode.UpArrow))    z = 1f;
        else if (Input.GetKey(KeyCode.DownArrow)) z = -1f;

        Vector3 input = new Vector3(x, 0f, z);

        if (input.sqrMagnitude > 1f) input.Normalize();

        Vector3 targetVelocity = input * moveSpeed;

        if (rb != null)
        {
            // MovePosition preserves physics interactions and collision resolution.
            // Only change X/Z position here; keep Y from physics (gravity/collisions).
            Vector3 displacement = new Vector3(targetVelocity.x, 0f, targetVelocity.z) * Time.fixedDeltaTime;
            Vector3 newPosition = rb.position + displacement;
            newPosition.y = rb.position.y; // keep vertical movement controlled by physics
            rb.MovePosition(newPosition);
        }
        else
        {
            // Fallback if no Rigidbody: simple transform move (not recommended for physics collisions)
            transform.position += input * moveSpeed * Time.fixedDeltaTime;
        }

        if (faceMovementDirection && input.sqrMagnitude > 0.01f)
        {
            Vector3 lookDir = new Vector3(input.x, 0f, input.z);
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.fixedDeltaTime);
        }
    }
}
