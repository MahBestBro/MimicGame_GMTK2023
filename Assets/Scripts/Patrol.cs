using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patrol : MonoBehaviour
{
    [SerializeField] List<PatrolAction> actions;
    
    [SerializeField] float visionConeRadius = 8f;
    [SerializeField][Range(0, 360)] float visionConeAngleDegrees = 30f;
    
    [SerializeField] float defaultSpeed;
    [SerializeField] float chaseSpeed;
    [SerializeField] float returnSpeed;
    
    [SerializeField] float waitTime;

    //Vector2 faceDir

    Transform visionCone;
    MeshRenderer visionConeRenderer;

    int actionIndex = 0;

    //All of these members are the "action state"
    //Animation animation;
    int pathIndex = 1;
    int repeatCount = 0;
    float elapsedTime = 0f;
    bool startedAction = true;

    // Start is called before the first frame update
    void Start()
    {
        Mesh visionConeMesh = new Mesh();
        Vector3[] vertices = new Vector3[128 + 1]; //+1 for point of origin
        int[] triangles = new int[3 * (vertices.Length - 2)];
        vertices[0] = Vector3.zero;
        for (int i = 1; i < vertices.Length; i++)
        {
            float t = 2f * i / (vertices.Length - 1) - 1f;
            float angleDegrees = Mathf.Lerp(-visionConeAngleDegrees, visionConeAngleDegrees, t);
            Vector3 rayDir = Quaternion.AngleAxis(angleDegrees, Vector3.forward) * Vector3.left;
            vertices[i] = rayDir * visionConeRadius;

            if (i < vertices.Length - 2)
            {
                triangles[3 * i    ] = 0;
                triangles[3 * i + 1] = i;
                triangles[3 * i + 2] = i + 1;
            }
        }
        visionConeMesh.vertices = vertices;
        visionConeMesh.triangles = triangles;

        visionCone = transform.GetChild(0);
        visionCone.GetComponent<MeshFilter>().mesh = visionConeMesh;
        //animation = GetComponent<Animation>();

        visionConeRenderer = visionCone.GetComponent<MeshRenderer>();
        visionConeRenderer.material.color = Color.red;
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

    void HandlePathWalk(PatrolPath path)
    {
        if (pathIndex < path.points.Length)
        {
            if (startedAction)
            {
                transform.position = path.points[0];
                startedAction = false;

                visionCone.rotation = Quaternion.identity;
                Vector2 firstDirection = path.points[1] - path.points[0];
                float rotationAngle = Vector2.SignedAngle(firstDirection, Vector2.left);
                visionCone.rotation *= Quaternion.AngleAxis(rotationAngle, Vector3.forward);
            }

            Vector2 destination = path.points[pathIndex];
            Vector2 direction = destination - path.points[pathIndex - 1];

            transform.position += (Vector3)direction * defaultSpeed * Time.fixedDeltaTime;

            //if ((Vector2)transform.position == destination)
            if (V2ApproxEquals((Vector2)transform.position, destination, 0.05f))
            {
                transform.position = path.points[pathIndex];

                //Vector2 prevDirection = destination - path.points[pathIndex - 1];
                pathIndex++;
                if (pathIndex == path.points.Length) return;

                Vector2 newDirection = path.points[pathIndex] - path.points[pathIndex - 1];
                float rotationAngle = Vector2.SignedAngle(direction, newDirection);
                visionCone.rotation *= Quaternion.AngleAxis(rotationAngle, Vector3.forward);
            }
        }
        else if (repeatCount < path.repeats)
        {
            pathIndex = 1;
            repeatCount++;  
        }
        else
        {
            actionIndex++;
            ResetActionState();
        }
    }

    void HandleAction()
    {
        if (actionIndex >= actions.Count) return;

        PatrolAction action = actions[actionIndex];

        switch(action.kind)
        {
            case PatrolActionKind.FollowPath: HandlePathWalk(action.path); break;

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

            case PatrolActionKind.PlayAnimation: break; 
            //{
            //    if (startedAction)
            //    {
            //        animation.AddClip(action.animation.clip, "clip");
            //        animation.Play();
            //        startedAction = false; 
            //    }
            //    
            //    if (!animation.isPlaying) animation.Play();
//
            //    elapsedTime += Time.fixedDeltaTime;
            //    if (elapsedTime > action.animation.extendedDuration)
            //    {
            //        animation.Stop();
            //        animation.RemoveClip(action.animation.clip);
            //        actionIndex++;
            //        ResetActionState();
            //    }
//
            //} break;
            case PatrolActionKind.ExitDungeon: startedAction = false; break;
        }
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


    void FixedUpdate()
    {
        //TODO: Move this out of fixed update cause not all of it is needed here
        HandleAction();
    }

}
