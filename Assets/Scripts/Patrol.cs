using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patrol : MonoBehaviour
{
    const int visionConeResolution = 128;

    [SerializeField] List<PatrolAction> actions;
    [SerializeField] Sprite[] itemsOfInterest;
    
    [SerializeField] float visionConeRadius = 8f;
    [SerializeField][Range(0, 360)] float visionConeAngleDegrees = 30f;
    [SerializeField][Range(0, 1)] float visionConeAlpha = 0.5f;
    
    [SerializeField] Vector2 startingDirection;

    [SerializeField] float defaultSpeed;
    [SerializeField] float chaseSpeed;

    [SerializeField] float detectWaitTime;
    [SerializeField] float inspectionTime;

    Transform visionCone;
    MeshRenderer visionConeRenderer;
    Vector2 visionConeDirection = Vector2.right; //NOTE: This is mostly only meant to be written over in Update 

    GameObject suspicionIndicator;

    Vector2 prevPlayerPos;
    Player player;

    int actionIndex = 0;

    IEnumerator coroutine;
    bool currentlyInCoroutine = false;
    enum PatrolState
    {
        PerformingScriptedActions,
        ReturnToScriptedAction,
        AboutToInspect,
        Inspecting,
        AboutToChase,
        Chasing
    };
    PatrolState state = PatrolState.PerformingScriptedActions;

    //All of these members are the "action context"
    //Animation animation;
    int pathIndex = 1;
    int repeatCount = 0;
    float elapsedTime = 0f;
    bool startedAction = true;

    //All of these members are the "action record", they represent where the patrol was in its
    //action sequence before being distracted
    Vector2 prevPos;

    bool Inspecting()
    {
        return state == PatrolState.AboutToInspect || state == PatrolState.Inspecting;
    }

    bool Chasing()
    {
        return state == PatrolState.AboutToChase || state == PatrolState.Chasing;
    }

    bool Following()
    {
        return Inspecting() || Chasing();
    }

    bool V2ApproxEquals(Vector2 a, Vector2 b, float epsilon)
    {
        return Vector2.Distance(a, b) <= epsilon;
    }

    void SetVisionConeDirection(Vector2 direction)
    {
        float rotationAngle = Vector2.SignedAngle(visionConeDirection, direction.normalized);
        visionCone.rotation *= Quaternion.AngleAxis(rotationAngle, Vector3.forward);
        visionConeDirection = direction;
    }

    void ResetActionContext()
    {
        pathIndex = 1;
        repeatCount = 0;
        elapsedTime = 0f;
        startedAction = true;
    }

    void StoreActionContext()
    {
        prevPos = (Vector2)transform.position;
    }

    void HandlePathWalk(PatrolPath path)
    {
        if (pathIndex < path.points.Length)
        {
            if (startedAction)
            {
                transform.position = path.points[0];
                startedAction = false;
            }

            Vector2 destination = path.points[pathIndex];
            Vector2 direction = (destination - path.points[pathIndex - 1]).normalized;
            SetVisionConeDirection(direction);

            transform.position += (Vector3)direction * defaultSpeed * Time.deltaTime;

            //if ((Vector2)transform.position == destination)
            if (V2ApproxEquals((Vector2)transform.position, destination, 0.05f))
            {
                transform.position = path.points[pathIndex];
                pathIndex++;
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
            ResetActionContext();
        }
    }

    void PerformAction()
    {
        if (actionIndex >= actions.Count) return;

        PatrolAction action = actions[actionIndex];

        switch(action.kind)
        {
            case PatrolActionKind.FollowPath: HandlePathWalk(action.path); break;

            case PatrolActionKind.Wait:
            {
                startedAction = false;

                elapsedTime += Time.deltaTime;
                if (elapsedTime > action.waitTime)
                {
                    actionIndex++;
                    ResetActionContext();
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
            //        ResetActionContext();
            //    }
//
            //} break;
            case PatrolActionKind.ExitDungeon: startedAction = false; break;
        }
    }

    void ReturnToAction()
    {
        if (actionIndex >= actions.Count) return;

        PatrolAction action = actions[actionIndex];

        switch(action.kind)
        {
            case PatrolActionKind.FollowPath:
            {
                Vector2 direction = (prevPos - (Vector2)transform.position).normalized;
                SetVisionConeDirection(direction);
                transform.position += (Vector3)direction * defaultSpeed * Time.deltaTime;

                if (V2ApproxEquals((Vector2)transform.position, prevPos, 0.05f))
                {
                    state = PatrolState.PerformingScriptedActions;
                }
            } break;

            case PatrolActionKind.Wait:
            {
                startedAction = false;

                elapsedTime += Time.deltaTime;
                if (elapsedTime > action.waitTime)
                {
                    actionIndex++;
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
            //        ResetActionContext();
            //    }
//
            //} break;
            case PatrolActionKind.ExitDungeon: startedAction = false; break;
        }
    }

    Color ChangeAlpha(Color c, float a)
    {
        return new Color(c.r, c.g, c.b, a);
    } 

    void ChasePlayer()
    {
        if (elapsedTime < detectWaitTime) 
        {
            suspicionIndicator.SetActive(true);
            elapsedTime += Time.deltaTime;
        } 
        else
        {
            suspicionIndicator.SetActive(false);
            Vector3 direction = (player.transform.position - transform.position).normalized;
            direction.z = 0f;
            transform.position += chaseSpeed * direction * Time.deltaTime;

            elapsedTime = 0f;
        }
    }

    IEnumerator ShowSuspicion(string suspicionPrompt)
    {
        SetVisionConeDirection((Vector2)(player.transform.position - transform.position));
        suspicionIndicator.GetComponent<TextMesh>().text = suspicionPrompt;
        suspicionIndicator.SetActive(true);
        yield return new WaitForSeconds(detectWaitTime);
        suspicionIndicator.SetActive(false);
    }

    IEnumerator FollowPlayer(float speed, Func<bool> shouldStop)
    {
        while (!shouldStop())
        {
            Vector3 direction = (player.transform.position - transform.position).normalized;
            direction.z = 0f;
            transform.position += speed * direction * Time.deltaTime;
            SetVisionConeDirection((Vector2)direction);
            yield return null;
        }
    }

    IEnumerator Chase()
    {
        currentlyInCoroutine = true;

        yield return StartCoroutine(ShowSuspicion("!"));
        yield return FollowPlayer(chaseSpeed, () => endFollow);  
        
        currentlyInCoroutine = false;
        endFollow = false; 
    }

    IEnumerator Inspect()
    {
        currentlyInCoroutine = true;

        yield return StartCoroutine(ShowSuspicion("?"));
        yield return FollowPlayer(defaultSpeed, () => endFollow);
         
        player.Crumch();
        Destroy(gameObject);
    }

    bool endFollow = false;
    void OnTriggerEnter2D(Collider2D other)
    {
        //TODO: make use of player member instead
        Player potentialPlayer = other.transform.GetComponent<Player>();
        if (potentialPlayer != null && Following()) 
        {
            if (Chasing()) GlobalState.onGameLoss?.Invoke();
            endFollow = true;
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

        //for (int i = 0; i < visionConeResolution; i++)
        //{
        //    float t = 2f * i / visionConeResolution - 1f;
        //    float angleDegrees = Mathf.Lerp(-visionConeAngleDegrees, visionConeAngleDegrees, t);
        //    Vector2 rayDir = Quaternion.AngleAxis(angleDegrees, Vector3.forward) * visionConeDirection;
        //    Vector2 rayEnd = (Vector2)transform.position + rayDir * visionConeRadius;
//
        //    Gizmos.DrawLine((Vector2)transform.position, rayEnd);
        //}
    }

    // Start is called before the first frame update
    void Start()
    {
        Mesh visionConeMesh = new Mesh();
        Vector3[] vertices = new Vector3[visionConeResolution + 1]; //+1 for point of origin
        int[] triangles = new int[3 * (vertices.Length - 2)];
        vertices[0] = Vector3.zero;
        for (int i = 1; i < vertices.Length; i++)
        {
            float t = 2f * i / (vertices.Length - 1) - 1f;
            float angleDegrees = Mathf.Lerp(-visionConeAngleDegrees, visionConeAngleDegrees, t);
            Vector3 rayDir = Quaternion.AngleAxis(angleDegrees, Vector3.forward) * Vector3.right;
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
        visionConeRenderer.material.color = new Color(0f, 0f, 0f, visionConeAlpha);

        SetVisionConeDirection(startingDirection);

        suspicionIndicator = transform.Find("SuspicionIndicator").gameObject;
        suspicionIndicator.SetActive(false);
    }

    void Update()
    {
        if (state == PatrolState.PerformingScriptedActions) PerformAction(); 
        else if (state == PatrolState.ReturnToScriptedAction) ReturnToAction();
    }

    bool firstHit = true;
    void FixedUpdate()
    {
        if (Chasing()) return;
        
        if (Inspecting()) goto checkForChase;
        
        bool hitPlayer = false;
        for (int i = 0; i < visionConeResolution; i++)
        {
            float t = 2f * i / visionConeResolution - 1f;
            float angleDegrees = Mathf.Lerp(-visionConeAngleDegrees, visionConeAngleDegrees, t);
            Vector2 rayDir = Quaternion.AngleAxis(angleDegrees, Vector3.forward) * visionConeDirection;
            Vector2 rayEnd = (Vector2)transform.position + rayDir * visionConeRadius;

            LayerMask rayMask = ~0;
            rayMask &= ~LayerMask.GetMask("Mimicable", "Interactable", "NonPhysicsColliders");
            RaycastHit2D hit = Physics2D.Linecast((Vector2)transform.position, rayEnd, rayMask);
            if (hit.collider != null)
            {
                Player potentialPlayer = hit.collider.transform.GetComponent<Player>();
                if (potentialPlayer != null)
                {
                    hitPlayer = true;
                    player = potentialPlayer;
                    if (firstHit) 
                    {
                        prevPlayerPos = (Vector2)player.transform.position;
                        firstHit = false;
                    }
                    break;
                }
            }
        }

        visionConeRenderer.material.color = ChangeAlpha(hitPlayer ? Color.red : Color.white, visionConeAlpha);
        if (!hitPlayer) 
        {
            firstHit = true;
            return;
        }
        
        checkForChase:
        if (prevPlayerPos != (Vector2)player.transform.position) 
        {
            state = PatrolState.AboutToChase;
            return;
        }

        if (!Inspecting() && Array.Exists(itemsOfInterest, x => player.GetMimicComponent().isDisguisedAs(x)))
            state = PatrolState.AboutToInspect;
    }

    void LateUpdate()
    {
        if (state == PatrolState.AboutToChase) 
        {
            if (currentlyInCoroutine) StopCoroutine(coroutine);
            state = PatrolState.Chasing;
            coroutine = Chase();
            StartCoroutine(coroutine);
        }

        if (state == PatrolState.AboutToInspect)
        {
            StoreActionContext();

            state = PatrolState.Inspecting;
            coroutine = Inspect();
            StartCoroutine(coroutine);
        }
    }
}
