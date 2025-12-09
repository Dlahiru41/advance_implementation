using UnityEngine;
using UnityEngine.UI;

namespace NPCAISystem
{
    /// <summary>
    /// Displays NPC ID and health bar above the NPC using world-space canvas.
    /// Shows color-coded health bar (green/yellow/red) and NPC identifier.
    /// </summary>
    [RequireComponent(typeof(NPCHealth))]
    public class NPCHealthDisplay : MonoBehaviour
    {
        [Header("Display Settings")]
        [Tooltip("Enable health display")]
        public bool enableDisplay = true;

        [Tooltip("Height offset above NPC for display")]
        public float heightOffset = 2f;

        [Tooltip("Scale of the canvas")]
        public float canvasScale = 0.01f;

        [Tooltip("Health bar width")]
        public float healthBarWidth = 100f;

        [Tooltip("Health bar height")]
        public float healthBarHeight = 15f;

        [Header("Colors")]
        [Tooltip("Color when health is high (>70%)")]
        public Color healthyColor = Color.green;

        [Tooltip("Color when health is medium (30-70%)")]
        public Color warnColor = Color.yellow;

        [Tooltip("Color when health is low (<30%)")]
        public Color criticalColor = Color.red;

        [Tooltip("Background color")]
        public Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        // Components
        private NPCHealth npcHealth;
        private Canvas canvas;
        private Image healthBarBackground;
        private Image healthBarFill;
        private Text npcIdText;
        private GameObject canvasObject;
        private string cachedDisplayName; // Cache the display name
        private float lastHealthPercentage = 1f; // Track last health for change detection

        void Start()
        {
            npcHealth = GetComponent<NPCHealth>();
            
            if (enableDisplay)
            {
                cachedDisplayName = GetNPCDisplayName(); // Cache name once
                CreateHealthDisplay();
            }
        }

        void Update()
        {
            if (!enableDisplay || canvasObject == null || npcHealth == null)
                return;

            // Update position to follow NPC
            canvasObject.transform.position = transform.position + Vector3.up * heightOffset;

            // Make canvas face the camera
            if (Camera.main != null)
            {
                canvasObject.transform.LookAt(canvasObject.transform.position + Camera.main.transform.rotation * Vector3.forward,
                                              Camera.main.transform.rotation * Vector3.up);
            }

            // Only update health bar if health changed (performance optimization)
            float currentHealthPercentage = npcHealth.GetHealthPercentage();
            if (Mathf.Abs(currentHealthPercentage - lastHealthPercentage) > 0.001f)
            {
                UpdateHealthBar();
                lastHealthPercentage = currentHealthPercentage;
            }

            // Hide display if NPC is dead
            if (npcHealth.IsDead())
            {
                canvasObject.SetActive(false);
            }
        }

        /// <summary>
        /// Create the world-space canvas with health bar and ID text
        /// </summary>
        private void CreateHealthDisplay()
        {
            // Create canvas object
            canvasObject = new GameObject("NPCHealthDisplay");
            canvasObject.transform.SetParent(transform);
            canvasObject.transform.position = transform.position + Vector3.up * heightOffset;

            // Add canvas component
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            // Configure canvas
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(200, 60);
            canvasObject.transform.localScale = Vector3.one * canvasScale;

            // Add CanvasScaler for proper scaling
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10;

            // Create ID text
            GameObject textObj = new GameObject("NPCIdText");
            textObj.transform.SetParent(canvasObject.transform, false);
            npcIdText = textObj.AddComponent<Text>();
            npcIdText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            npcIdText.fontSize = 14;
            npcIdText.alignment = TextAnchor.MiddleCenter;
            npcIdText.color = Color.white;
            npcIdText.text = GetNPCDisplayName();

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchoredPosition = new Vector2(0, 15);
            textRect.sizeDelta = new Vector2(180, 20);

            // Create health bar background
            GameObject bgObj = new GameObject("HealthBarBackground");
            bgObj.transform.SetParent(canvasObject.transform, false);
            healthBarBackground = bgObj.AddComponent<Image>();
            healthBarBackground.color = backgroundColor;

            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchoredPosition = new Vector2(0, -10);
            bgRect.sizeDelta = new Vector2(healthBarWidth, healthBarHeight);

            // Create health bar fill
            GameObject fillObj = new GameObject("HealthBarFill");
            fillObj.transform.SetParent(bgObj.transform, false);
            healthBarFill = fillObj.AddComponent<Image>();
            healthBarFill.color = healthyColor;

            RectTransform fillRect = fillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0, 0);
            fillRect.anchorMax = new Vector2(1, 1);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
        }

        /// <summary>
        /// Update the health bar fill and color based on current health
        /// </summary>
        private void UpdateHealthBar()
        {
            if (healthBarFill == null || npcHealth == null)
                return;

            // Update fill amount
            float healthPercentage = npcHealth.GetHealthPercentage();
            healthBarFill.fillAmount = healthPercentage;

            // Update color based on health percentage
            if (healthPercentage > 0.7f)
            {
                healthBarFill.color = healthyColor;
            }
            else if (healthPercentage > 0.3f)
            {
                healthBarFill.color = warnColor;
            }
            else
            {
                healthBarFill.color = criticalColor;
            }

            // Update text with current HP using cached display name
            if (npcIdText != null)
            {
                npcIdText.text = $"{cachedDisplayName}\nHP: {npcHealth.GetCurrentHealth():F0}/{npcHealth.maxHealth:F0}";
            }
        }

        /// <summary>
        /// Get the display name for this NPC (called once at start and cached)
        /// </summary>
        private string GetNPCDisplayName()
        {
            // Try to get a meaningful name
            NPCController controller = GetComponent<NPCController>();
            if (controller != null)
            {
                string npcType = controller.isWeakNPC ? "Weak" : "Combat";
                // Extract number from GameObject name if it exists (e.g., "NPC_5" -> "5")
                string name = gameObject.name;
                
                // Find the last sequence of digits in the name
                int lastDigitIndex = -1;
                int firstDigitIndex = -1;
                for (int i = name.Length - 1; i >= 0; i--)
                {
                    if (char.IsDigit(name[i]))
                    {
                        if (lastDigitIndex == -1)
                            lastDigitIndex = i;
                        firstDigitIndex = i;
                    }
                    else if (lastDigitIndex != -1)
                    {
                        // Found start of digit sequence
                        break;
                    }
                }
                
                if (lastDigitIndex != -1)
                {
                    string number = name.Substring(firstDigitIndex, lastDigitIndex - firstDigitIndex + 1);
                    return $"{npcType} NPC #{number}";
                }
                return $"{npcType} NPC";
            }
            
            return gameObject.name;
        }

        /// <summary>
        /// Enable or disable the health display
        /// </summary>
        public void SetDisplayEnabled(bool enabled)
        {
            enableDisplay = enabled;
            if (canvasObject != null)
            {
                canvasObject.SetActive(enabled);
            }
        }

        void OnDestroy()
        {
            // Clean up canvas when NPC is destroyed
            if (canvasObject != null)
            {
                Destroy(canvasObject);
            }
        }
    }
}
