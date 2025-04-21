using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    public enum GameState { Inactive, Active, Paused, Ended }
    public GameState CurrentGameState = GameState.Inactive;
    public static GameManager Instance;
    public GameObject Ball;
    [SerializeField] private float _gameTimer;
    private Rigidbody _ballRigidbody;
    private float _timer;
    private int _serveCounter, _hostScore, _clientScore = 0;
    [SerializeField] private Vector3 _hostServeLocation, _clientServeLocation;
    private bool _deuce = false;

    void Awake()
    {
        Instance = this;
        Ball.transform.position = _hostServeLocation;
        _ballRigidbody = Ball.GetComponent<Rigidbody>();
        _timer = _gameTimer;
    }

    void Update()
    {
        if (!IsOwner) return;
        if (CurrentGameState != GameState.Active) return;
        if (_timer <= 0)
        {
            ResetBall();
            UIManager.Instance.TimesUpPanel.SetActive(true);
            CurrentGameState = GameState.Ended;
        }
        else 
        {
        _timer -= Time.deltaTime;
        UIManager.Instance.UpdateTimer(_timer);
        }
    }

    public void ResetBall()
    {
        if (!IsOwner) return;
        _ballRigidbody.linearVelocity = Vector3.zero;
        _ballRigidbody.angularVelocity = Vector3.zero;
        Ball.GetComponent<BallPhysicsComponent>().ballState = BallPhysicsComponent.BallState.Held;
        if (_hostScore >= 10 && _clientScore >= 10) _deuce = true;
        if (!_deuce)
        {
            if (_serveCounter >= 4) _serveCounter = 0;
            if (_serveCounter < 2) Ball.transform.position = _hostServeLocation;
            else Ball.transform.position = _clientServeLocation;
        }
        else
        {
            if (_serveCounter >= 2) _serveCounter = 0;
            if (_serveCounter < 1) Ball.transform.position = _hostServeLocation;
            else Ball.transform.position = _clientServeLocation;
        }
    }

    public void StartGame() 
    {
        if (!IsOwner) return;
        CurrentGameState = GameState.Active;
    }

    public void UpdateHostScore() 
    {
        if (!IsOwner) return;
        _serveCounter += 1;
        _hostScore += 1;
        UIManager.Instance.UpdateHostScore(_hostScore);
        ResetBall();
        CheckForWin();
    }
    public void UpdateClientScore() 
    {
        if (!IsOwner) return;
        _serveCounter += 1;
        _clientScore += 1;
        UIManager.Instance.UpdateClientScore(_clientScore);
        ResetBall();
        CheckForWin();
    }

    public void NoScore()
    {
        if (!IsOwner) return;
        ResetBall();
    }

    public void CheckForWin()
    {
        if (!IsOwner) return;
        if (_hostScore >= 11 && _hostScore - _clientScore >= 2) 
        {
            UIManager.Instance.HostWinsPanel.SetActive(true);
            ResetBall();
            CurrentGameState = GameState.Ended;
        }
        if (_clientScore >= 11 && _clientScore - _hostScore >= 2) 
        {
            UIManager.Instance.ClientWinsPanel.SetActive(true);
            ResetBall();
            CurrentGameState = GameState.Ended;
        }
    }

    public void Restart()
    {
        if (!IsOwner) return;
        _hostScore = 0;
        _clientScore = 0;
        _serveCounter = 0;
        _deuce = false;
        _timer = _gameTimer;
        UIManager.Instance.UpdateTimer(_timer);
        UIManager.Instance.UpdateHostScore(_hostScore);
        UIManager.Instance.UpdateClientScore(_clientScore);
        UIManager.Instance.TimesUpPanel.SetActive(false);
        UIManager.Instance.HostWinsPanel.SetActive(false);
        UIManager.Instance.ClientWinsPanel.SetActive(false);
        ResetBall();
        CurrentGameState = GameState.Active;
    }
}
