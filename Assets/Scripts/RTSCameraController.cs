using UnityEngine;

/// <summary>
/// RTS-style camera controller for top-down/isometric strategy games like Clash of Clans.
/// Provides pan, zoom, and rotation controls with boundary constraints.
/// </summary>
public class RTSCameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [Tooltip("Initial height above terrain")]
    public float initialHeight = 80f;
    
    [Tooltip("Initial angle looking down (degrees from horizontal)")]
    [Range(30f, 90f)]
    public float initialAngle = 45f;
    
    [Tooltip("Initial rotation around Y axis")]
    public float initialRotationY = 0f;

    [Header("Pan Settings")]
    [Tooltip("Camera pan speed")]
    public float panSpeed = 30f;
    
    [Tooltip("Enable edge panning (move camera when mouse near screen edge)")]
    public bool enableEdgePan = true;
    
    [Tooltip("Distance from screen edge to trigger panning (pixels)")]
    public float edgePanBorder = 20f;

    [Header("Zoom Settings")]
    [Tooltip("Zoom speed")]
    public float zoomSpeed = 20f;
    
    [Tooltip("Minimum camera height")]
    public float minHeight = 20f;
    
    [Tooltip("Maximum camera height")]
    public float maxHeight = 150f;

    [Header("Rotation Settings")]
    [Tooltip("Rotation speed (degrees per second)")]
    public float rotationSpeed = 100f;
    
    [Tooltip("Enable camera rotation")]
    public bool enableRotation = true;

    [Header("Boundaries")]
    [Tooltip("Terrain reference for calculating boundaries")]
    public Terrain terrain;
    
    [Tooltip("Boundary padding from terrain edges")]
    public float boundaryPadding = 20f;

    [Header("Smoothing")]
    [Tooltip("Camera movement smoothing")]
    [Range(0.01f, 0.5f)]
    public float smoothTime = 0.1f;

    private Vector3 targetPosition;
    private float currentHeight;
    private float currentRotationY;
    private Vector3 velocity = Vector3.zero;
    private bool initialized = false;

    void Start()
    {
        InitializeCamera();
    }

    /// <summary>
    /// Initialize camera position and rotation
    /// </summary>
    void InitializeCamera()
    {
        if (terrain == null)
        {
            terrain = FindObjectOfType<Terrain>();
            if (terrain == null)
            {
                Debug.LogWarning("RTSCameraController: No terrain found. Camera boundaries may not work correctly.");
            }
        }

        // Calculate center of terrain
        Vector3 terrainCenter = Vector3.zero;
        if (terrain != null)
        {
            terrainCenter = terrain.transform.position + new Vector3(
                terrain.terrainData.size.x * 0.5f,
                0,
                terrain.terrainData.size.z * 0.5f
            );
        }

        // Set initial position above terrain center
        currentHeight = initialHeight;
        currentRotationY = initialRotationY;
        targetPosition = terrainCenter;
        targetPosition.y = currentHeight;

        // Set rotation for isometric view
        transform.rotation = Quaternion.Euler(initialAngle, currentRotationY, 0);
        transform.position = targetPosition;

        initialized = true;
        
        Debug.Log($"RTSCameraController initialized at position {transform.position}");
    }

    void Update()
    {
        if (!initialized) return;

        HandlePanning();
        HandleZoom();
        HandleRotation();
        ApplyCameraMovement();
    }

    /// <summary>
    /// Handle camera panning with WASD/Arrow keys and optional edge panning
    /// </summary>
    void HandlePanning()
    {
        Vector3 moveDirection = Vector3.zero;

        // Keyboard input
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            moveDirection += Vector3.forward;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            moveDirection += Vector3.back;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            moveDirection += Vector3.left;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            moveDirection += Vector3.right;

        // Edge panning
        if (enableEdgePan)
        {
            Vector3 mousePos = Input.mousePosition;
            if (mousePos.x < edgePanBorder)
                moveDirection += Vector3.left;
            if (mousePos.x > Screen.width - edgePanBorder)
                moveDirection += Vector3.right;
            if (mousePos.y < edgePanBorder)
                moveDirection += Vector3.back;
            if (mousePos.y > Screen.height - edgePanBorder)
                moveDirection += Vector3.forward;
        }

        // Apply movement relative to camera rotation
        if (moveDirection != Vector3.zero)
        {
            moveDirection = Quaternion.Euler(0, currentRotationY, 0) * moveDirection;
            targetPosition += moveDirection.normalized * panSpeed * Time.deltaTime;
            targetPosition = ClampToBoundaries(targetPosition);
        }
    }

    /// <summary>
    /// Handle camera zoom with mouse wheel
    /// </summary>
    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            currentHeight -= scroll * zoomSpeed;
            currentHeight = Mathf.Clamp(currentHeight, minHeight, maxHeight);
            targetPosition.y = currentHeight;
        }
    }

    /// <summary>
    /// Handle camera rotation with Q/E keys or middle mouse button
    /// </summary>
    void HandleRotation()
    {
        if (!enableRotation) return;

        float rotationInput = 0f;

        // Keyboard rotation
        if (Input.GetKey(KeyCode.E))
            rotationInput += 1f;
        if (Input.GetKey(KeyCode.Q))
            rotationInput -= 1f;

        // Middle mouse button rotation
        if (Input.GetMouseButton(2))
        {
            rotationInput += Input.GetAxis("Mouse X");
        }

        if (rotationInput != 0)
        {
            currentRotationY += rotationInput * rotationSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Euler(initialAngle, currentRotationY, 0);
        }
    }

    /// <summary>
    /// Apply smooth camera movement
    /// </summary>
    void ApplyCameraMovement()
    {
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }

    /// <summary>
    /// Clamp camera position to terrain boundaries
    /// </summary>
    Vector3 ClampToBoundaries(Vector3 position)
    {
        if (terrain == null) return position;

        Vector3 terrainPos = terrain.transform.position;
        Vector3 terrainSize = terrain.terrainData.size;

        position.x = Mathf.Clamp(position.x, 
            terrainPos.x + boundaryPadding, 
            terrainPos.x + terrainSize.x - boundaryPadding);
        
        position.z = Mathf.Clamp(position.z, 
            terrainPos.z + boundaryPadding, 
            terrainPos.z + terrainSize.z - boundaryPadding);

        return position;
    }

    /// <summary>
    /// Focus camera on a specific position
    /// </summary>
    public void FocusOn(Vector3 position)
    {
        targetPosition = position;
        targetPosition.y = currentHeight;
        targetPosition = ClampToBoundaries(targetPosition);
    }

    /// <summary>
    /// Reset camera to initial position and rotation
    /// </summary>
    [ContextMenu("Reset Camera")]
    public void ResetCamera()
    {
        InitializeCamera();
    }

    // Display camera controls in editor
    void OnGUI()
    {
        if (Input.GetKey(KeyCode.H))
        {
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.fontSize = 14;
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.UpperLeft;
            
            string controls = 
                "=== RTS Camera Controls ===\n\n" +
                "WASD / Arrow Keys: Pan camera\n" +
                "Mouse Wheel: Zoom in/out\n" +
                "Q/E: Rotate camera\n" +
                "Middle Mouse: Hold and drag to rotate\n" +
                (enableEdgePan ? "Mouse at screen edges: Pan camera\n" : "") +
                "\nPress H to hide this help";
            
            GUI.Box(new Rect(10, 10, 350, enableEdgePan ? 200 : 180), controls, style);
        }
        else
        {
            GUIStyle hintStyle = new GUIStyle(GUI.skin.label);
            hintStyle.fontSize = 12;
            hintStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(10, 10, 200, 30), "Press H for camera controls", hintStyle);
        }
    }
}
