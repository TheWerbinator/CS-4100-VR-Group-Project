using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    public GameObject BallPrefab;
    [SerializeField] private float _gameTimer;
    private int _serveCounter, _hostScore, _clientScore = 0;
    [SerializeField] private Vector3 _hostServeLocation, _clientServeLocation;
    private bool _deuce = false;

    void Awake()
    {
        Instance = this;
        Instantiate(BallPrefab, _hostServeLocation, Quaternion.identity);
    }

    void Update()
    {
        if (_gameTimer <= 0) {//end game
                              }
        _gameTimer -= Time.deltaTime;
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
        Destroy(BallPrefab);
        CheckForWin();
        InstantiateBall();
    }
    public void UpdateClientScore() 
    {
        if (!IsOwner) return;
        _serveCounter += 1;
        _clientScore += 1;
        Destroy(BallPrefab);
        CheckForWin();
        InstantiateBall();
    }

    public void CheckForWin()
    {
        if (!IsOwner) return;
        if (_hostScore >= 11 && _hostScore - _clientScore >= 2) { }//host wins
        if (_clientScore >= 11 && _clientScore - _hostScore >= 2) { }//client wins
    }
}
