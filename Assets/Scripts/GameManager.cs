using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    public enum GameState { Inactive, Active, Paused, Ended }
    public GameState CurrentGameState = GameState.Inactive;
    public static GameManager Instance;
    public GameObject BallPrefab;
    [SerializeField] private float _gameTimer;
    private float _timer;
    private int _serveCounter, _hostScore, _clientScore = 0;
    [SerializeField] private Vector3 _hostServeLocation, _clientServeLocation;
    private bool _deuce = false;

    void Awake()
    {
        Instance = this;
        Instantiate(BallPrefab, _hostServeLocation, Quaternion.identity);
        _timer = _gameTimer;
    }

    void Update()
    {
        if (CurrentGameState != GameState.Active) return;
        if (_timer <= 0)
        {
            Destroy(BallPrefab);
            UIManager.Instance.TimesUpPanel.SetActive(true);
            CurrentGameState = GameState.Ended;
        }
        else 
        {
        _timer -= Time.deltaTime;
        UIManager.Instance.UpdateTimer(_timer);
        }
    }

    public void InstantiateBall()
    {
        if (_hostScore >= 10 && _clientScore >= 10) _deuce = true;
        if (!_deuce)
        {
            if (_serveCounter >= 4) _serveCounter = 0;
            if (_serveCounter < 2) Instantiate(BallPrefab, _hostServeLocation, Quaternion.identity);
            else Instantiate(BallPrefab, _clientServeLocation, Quaternion.identity);
        }
        else
        {
            if (_serveCounter >= 2) _serveCounter = 0;
            if (_serveCounter < 1) Instantiate(BallPrefab, _hostServeLocation, Quaternion.identity);
            else Instantiate(BallPrefab, _clientServeLocation, Quaternion.identity);
        }
    }

    public void UpdateHostScore() 
    {
        if (!IsOwner) return;
        _serveCounter += 1;
        _hostScore += 1;
        UIManager.Instance.UpdateHostScore(_hostScore);
        Destroy(BallPrefab);
        CheckForWin();
        InstantiateBall();
    }
    public void UpdateClientScore() 
    {
        if (!IsOwner) return;
        _serveCounter += 1;
        _clientScore += 1;
        UIManager.Instance.UpdateClientScore(_clientScore);
        Destroy(BallPrefab);
        CheckForWin();
        InstantiateBall();
    }

    public void CheckForWin()
    {
        if (!IsOwner) return;
        if (_hostScore >= 11 && _hostScore - _clientScore >= 2) 
        {
            UIManager.Instance.HostWinsPanel.SetActive(true);
            Destroy(BallPrefab);
            CurrentGameState = GameState.Ended;
        }
        if (_clientScore >= 11 && _clientScore - _hostScore >= 2) 
        {
            UIManager.Instance.ClientWinsPanel.SetActive(true);
            Destroy(BallPrefab);
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
        InstantiateBall();
        CurrentGameState = GameState.Active;
    }
}
