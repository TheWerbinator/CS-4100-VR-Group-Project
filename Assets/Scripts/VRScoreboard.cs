using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using Unity.Netcode;
using UnityEngine.UI;

public class VRScoreboard : NetworkBehaviour
{
    [Header("Scoreboard References")]
    [SerializeField] private Transform scoreboardTransform;
    [SerializeField] private TextMeshProUGUI hostScoreText;
    [SerializeField] private TextMeshProUGUI guestScoreText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject scoreboardPanel;
    [SerializeField] private Image backgroundPanel;

    [Header("Settings")]
    [SerializeField] private float defaultDistance = 2f;
    [SerializeField] private float defaultHeight = 1.5f;
    [SerializeField] private float scaleFactor = 0.002f;
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float lerpSpeed = 5f;

    [Header("UI Settings")]
    [SerializeField] private Color hostColor = new Color(0.2f, 0.6f, 1f, 1f);
    [SerializeField] private Color guestColor = new Color(1f, 0.4f, 0.2f, 1f);
    [SerializeField] private Color timerColor = Color.white;
    [SerializeField] private float textSize = 48f;
    [SerializeField] private float padding = 20f;

    private XRRig xrRig;
    private bool isGrabbed = false;
    private Vector3 grabOffset;
    private Quaternion grabRotation;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private NetworkVariable<int> hostScore = new NetworkVariable<int>();
    private NetworkVariable<int> guestScore = new NetworkVariable<int>();
    private NetworkVariable<int> minutes = new NetworkVariable<int>();
    private NetworkVariable<int> seconds = new NetworkVariable<int>();

    private void Awake()
    {
        ValidateReferences();
        SetupUI();
    }

    private void Start()
    {
        if (IsServer)
        {
            InitializeScoreboard();
        }

        xrRig = FindObjectOfType<XRRig>();
        if (xrRig == null)
        {
            Debug.LogError("XRRig not found in scene!");
            return;
        }

        // Subscribe to network variable changes
        hostScore.OnValueChanged += OnHostScoreChanged;
        guestScore.OnValueChanged += OnGuestScoreChanged;
        minutes.OnValueChanged += OnTimeChanged;
        seconds.OnValueChanged += OnTimeChanged;

        // Position scoreboard in front of player
        PositionScoreboard();
    }

    private void OnDestroy()
    {
        // Unsubscribe from network variable changes
        hostScore.OnValueChanged -= OnHostScoreChanged;
        guestScore.OnValueChanged -= OnGuestScoreChanged;
        minutes.OnValueChanged -= OnTimeChanged;
        seconds.OnValueChanged -= OnTimeChanged;
    }

    private void ValidateReferences()
    {
        if (scoreboardTransform == null)
        {
            Debug.LogError($"{nameof(scoreboardTransform)} reference is missing in {nameof(VRScoreboard)}");
            enabled = false;
            return;
        }

        if (hostScoreText == null)
        {
            Debug.LogError($"{nameof(hostScoreText)} reference is missing in {nameof(VRScoreboard)}");
            enabled = false;
            return;
        }

        if (guestScoreText == null)
        {
            Debug.LogError($"{nameof(guestScoreText)} reference is missing in {nameof(VRScoreboard)}");
            enabled = false;
            return;
        }

        if (timerText == null)
        {
            Debug.LogError($"{nameof(timerText)} reference is missing in {nameof(VRScoreboard)}");
            enabled = false;
            return;
        }

        if (scoreboardPanel == null)
        {
            Debug.LogError($"{nameof(scoreboardPanel)} reference is missing in {nameof(VRScoreboard)}");
            enabled = false;
            return;
        }
    }

    private void SetupUI()
    {
        // Set up text styles
        hostScoreText.fontSize = textSize;
        guestScoreText.fontSize = textSize;
        timerText.fontSize = textSize;

        hostScoreText.color = hostColor;
        guestScoreText.color = guestColor;
        timerText.color = timerColor;

        // Set up background
        if (backgroundPanel != null)
        {
            backgroundPanel.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        }

        // Set up layout
        RectTransform rectTransform = scoreboardPanel.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(600, 300);
        }
    }

    private void InitializeScoreboard()
    {
        if (scoreboardPanel != null)
        {
            scoreboardPanel.SetActive(true);
        }

        hostScore.Value = 0;
        guestScore.Value = 0;
        minutes.Value = 0;
        seconds.Value = 0;
    }

    private void PositionScoreboard()
    {
        if (scoreboardTransform == null || xrRig == null) return;

        targetPosition = xrRig.transform.position + 
            xrRig.transform.forward * defaultDistance + 
            Vector3.up * defaultHeight;

        targetRotation = Quaternion.LookRotation(
            targetPosition - xrRig.transform.position);

        scoreboardTransform.position = targetPosition;
        scoreboardTransform.rotation = targetRotation;
        scoreboardTransform.localScale = Vector3.one * scaleFactor;
    }

    public void OnGrabStart(XRBaseInteractor interactor)
    {
        isGrabbed = true;
        grabOffset = scoreboardTransform.position - interactor.transform.position;
        grabRotation = scoreboardTransform.rotation;
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
            // Smoothly move to target position
            scoreboardTransform.position = Vector3.Lerp(
                scoreboardTransform.position,
                targetPosition,
                lerpSpeed * Time.deltaTime);

            scoreboardTransform.rotation = Quaternion.Slerp(
                scoreboardTransform.rotation,
                targetRotation,
                lerpSpeed * Time.deltaTime);
        }
    }

    private void OnHostScoreChanged(int previous, int current)
    {
        if (hostScoreText != null)
        {
            hostScoreText.text = current.ToString();
        }
    }

    private void OnGuestScoreChanged(int previous, int current)
    {
        if (guestScoreText != null)
        {
            guestScoreText.text = current.ToString();
        }
    }

    private void OnTimeChanged(int previous, int current)
    {
        if (timerText != null)
        {
            timerText.text = $"{minutes.Value:D2}:{seconds.Value:D2}";
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateHostScoreServerRpc(int score)
    {
        hostScore.Value = score;
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateGuestScoreServerRpc(int score)
    {
        guestScore.Value = score;
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateTimeServerRpc(int mins, int secs)
    {
        minutes.Value = mins;
        seconds.Value = secs;
    }

    public void RecenterScoreboard()
    {
        PositionScoreboard();
    }

    public void ShowScoreboard(bool show)
    {
        if (scoreboardPanel != null)
        {
            scoreboardPanel.SetActive(show);
        }
    }

    public void UpdateSettings(float newMoveSpeed, float newRotationSpeed, float newScaleFactor, float newDistance, float newHeight)
    {
        moveSpeed = newMoveSpeed;
        rotationSpeed = newRotationSpeed;
        scaleFactor = newScaleFactor;
        defaultDistance = newDistance;
        defaultHeight = newHeight;

        // Update the scoreboard's position and scale
        PositionScoreboard();
        scoreboardTransform.localScale = Vector3.one * scaleFactor;
    }
} 