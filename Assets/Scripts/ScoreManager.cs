using TMPro;
using UnityEngine;
using Unity.Netcode;

public class ScoreManager : NetworkBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Score Variables")]
    public NetworkVariable<int> guestScore = new NetworkVariable<int>();
    public NetworkVariable<int> homeScore = new NetworkVariable<int>();
    private NetworkVariable<int> minutes = new NetworkVariable<int>();
    private NetworkVariable<int> seconds = new NetworkVariable<int>();

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI guestText;
    [SerializeField] private TextMeshProUGUI homeText;
    [SerializeField] private VRScoreboard vrScoreboard;

    [Header("Game Settings")]
    [SerializeField] private int matchDurationMinutes = 5;
    private bool isMatchActive = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError($"Multiple instances of {nameof(ScoreManager)} detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        ValidateReferences();
    }

    private void Start()
    {
        if (IsServer)
        {
            InitializeScores();
        }

        // Subscribe to network variable changes
        guestScore.OnValueChanged += OnGuestScoreChanged;
        homeScore.OnValueChanged += OnHomeScoreChanged;
        minutes.OnValueChanged += OnTimeChanged;
        seconds.OnValueChanged += OnTimeChanged;
    }

    private void OnDestroy()
    {
        // Unsubscribe from network variable changes
        guestScore.OnValueChanged -= OnGuestScoreChanged;
        homeScore.OnValueChanged -= OnHomeScoreChanged;
        minutes.OnValueChanged -= OnTimeChanged;
        seconds.OnValueChanged -= OnTimeChanged;
    }

    private void ValidateReferences()
    {
        if (timeText == null)
        {
            Debug.LogWarning($"{nameof(timeText)} reference is missing in {nameof(ScoreManager)}");
        }

        if (guestText == null)
        {
            Debug.LogWarning($"{nameof(guestText)} reference is missing in {nameof(ScoreManager)}");
        }

        if (homeText == null)
        {
            Debug.LogWarning($"{nameof(homeText)} reference is missing in {nameof(ScoreManager)}");
        }

        if (vrScoreboard == null)
        {
            Debug.LogWarning($"{nameof(vrScoreboard)} reference is missing in {nameof(ScoreManager)}");
        }
    }

    private void InitializeScores()
    {
        guestScore.Value = 0;
        homeScore.Value = 0;
        minutes.Value = matchDurationMinutes;
        seconds.Value = 0;
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartMatchServerRpc()
    {
        if (!IsServer) return;

        isMatchActive = true;
        InitializeScores();
        InvokeRepeating(nameof(UpdateTime), 0, 1);
    }

    [ServerRpc(RequireOwnership = false)]
    public void StopMatchServerRpc()
    {
        if (!IsServer) return;

        isMatchActive = false;
        CancelInvoke(nameof(UpdateTime));
    }

    private void UpdateTime()
    {
        if (!IsServer || !isMatchActive) return;

        if (seconds.Value > 0)
        {
            seconds.Value--;
        }
        else if (minutes.Value > 0)
        {
            minutes.Value--;
            seconds.Value = 59;
        }
        else
        {
            // Match ended
            isMatchActive = false;
            CancelInvoke(nameof(UpdateTime));
            OnMatchEnded();
        }
    }

    private void OnMatchEnded()
    {
        // Handle match end logic
        Debug.Log("Match ended!");
        // You can add additional match end logic here
    }

    private void OnTimeChanged(int previous, int current)
    {
        UpdateTimeUI();
        if (vrScoreboard != null)
        {
            vrScoreboard.UpdateTimeServerRpc(minutes.Value, seconds.Value);
        }
    }

    private void OnGuestScoreChanged(int previous, int current)
    {
        UpdateGuestScoreUI();
        if (vrScoreboard != null)
        {
            vrScoreboard.UpdateGuestScoreServerRpc(current);
        }
    }

    private void OnHomeScoreChanged(int previous, int current)
    {
        UpdateHomeScoreUI();
        if (vrScoreboard != null)
        {
            vrScoreboard.UpdateHostScoreServerRpc(current);
        }
    }

    private void UpdateTimeUI()
    {
        if (timeText != null)
        {
            timeText.text = $"{minutes.Value:D2}:{seconds.Value:D2}";
        }
    }

    private void UpdateGuestScoreUI()
    {
        if (guestText != null)
        {
            guestText.text = guestScore.Value.ToString();
        }
    }

    private void UpdateHomeScoreUI()
    {
        if (homeText != null)
        {
            homeText.text = homeScore.Value.ToString();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddScoreHomeServerRpc()
    {
        if (!IsServer || !isMatchActive) return;
        homeScore.Value += 1;
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddScoreGuestServerRpc()
    {
        if (!IsServer || !isMatchActive) return;
        guestScore.Value += 1;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ResetScoresServerRpc()
    {
        if (!IsServer) return;
        InitializeScores();
    }
}