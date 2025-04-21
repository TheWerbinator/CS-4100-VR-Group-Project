using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using TMPro;

public class VRUIHandler : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Canvas vrCanvas;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;

    [Header("UI Settings")]
    [SerializeField] private float uiDistance = 2f;
    [SerializeField] private float uiHeight = 1.5f;
    [SerializeField] private float uiScale = 0.002f;

    private XRRig xrRig;
    private Camera xrCamera;

    private void Start()
    {
        SetupVRUI();
    }

    private void SetupVRUI()
    {
        // Get XR components
        xrRig = FindObjectOfType<XRRig>();
        if (xrRig == null)
        {
            Debug.LogError("XRRig not found in scene!");
            return;
        }

        xrCamera = xrRig.GetComponentInChildren<Camera>();
        if (xrCamera == null)
        {
            Debug.LogError("XR Camera not found!");
            return;
        }

        // Configure VR canvas
        if (vrCanvas != null)
        {
            vrCanvas.renderMode = RenderMode.WorldSpace;
            vrCanvas.worldCamera = xrCamera;
            
            // Set canvas position and scale
            vrCanvas.transform.position = xrRig.transform.position + 
                xrRig.transform.forward * uiDistance + 
                Vector3.up * uiHeight;
            vrCanvas.transform.rotation = Quaternion.LookRotation(
                vrCanvas.transform.position - xrRig.transform.position);
            vrCanvas.transform.localScale = Vector3.one * uiScale;

            // Setup UI event system
            SetupUIEventSystem();
        }
    }

    private void SetupUIEventSystem()
    {
        // Add XR UI Input Module if not present
        if (FindObjectOfType<XRUIInputModule>() == null)
        {
            var eventSystem = FindObjectOfType<EventSystem>();
            if (eventSystem == null)
            {
                var eventSystemObj = new GameObject("EventSystem");
                eventSystem = eventSystemObj.AddComponent<EventSystem>();
            }
            eventSystem.gameObject.AddComponent<XRUIInputModule>();
        }

        // Setup button interactions
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitButtonClicked);
        }
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }

    public void UpdateTimer(float time)
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    public void ShowGameOver(bool show)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(show);
        }
    }

    private void OnRestartButtonClicked()
    {
        // Handle restart game
        Debug.Log("Restart button clicked");
    }

    private void OnQuitButtonClicked()
    {
        // Handle quit game
        Debug.Log("Quit button clicked");
    }

    public void RecenterUI()
    {
        if (vrCanvas != null && xrRig != null)
        {
            vrCanvas.transform.position = xrRig.transform.position + 
                xrRig.transform.forward * uiDistance + 
                Vector3.up * uiHeight;
            vrCanvas.transform.rotation = Quaternion.LookRotation(
                vrCanvas.transform.position - xrRig.transform.position);
        }
    }
} 