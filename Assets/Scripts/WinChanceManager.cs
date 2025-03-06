using System.Collections.Generic;
using UnityEngine;

public class WinChanceManager : MonoBehaviour
{
    [Header("Base Chance Settings")]
    [SerializeField][Range(0, 100)] private float baseWinChance = 52f;
   [Range(0, 100)] public int minWinChance = 0;
   [Range(0, 100)] public int maxWinChance = 99;

    [Header("Dynamic Adjustments")]
   [Range(1, 100)] private int chanceDecreasePerThrowMin = 10;
    [Range(1, 100)] private int chanceDecreasePerThrowMax = 50;
   [Range(1, 20)] private int chanceDecreasePerRotation = 5;
   [Range(0, 50)] private int minRotationWinChance = 15;

    [Header("Multiplier Settings")]
  [Min(1f)] private float minMultiplier = 1;
   [Min(1f)] public float maxMultiplier = 3;
   private float minAirJumpMultiplier = 0.1f;
 private float maxAirJumpMultiplier = 0.5f;

    [Header("Balance Control")]
[Range(0.1f, 0.15f)] private float initialBalanceLoss = 0.12f;
    [Min(0.1f)] private float balanceSmoothness = 2f;
    [Range(0f, 1f)] private float deviationChance = 0.15f;
    [Range(0.5f, 0.9f)] private float maxDeviationPercent = 0.9f;
    [Min(1f)] private float recoveryBoostMultiplier = 2f;
    [SerializeField] private AnimationCurve winChanceCurve;

    [Header("Win Boost Mechanism")]
    [SerializeField][Min(0)] private int minGamesForBoost = 25;
    [Min(0)] private int maxGamesForBoost = 60;
    [Range(0f, 1f)] private float winBoostChance = 0.60f;
    [Min(1f)] private float winBoostMultiplier = 1.30f;

    [Header("Hard Reset Chance")]
    [SerializeField][Range(0f, 1f)] private float hardResetChance = 0.07f;
    [Min(0f)] private float controlGoal = 30;

    [Header("Slot Simulation Settings")]
    [SerializeField] private List<int> virtualReelStrip = new List<int> { 70, 65, 60, 55, 50, 45, 40 };
    [SerializeField] private int currentReelIndex;
    [SerializeField] private float reelAdvanceSpeed = 0.3f;
    [SerializeField] private bool forceWinNextThrow;

    [Header("Virtual Line Settings")]
    [SerializeField] private int minSpinsForWin = 3;
    [SerializeField] private float uprightThresholdForLine = 15f;
    [SerializeField] private float scatterZoneMultiplier = 2f;

    private bool _predeterminedWin;
    private float _calculatedMultiplier;
    private int _virtualLinesWon;

    private float currentGeneratedMultiplier;
    private float deviationMultiplier = 1f;
    private bool isDeviationActive;
    private int consecutiveLosses;
    private int totalGamesPlayed;
    private int nextBoostGame;


    public static WinChanceManager Instance { get; private set; }

    public float CurrentWinChance { get; private set; }
    public float CurrentMultiplier => currentGeneratedMultiplier;
    public static float PlayerBalance
    {
        get => PlayerPrefs.GetFloat("coins", 100.00f);
        set => PlayerPrefs.SetFloat("coins", value);
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadData()
    {
        CurrentWinChance = PlayerPrefs.GetFloat("CurrentWinChance", baseWinChance);
        consecutiveLosses = PlayerPrefs.GetInt("ConsecutiveLosses", 0);
        totalGamesPlayed = PlayerPrefs.GetInt("TotalGamesPlayed", 0);
        currentReelIndex = PlayerPrefs.GetInt("CurrentReelIndex", 0);
    }
    public void SaveData()
    {
        PlayerPrefs.SetFloat("CurrentWinChance", CurrentWinChance);
        PlayerPrefs.SetInt("ConsecutiveLosses", consecutiveLosses);
        PlayerPrefs.SetInt("TotalGamesPlayed", totalGamesPlayed);
        PlayerPrefs.SetInt("CurrentReelIndex", currentReelIndex);
    }
    private void Start()
    {
        CurrentWinChance = baseWinChance;
        GenerateNewMultiplier();
        CalculateTargetBalance();
        nextBoostGame = Random.Range(minGamesForBoost, maxGamesForBoost);
    }
    public void ResetReelState()
    {
        forceWinNextThrow = false;
        _virtualLinesWon = 0;
        deviationMultiplier = 1f;
    }
    public float GetPredeterminedMultiplier()
    {
        if (!_predeterminedWin) return 1f;

        float baseMultiplier = _calculatedMultiplier;

        float bonus = _virtualLinesWon * 0.5f;

        return Mathf.Clamp(baseMultiplier + bonus, minMultiplier, maxMultiplier);
    }
    public void GeneratePredefinedResult()
    {
        currentReelIndex = (currentReelIndex + 1) % virtualReelStrip.Count;
        CurrentWinChance = virtualReelStrip[currentReelIndex];

        if (consecutiveLosses >= controlGoal)
        {
            forceWinNextThrow = true;
        }

        _calculatedMultiplier = Mathf.Lerp(minMultiplier, maxMultiplier,
            winChanceCurve.Evaluate(PlayerBalance / 100f));

        _predeterminedWin = forceWinNextThrow ||
            (Random.value <= CurrentWinChance / 100f * deviationMultiplier);
    }
    public void GenerateNewMultiplier()
    {
        float t = Random.value;
        currentGeneratedMultiplier = Mathf.Clamp(
            Mathf.Lerp(minMultiplier, maxMultiplier, t),
            minMultiplier,
            maxMultiplier
        );
    }
    private void OnApplicationQuit()
    {
        SaveData();
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused) SaveData();
    }
    
