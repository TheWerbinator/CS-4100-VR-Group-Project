using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;

public class VRSettingsMenu : NetworkBehaviour
{
    [Header("Menu References")]
    [SerializeField] private Transform menuTransform;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Image backgroundPanel;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button applyButton;
    [SerializeField] private Button resetButton;

    [Header("VR Settings")]
    [SerializeField] private Slider movementSpeedSlider;
    [SerializeField] private Slider rotationSpeedSlider;
    [SerializeField] private Slider uiScaleSlider;
    [SerializeField] private Toggle snapTurnToggle;
    [SerializeField] private Toggle smoothLocomotionToggle;
    [SerializeField] private TMP_Dropdown dominantHandDropdown;

    [Header("Game Settings")]
    [SerializeField] private Slider matchDurationSlider;
    [SerializeField] private Toggle autoStartToggle;
    [SerializeField] private Toggle showScoreboardToggle;
    [SerializeField] private Slider scoreboardDistanceSlider;
    [SerializeField] private Slider scoreboardHeightSlider;

    [Header("Settings")]
    [SerializeField] private float defaultDistance = 2f;
    [SerializeField] private float defaultHeight = 1.5f;
    [SerializeField] private float scaleFactor = 0.002f;
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float lerpSpeed = 5f;

    private XRRig xrRig;
    private bool isGrabbed = false;
    private Vector3 grabOffset;
    private Quaternion grabRotation;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private VRScoreboard vrScoreboard;
    private ScoreManager scoreManager;
    private bool isMenuOpen = false;

    private void Awake()
    {
        ValidateReferences();
        SetupUI();
    }

    private void Start()
    {
        xrRig = FindObjectOfType<XRRig>();
        vrScoreboard = FindObjectOfType<VRScoreboard>();
        scoreManager = FindObjectOfType<ScoreManager>();

        if (xrRig == null)
        {
            Debug.LogError("XRRig not found in scene!");
            return;
        }

        LoadSettings();
        CloseMenu(); // Start with menu closed
    }

    private void ValidateReferences()
    {
        if (menuTransform == null)
        {
            Debug.LogError($"{nameof(menuTransform)} reference is missing in {nameof(VRSettingsMenu)}");
            enabled = false;
            return;
        }

        if (menuPanel == null)
        {
            Debug.LogError($"{nameof(menuPanel)} reference is missing in {nameof(VRSettingsMenu)}");
            enabled = false;
            return;
        }

        if (closeButton == null || applyButton == null || resetButton == null)
        {
            Debug.LogError("Button references are missing in {nameof(VRSettingsMenu)}");
            enabled = false;
            return;
        }

        if (movementSpeedSlider == null || rotationSpeedSlider == null || uiScaleSlider == null)
        {
            Debug.LogError("Slider references are missing in {nameof(VRSettingsMenu)}");
            enabled = false;
            return;
        }

        if (snapTurnToggle == null || smoothLocomotionToggle == null)
        {
            Debug.LogError("Toggle references are missing in {nameof(VRSettingsMenu)}");
            enabled = false;
            return;
        }

        if (dominantHandDropdown == null)
        {
            Debug.LogError($"{nameof(dominantHandDropdown)} reference is missing in {nameof(VRSettingsMenu)}");
            enabled = false;
            return;
        }
    }

