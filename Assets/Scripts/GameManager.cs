using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.XR.Interaction.Toolkit;

public class GameManager : NetworkBehaviour
{
    public enum GameState { Inactive, Active, Paused, Ended }
    public NetworkVariable<GameState> NetworkGameState = new NetworkVariable<GameState>(GameState.Inactive);
    public GameState CurrentGameState { get => NetworkGameState.Value; set => NetworkGameState.Value = value; }
    public static GameManager Instance;
    public GameObject BallPrefab;
    [SerializeField] private float _gameTimer;
    [SerializeField] private float hapticIntensity = 0.5f;
    [SerializeField] private float hapticDuration = 0.1f;
    private NetworkVariable<float> _networkTimer = new NetworkVariable<float>();
    private float _timer { get => _networkTimer.Value; set => _networkTimer.Value = value; }
    private NetworkVariable<int> _networkServeCounter = new NetworkVariable<int>();
    private int _serveCounter { get => _networkServeCounter.Value; set => _networkServeCounter.Value = value; }
    private NetworkVariable<int> _networkHostScore = new NetworkVariable<int>();
    private int _hostScore { get => _networkHostScore.Value; set => _networkHostScore.Value = value; }
    private NetworkVariable<int> _networkClientScore = new NetworkVariable<int>();
    private int _clientScore { get => _networkClientScore.Value; set => _networkClientScore.Value = value; }
    [SerializeField] private Vector3 _hostServeLocation, _clientServeLocation;
    private NetworkVariable<bool> _networkDeuce = new NetworkVariable<bool>();
    private bool _deuce { get => _networkDeuce.Value; set => _networkDeuce.Value = value; }
    private GameObject _currentBall;
    private bool _isGameInitialized = false;
    private bool _isPaused = false;
    private XRBaseController _leftController;
    private XRBaseController _rightController;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        FindControllers();
    }

    private void FindControllers()
    {
        var controllers = FindObjectsOfType<XRBaseController>();
        foreach (var controller in controllers)
        {
            if (controller.controllerNode == UnityEngine.XR.XRNode.LeftHand)
            {
                _leftController = controller;
            }
            else if (controller.controllerNode == UnityEngine.XR.XRNode.RightHand)
            {
                _rightController = controller;
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            InitializeGame();
        }

        NetworkManager.OnClientDisconnectCallback += HandleClientDisconnect;
        NetworkGameState.OnValueChanged += HandleGameStateChange;
        _networkHostScore.OnValueChanged += HandleHostScoreChange;
        _networkClientScore.OnValueChanged += HandleClientScoreChange;
    }

    public override void OnNetworkDespawn()
    {
        NetworkManager.OnClientDisconnectCallback -= HandleClientDisconnect;
        NetworkGameState.OnValueChanged -= HandleGameStateChange;
        _networkHostScore.OnValueChanged -= HandleHostScoreChange;
        _networkClientScore.OnValueChanged -= HandleClientScoreChange;
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        if (IsServer)
        {
            Debug.Log($"Client {clientId} disconnected");
            if (CurrentGameState == GameState.Active)
            {
                PauseGame();
            }
        }
    }

    private void HandleGameStateChange(GameState previous, GameState current)
    {
        UpdateUIForGameState(current);
        SendHapticFeedbackOnStateChange(current);
    }

    private void HandleHostScoreChange(int previous, int current)
    {
        UpdateScoreUI();
        SendHapticFeedbackOnScore();
    }

    private void HandleClientScoreChange(int previous, int current)
    {
        UpdateScoreUI();
        SendHapticFeedbackOnScore();
    }

    private void SendHapticFeedbackOnStateChange(GameState state)
    {
        switch (state)
        {
            case GameState.Active:
                SendHapticFeedback(hapticIntensity, hapticDuration);
                break;
            case GameState.Ended:
                SendHapticFeedback(hapticIntensity * 2, hapticDuration * 2);
                break;
        }
    }

    private void SendHapticFeedbackOnScore()
    {
        SendHapticFeedback(hapticIntensity * 0.5f, hapticDuration * 0.5f);
    }

    private void SendHapticFeedback(float intensity, float duration)
    {
        if (_leftController != null)
        {
            _leftController.SendHapticImpulse(intensity, duration);
        }
        if (_rightController != null)
        {
            _rightController.SendHapticImpulse(intensity, duration);
        }
    }

    private void UpdateUIForGameState(GameState state)
    {
        if (UIManager.Instance == null) return;

        // Hide all panels first
        UIManager.Instance.GameActivePanel.SetActive(false);
        UIManager.Instance.GamePausedPanel.SetActive(false);
        UIManager.Instance.TimesUpPanel.SetActive(false);
        UIManager.Instance.HostWinsPanel.SetActive(false);
        UIManager.Instance.ClientWinsPanel.SetActive(false);

        // Show appropriate panel
        switch (state)
        {
            case GameState.Active:
                UIManager.Instance.GameActivePanel.SetActive(true);
                break;
            case GameState.Paused:
                UIManager.Instance.GamePausedPanel.SetActive(true);
                break;
            case GameState.Ended:
                // Specific end condition panels are handled in CheckForWin
                break;
        }
    }

    private void UpdateScoreUI()
    {
        if (UIManager.Instance == null) return;
        UIManager.Instance.UpdateHostScore(_hostScore);
        UIManager.Instance.UpdateClientScore(_clientScore);
    }

    private void InitializeGame()
    {
        if (_isGameInitialized) return;
        
        _timer = _gameTimer;
        _serveCounter = 0;
        _hostScore = 0;
        _clientScore = 0;
        _deuce = false;
        CurrentGameState = GameState.Inactive;
        _isGameInitialized = true;
    }

    void Update()
    {
        if (CurrentGameState != GameState.Active || _isPaused) return;
        
        if (_timer <= 0)
        {
            EndGame();
        }
        else 
        {
            if (IsServer)
            {
                _timer = Mathf.Max(0, _timer - Time.deltaTime);
            }
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateTimer(_timer);
            }
        }
    }

    private void EndGame()
    {
        if (!IsServer) return;
        
        if (_currentBall != null)
        {
            Destroy(_currentBall);
        }
        if (UIManager.Instance != null)
        {
            UIManager.Instance.TimesUpPanel.SetActive(true);
        }
        CurrentGameState = GameState.Ended;
    }

    public void PauseGame()
    {
        if (!IsServer) return;
        _isPaused = true;
        CurrentGameState = GameState.Paused;
    }

    public void ResumeGame()
    {
        if (!IsServer) return;
        _isPaused = false;
        CurrentGameState = GameState.Active;
    }

    public void InstantiateBall()
    {
        if (!IsServer) return;
        
        if (_hostScore >= 10 && _clientScore >= 10) _deuce = true;
        if (!_deuce)
        {
            if (_serveCounter >= 4) _serveCounter = 0;
            if (_serveCounter < 2) _currentBall = Instantiate(BallPrefab, _hostServeLocation, Quaternion.identity);
            else _currentBall = Instantiate(BallPrefab, _clientServeLocation, Quaternion.identity);
        }
        else
        {
            if (_serveCounter >= 2) _serveCounter = 0;
            if (_serveCounter < 1) _currentBall = Instantiate(BallPrefab, _hostServeLocation, Quaternion.identity);
            else _currentBall = Instantiate(BallPrefab, _clientServeLocation, Quaternion.identity);
        }

        if (_currentBall != null)
        {
            NetworkObject networkObject = _currentBall.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                networkObject.Spawn();
            }
            else
            {
                Debug.LogError("Ball prefab is missing NetworkObject component");
                Destroy(_currentBall);
            }
        }
    }

    public void UpdateHostScore() 
    {
        if (!IsServer) return;
        _serveCounter += 1;
        _hostScore += 1;
        if (_currentBall != null)
        {
            Destroy(_currentBall);
        }
        CheckForWin();
        InstantiateBall();
    }

    public void UpdateClientScore() 
    {
        if (!IsServer) return;
        _serveCounter += 1;
        _clientScore += 1;
        if (_currentBall != null)
        {
            Destroy(_currentBall);
        }
        CheckForWin();
        InstantiateBall();
    }

    public void CheckForWin()
    {
        if (!IsServer) return;
        if (_hostScore >= 11 && _hostScore - _clientScore >= 2) 
        {
            EndGameWithWinner(true);
        }
        if (_clientScore >= 11 && _clientScore - _hostScore >= 2) 
        {
            EndGameWithWinner(false);
        }
    }

    private void EndGameWithWinner(bool isHostWinner)
    {
        if (!IsServer) return;
        
        if (_currentBall != null)
        {
            Destroy(_currentBall);
        }
        if (UIManager.Instance != null)
        {
            if (isHostWinner)
            {
                UIManager.Instance.HostWinsPanel.SetActive(true);
            }
            else
            {
                UIManager.Instance.ClientWinsPanel.SetActive(true);
            }
        }
        CurrentGameState = GameState.Ended;
    }

    public void Restart()
    {
        if (!IsServer) return;
        _hostScore = 0;
        _clientScore = 0;
        _serveCounter = 0;
        _deuce = false;
        _timer = _gameTimer;
        _isPaused = false;
        UpdateUIForGameState(GameState.Active);
        if (_currentBall != null)
        {
            Destroy(_currentBall);
        }
        InstantiateBall();
        CurrentGameState = GameState.Active;
    }
}
