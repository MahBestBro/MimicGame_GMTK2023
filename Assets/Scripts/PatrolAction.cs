using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PatrolActionKind
{
    FollowPath,
    Wait,
    PlayAnimation,
    ExitDungeon
}

[System.Serializable]
public class PatrolAction
{
    public PatrolActionKind kind;
    public PatrolPath path;
    public PatrolAnimation animation;
    public float waitTime;
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
    public AnimationClip clip;
    //Duration that can exceed animation time, in which it will repeat the animation
    public float extendedDuration;
}