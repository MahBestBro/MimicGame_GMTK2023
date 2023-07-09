using System;
using System.Collections.Generic;
using UnityEngine;

public class GlobalState : MonoBehaviour
{
    public static Action onGameLoss;

    void GaemOverr()
    {
        Debug.Log("gaem overr!!");
    }

    void Awake()
    {
        onGameLoss += GaemOverr;
    }
}
