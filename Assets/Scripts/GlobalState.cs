using System;
using System.Collections.Generic;
using UnityEngine;

public class GlobalState : MonoBehaviour
{
    public static Action onGameLoss;
    public static Action<float> addToScore;

    float score;

    void GaemOverr()
    {
        Debug.Log($"gaem overr!! Final Score: {score}");
    }

    void AddToScore(float addedScore)
    {
        score += addedScore;
    }

    void Awake()
    {
        addToScore += AddToScore;
        onGameLoss += GaemOverr;
    }
}
