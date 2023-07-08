using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patrol : MonoBehaviour
{
    [SerializeField] List<PatrolAction> actions;
    
    [SerializeField] float defaultSpeed;
    [SerializeField] float chaseSpeed;
    [SerializeField] float returnSpeed;
    
    [SerializeField] float waitTime;

    int actionIndex = 0;

    //All of these members are the "action state"
    int pathIndex = 1;
    int repeatCount = 0;
    float elapsedTime = 0f;
    bool startedAction = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    bool V2ApproxEquals(Vector2 a, Vector2 b, float epsilon)
    {
        return b.x <= a.x + epsilon && b.x >= a.x - epsilon &&
               b.y <= a.y + epsilon && b.y >= a.y - epsilon;
    }

    void ResetActionState()
    {
        pathIndex = 1;
        repeatCount = 0;
        elapsedTime = 0f;
        startedAction = true;
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        foreach (PatrolAction action in actions)
        {
            if (action.kind != PatrolActionKind.FollowPath) continue;
            
            foreach(Vector2 point in action.path.points) Gizmos.DrawSphere((Vector3)point, 0.1f);
        }
    }

    //TODO: Move a lot of this stuff out of fixed update cause not all of it is needed here
    // Update is called once per frame
    void FixedUpdate()
    {
        if (actionIndex >= actions.Count) return;

        PatrolAction action = actions[actionIndex];

        switch(action.kind)
        {
            case PatrolActionKind.FollowPath:
            {
                if (pathIndex < action.path.points.Length)
                {
                    if (startedAction)
                    {
                        transform.position = action.path.points[0];
                        startedAction = false;
                    }

                    Vector2 destination = action.path.points[pathIndex];
                    Vector2 direction = destination - action.path.points[pathIndex - 1];

                    transform.position += (Vector3)direction * defaultSpeed * Time.fixedDeltaTime;

                    //if ((Vector2)transform.position == destination)
                    if (V2ApproxEquals((Vector2)transform.position, destination, 0.05f))
                    {
                        transform.position = action.path.points[pathIndex];
                        pathIndex++;
                    };
                }
                else if (repeatCount < action.path.repeats)
                {
                    pathIndex = 1;
                    repeatCount++;  
                }
                else
                {
                    actionIndex++;
                    ResetActionState();
                }
            } break;

            case PatrolActionKind.Wait:
            {
                startedAction = false;

                elapsedTime += Time.fixedDeltaTime;
                if (elapsedTime > action.waitTime)
                {
                    actionIndex++;
                    ResetActionState();
                }
            } break;

            case PatrolActionKind.PlayAnimation: startedAction = false; break;
            case PatrolActionKind.ExitDungeon: startedAction = false; break;
        }
    }
}