    private void SetupUI()
    {
        // Set up button listeners
        closeButton.onClick.AddListener(CloseMenu);
        applyButton.onClick.AddListener(ApplySettings);
        resetButton.onClick.AddListener(ResetSettings);

        // Set up sliders
        movementSpeedSlider.minValue = 0.5f;
        movementSpeedSlider.maxValue = 5f;
        movementSpeedSlider.value = moveSpeed;
        movementSpeedSlider.onValueChanged.AddListener(OnMovementSpeedChanged);

        rotationSpeedSlider.minValue = 50f;
        rotationSpeedSlider.maxValue = 200f;
        rotationSpeedSlider.value = rotationSpeed;
        rotationSpeedSlider.onValueChanged.AddListener(OnRotationSpeedChanged);

        uiScaleSlider.minValue = 0.001f;
        uiScaleSlider.maxValue = 0.005f;
        uiScaleSlider.value = scaleFactor;
        uiScaleSlider.onValueChanged.AddListener(OnUIScaleChanged);

        // Set up toggles
        snapTurnToggle.isOn = true;
        snapTurnToggle.onValueChanged.AddListener(OnSnapTurnChanged);

        smoothLocomotionToggle.isOn = true;
        smoothLocomotionToggle.onValueChanged.AddListener(OnSmoothLocomotionChanged);

        // Set up dropdown
        dominantHandDropdown.ClearOptions();
        dominantHandDropdown.AddOptions(new System.Collections.Generic.List<string> { "Left", "Right" });
        dominantHandDropdown.value = 1; // Default to right hand
        dominantHandDropdown.onValueChanged.AddListener(OnDominantHandChanged);

        // Set up background
        if (backgroundPanel != null)
        {
            backgroundPanel.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        }

        // Set up layout
        RectTransform rectTransform = menuPanel.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(800, 600);
        }
    }

    private void LoadSettings()
    {
        // Load saved settings or use defaults
        moveSpeed = PlayerPrefs.GetFloat("MovementSpeed", moveSpeed);
        rotationSpeed = PlayerPrefs.GetFloat("RotationSpeed", rotationSpeed);
        scaleFactor = PlayerPrefs.GetFloat("UIScale", scaleFactor);
        defaultDistance = PlayerPrefs.GetFloat("ScoreboardDistance", defaultDistance);
        defaultHeight = PlayerPrefs.GetFloat("ScoreboardHeight", defaultHeight);

        movementSpeedSlider.value = moveSpeed;
        rotationSpeedSlider.value = rotationSpeed;
        uiScaleSlider.value = scaleFactor;
        scoreboardDistanceSlider.value = defaultDistance;
        scoreboardHeightSlider.value = defaultHeight;
        snapTurnToggle.isOn = PlayerPrefs.GetInt("SnapTurn", 1) == 1;
        smoothLocomotionToggle.isOn = PlayerPrefs.GetInt("SmoothLocomotion", 1) == 1;
        dominantHandDropdown.value = PlayerPrefs.GetInt("DominantHand", 1);
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetFloat("MovementSpeed", moveSpeed);
        PlayerPrefs.SetFloat("RotationSpeed", rotationSpeed);
        PlayerPrefs.SetFloat("UIScale", scaleFactor);
        PlayerPrefs.SetFloat("ScoreboardDistance", defaultDistance);
        PlayerPrefs.SetFloat("ScoreboardHeight", defaultHeight);
        PlayerPrefs.SetInt("SnapTurn", snapTurnToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("SmoothLocomotion", smoothLocomotionToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("DominantHand", dominantHandDropdown.value);
        PlayerPrefs.Save();
    }

    private void ApplySettings()
    {
        moveSpeed = movementSpeedSlider.value;
        rotationSpeed = rotationSpeedSlider.value;
        scaleFactor = uiScaleSlider.value;
        defaultDistance = scoreboardDistanceSlider.value;
        defaultHeight = scoreboardHeightSlider.value;

        if (vrScoreboard != null)
        {
            vrScoreboard.UpdateSettings(moveSpeed, rotationSpeed, scaleFactor, defaultDistance, defaultHeight);
        }

        SaveSettings();
        CloseMenu();
    }

    private void ResetSettings()
    {
        movementSpeedSlider.value = 2f;
        rotationSpeedSlider.value = 100f;
        uiScaleSlider.value = 0.002f;
        scoreboardDistanceSlider.value = 2f;
        scoreboardHeightSlider.value = 1.5f;
        snapTurnToggle.isOn = true;
        smoothLocomotionToggle.isOn = true;
        dominantHandDropdown.value = 1;

        ApplySettings();
    }

    private void PositionMenu()
    {
        if (menuTransform == null || xrRig == null) return;

        targetPosition = xrRig.transform.position + 
            xrRig.transform.forward * defaultDistance + 
            Vector3.up * defaultHeight;

        targetRotation = Quaternion.LookRotation(
            targetPosition - xrRig.transform.position);

        menuTransform.position = targetPosition;
        menuTransform.rotation = targetRotation;
        menuTransform.localScale = Vector3.one * scaleFactor;
    }

    public void OnGrabStart(XRBaseInteractor interactor)
    {
        isGrabbed = true;
        grabOffset = menuTransform.position - interactor.transform.position;
        grabRotation = menuTransform.rotation;
    }

    public void OnGrabEnd(XRBaseInteractor interactor)
    {
        isGrabbed = false;
    }

    private void Update()
    {
        if (xrRig == null) return;

        if (!isGrabbed)
        {
            menuTransform.position = Vector3.Lerp(
                menuTransform.position,
                targetPosition,
                lerpSpeed * Time.deltaTime);

            menuTransform.rotation = Quaternion.Slerp(
                menuTransform.rotation,
                targetRotation,
                lerpSpeed * Time.deltaTime);
        }
    }

    public void ToggleMenu()
    {
        if (isMenuOpen)
        {
            CloseMenu();
        }
        else
        {
            ShowMenu();
        }
    }

    public void ShowMenu()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
            PositionMenu();
            isMenuOpen = true;
        }
    }

    public void CloseMenu()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
            isMenuOpen = false;
        }
    }

    public void RecenterMenu()
    {
        PositionMenu();
    }

    // Event handlers for UI changes
    private void OnMovementSpeedChanged(float value)
    {
        moveSpeed = value;
    }

    private void OnRotationSpeedChanged(float value)
    {
        rotationSpeed = value;
    }

    private void OnUIScaleChanged(float value)
    {
        scaleFactor = value;
        menuTransform.localScale = Vector3.one * scaleFactor;
    }

    private void OnSnapTurnChanged(bool value)
    {
        // Implement snap turn logic here
    }

    private void OnSmoothLocomotionChanged(bool value)
    {
        // Implement smooth locomotion logic here
    }

    private void OnDominantHandChanged(int value)
    {
        // Implement dominant hand logic here
    }
} 