    public float GetAirJumpMultiplier() =>
        Random.Range(minAirJumpMultiplier, maxAirJumpMultiplier);

    public void AdjustChanceAfterThrow(bool won, float betAmount)
    {
        float actualBalance = PlayerBalance;
        float adjustment = won ?
            -Random.Range(chanceDecreasePerThrowMin, chanceDecreasePerThrowMax) :
            chanceDecreasePerThrowMax;

        CurrentWinChance = Mathf.Clamp(
            CurrentWinChance + adjustment,
            minWinChance,
            maxWinChance
        );

        PlayerBalance = Mathf.Clamp(
            PlayerBalance + (won ? betAmount * currentGeneratedMultiplier : -betAmount),
            0.01f,
            float.MaxValue
        );

        totalGamesPlayed++;
        CheckForWinBoost();
    }

    public void DecreaseWinChancePerRotation()
    {
        CurrentWinChance = Mathf.Clamp(
            CurrentWinChance - chanceDecreasePerRotation,
            minRotationWinChance,
            maxWinChance
        );
    }

    public bool CalculateWinResult()
    {
        float effectiveChance = Mathf.Clamp(
            CurrentWinChance * deviationMultiplier,
            minWinChance,
            maxWinChance
        );

        bool result = Random.Range(0f, 100f) <= effectiveChance;

        if (!result)
        {
            consecutiveLosses++;
            CheckForHardReset();
        }
        else
        {
            consecutiveLosses = 0;
        }

        return result;
    }

    public void CheckForWinBoost()
    {
        if (totalGamesPlayed >= nextBoostGame && Random.value < winBoostChance)
        {
            CurrentWinChance *= winBoostMultiplier;
            nextBoostGame = totalGamesPlayed + Random.Range(minGamesForBoost, maxGamesForBoost);
        }
    }

    public void CheckForHardReset()
    {
        if (consecutiveLosses >= controlGoal && Random.value < hardResetChance)
        {
            CurrentWinChance = baseWinChance;
            consecutiveLosses = 0;
        }
        SaveData();
    }

    public void CalculateTargetBalance()
    {
        float targetBalance = PlayerBalance * (1 - initialBalanceLoss);
        UpdateBalanceControl(targetBalance);
    }

    public void UpdateBalanceControl(float targetBalance)
    {
        float balanceDifference = PlayerBalance - targetBalance;
        float normalizedDifference = Mathf.Clamp(balanceDifference / targetBalance, -1f, 1f);
        float dynamicChance = winChanceCurve.Evaluate(normalizedDifference) * 100f;

        if (isDeviationActive)
        {
            CurrentWinChance = Mathf.Lerp(
                CurrentWinChance,
                dynamicChance * deviationMultiplier,
                Time.deltaTime * balanceSmoothness
            );

            if (PlayerBalance <= targetBalance * (1 - maxDeviationPercent))
            {
                deviationMultiplier = recoveryBoostMultiplier;
            }

            if (Mathf.Abs(PlayerBalance - targetBalance) < targetBalance * 0.05f)
            {
                isDeviationActive = false;
                deviationMultiplier = 1f;
            }
        }
        else
        {
            CurrentWinChance = Mathf.Lerp(
                CurrentWinChance,
                dynamicChance,
                Time.deltaTime * balanceSmoothness
            );
        }
        SaveData();
        CurrentWinChance = Mathf.Clamp(CurrentWinChance, minWinChance, maxWinChance);
    }
    public void AdjustChance(bool boost)
    {
        if (boost)
        {
            CurrentWinChance = Mathf.Clamp(
                CurrentWinChance + chanceDecreasePerRotation * 2,
                minWinChance,
                maxWinChance
            );
        }
        else
        {
            CurrentWinChance = Mathf.Clamp(
                CurrentWinChance - chanceDecreasePerRotation,
                minWinChance,
                maxWinChance
            );
        }
        SaveData();
    }
    public void CheckForDeviation()
    {
        if (isDeviationActive) return;

        if (Random.value < deviationChance)
        {
            deviationMultiplier = Random.Range(0.1f, maxDeviationPercent);
            isDeviationActive = true;
        }
        SaveData();
    }

    public string GetGameParameters()
    {
        float rtp = (CurrentWinChance / 100f) * currentGeneratedMultiplier * 100f;
        return $"RTP: {rtp:F2}% | Chance: {CurrentWinChance:F2}% | Multiplier: {currentGeneratedMultiplier:F2}x";
    }
}