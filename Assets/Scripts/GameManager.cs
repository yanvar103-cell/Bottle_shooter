using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private float _timeLimit = 60f;
    [SerializeField] private float _startMessageDuration = 3f;
    [SerializeField, Range(0f, 1f)] private float _requiredPercent = 0.75f;
    [SerializeField] private Launcher _launcher;
    [SerializeField] private LineRenderer _trajectoryLine;

    [Header("UI (optional)")]
    [SerializeField] private TMP_Text _timerText;
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private TMP_Text _resultText;
    [SerializeField] private GameObject _restartButton;

    private int _totalBottles;
    private int _shotBottles;
    private int _requiredBottles;
    private float _timeLeft;
    private bool _isRunning;

    private void Awake()
    {
        Instance = this;
    }

    public void StartGame(int totalBottles)
    {
        _totalBottles = totalBottles;
        _requiredBottles = Mathf.CeilToInt(_totalBottles * _requiredPercent);
        _shotBottles = 0;
        _timeLeft = _timeLimit;
        _isRunning = false;          // timer waits until the intro is over

        if (_restartButton != null) _restartButton.SetActive(false);

        UpdateUI();                  // shows full time (60) and 0 / total
        StartCoroutine(IntroRoutine());
    }

    public void BottleShot()
    {
        if (!_isRunning) return;

        _shotBottles++;
        UpdateUI();

        if (_shotBottles >= _requiredBottles)
            EndGame(true);
    }

    //Hook this to the Restart button's OnClick
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void Update()
    {
        if (!_isRunning) return;

        _timeLeft -= Time.deltaTime;
        if (_timeLeft <= 0f)
        {
            _timeLeft = 0f;
            UpdateUI();
            EndGame(false);
            return;
        }

        UpdateUI();
    }

    private void EndGame(bool won)
    {
        _isRunning = false;

        //stop shooting and aiming
        _launcher.enabled = false;
        _trajectoryLine.enabled = false;

        string message;
        if (won)
            message = $"You win!\nBottles: {_shotBottles}/{_totalBottles} (needed: {_requiredBottles})";
        else
            message = $"Time's up! You lose.\nBottles: {_shotBottles}/{_totalBottles} (needed: {_requiredBottles})";
        
        if (_resultText != null) _resultText.text = message;
        if (_restartButton != null) _restartButton.SetActive(true);
    }

    private void UpdateUI()
    {
        if (_timerText != null) _timerText.text = $"Time: {Mathf.CeilToInt(_timeLeft)}";
        if (_scoreText != null) _scoreText.text = $"Bottles: {_shotBottles} / {_totalBottles}";
    }

    private IEnumerator IntroRoutine()
    {
        //block aiming and shooting during the intro
        _launcher.enabled = false;
        _trajectoryLine.enabled = false;

        if (_resultText != null)
            _resultText.text = $"Shoot at least {Mathf.RoundToInt(_requiredPercent * 100f)}% of total bottles\nin time remaining to win!\nincrease/decrease force by Q/E\nWSAD to rotate";

        yield return new WaitForSeconds(_startMessageDuration);

        if (_resultText != null) _resultText.text = "";

        //start the game
        _launcher.enabled = true;
        _trajectoryLine.enabled = true;
        _isRunning = true;
    }
}
