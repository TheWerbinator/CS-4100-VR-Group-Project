using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
  public static int guestScore = 0;
  public static int homeScore = 0;
  [SerializeField] private TextMeshProUGUI _timeText;
  [SerializeField] private TextMeshProUGUI _guestText;
  [SerializeField] private TextMeshProUGUI _homeText;
  private static int _minutes = 0;
  private static int _seconds = 0;

  void Start()
  {
    InvokeRepeating("UpdateTime", 0, 1);
  }
  void Update()
  {
    if (_seconds == 60)
    {
      _seconds = 0;
      _minutes += 1;
    }
    ;
    if (_timeText != null) _timeText.text = "" + _minutes.ToString("D2") + ":" + _seconds.ToString("D2"); ;
    if (_guestText != null) _guestText.text = "" + guestScore;
    if (_homeText != null) _homeText.text = "" + homeScore;
  }
  private void UpdateTime()
  {
    _seconds += 1;
  }
  public static void AddScoreHome()
  {
    homeScore += 1;
  }
  public static void AddScoreGuest()
  {
    guestScore += 1;
  }
}