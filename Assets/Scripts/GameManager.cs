using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private float _timeLimit = 60f;
    [SerializeField, Range(0f, 1f)] private float _requiredPercent = 0.75f;
    [SerializeField] private Launcher _launcher;
    [SerializeField] private LineRenderer _trajectoryLine;

    [Header("UI (optional)")]
    [SerializeField] private TMP_Text _timerText;
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private TMP_Text _resultText;

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
        _isRunning = true;

        if (_resultText != null) _resultText.text = "";
        UpdateUI();
    }

    public void BottleShot()
    {
        if (!_isRunning) return;

        _shotBottles++;
        UpdateUI();

        if (_shotBottles >= _requiredBottles)
            EndGame(true);
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
            message = $"You win! Bottles: {_shotBottles}/{_totalBottles} (needed: {_requiredBottles})";
        else
            message = $"Time's up! You lose. Bottles: {_shotBottles}/{_totalBottles} (needed: {_requiredBottles})";
        
        if (_resultText != null) _resultText.text = message;
    }

    private void UpdateUI()
    {
        if (_timerText != null) _timerText.text = $"Time: {Mathf.CeilToInt(_timeLeft)}";
        if (_scoreText != null) _scoreText.text = $"Bottles: {_shotBottles} / {_requiredBottles}";
    }
}
