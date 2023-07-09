using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PatrolActionKind
{
    FollowPath,
    Wait,
    PlayAnimation,
    FaceDirection,
    ExitDungeon
}

[System.Serializable]
public class PatrolAction
{
    public PatrolActionKind kind;
    public PatrolPath path;
    public PatrolAnimation animation;
    public float waitTime;
    public Vector2 directionToFace;
}

[System.Serializable]
public class PatrolPath
{
    public Vector2[] points;
    //NOTE: Specifies additional number of times path taken (e.g., 2 means first cycle + 2, so path 
    //is taken 3 times in total)
    public int repeats;
}

[System.Serializable]
public class PatrolAnimation
{
    // Use a different animatior if assigned
    public Animator alternativeAnimator = null;
    // State name in animation controller
    public string stateName;
    // If animation doesn't run as long as minDuration, wait.
    public float minDuration;
}