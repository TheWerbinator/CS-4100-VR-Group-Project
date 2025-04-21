using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class UIManager : NetworkBehaviour
{
    public static UIManager Instance;
    public GameObject TimesUpPanel, HostWinsPanel, ClientWinsPanel;
    [SerializeField] private TextMeshProUGUI _timerText, _hostScoreText, _clientScoreText;

    private void Awake()
    {
        Instance = this;
        this.TimesUpPanel.SetActive(false);
        this.HostWinsPanel.SetActive(false);
        this.ClientWinsPanel.SetActive(false);
    }
    public void Start()
    {
        _timerText.text = "00:00";
        _hostScoreText.text = "0";
        _clientScoreText.text = "0";
    }

    public void HostGame()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void JoinAsClient()
    {
        NetworkManager.Singleton.StartClient();
    }

public void UpdateTimer(float time)
    {
        if (!IsOwner) return;
        int minutes = (int)(time / 60);
        int seconds = (int)(time % 60);
        _timerText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    public void UpdateHostScore(int score)
    {
        if (!IsOwner) return;
        _hostScoreText.text = score.ToString();
    }

    public void UpdateClientScore(int score)
    {
        if (!IsOwner) return;
        _clientScoreText.text = score.ToString();
    }
}
