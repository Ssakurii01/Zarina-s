using UnityEngine;
using System;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    private int _roundsSurvived = 0;

    public int RoundsSurvived => _roundsSurvived;

    public static event Action<int> OnRoundsSurvivedChanged;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void OnEnable()
    {
        RoundManager.OnRoundStart += HandleRoundStart;
    }

    void OnDisable()
    {
        RoundManager.OnRoundStart -= HandleRoundStart;
    }

    private void HandleRoundStart(int round)
    {
        _roundsSurvived = round - 1;
        OnRoundsSurvivedChanged?.Invoke(_roundsSurvived);
    }
}
