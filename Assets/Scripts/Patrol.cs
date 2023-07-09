using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ItemOfInterest
{
    public Sprite sprite;
    public float interestScore;
}

public class Patrol : MonoBehaviour
{
    const int visionConeResolution = 128;
    readonly Color inspectColor = new Color(48, 41, 36) / 255f; 

    [SerializeField] UnityEvent onExitDungeon;

    [SerializeField] List<PatrolAction> actions;
    [SerializeField] ItemOfInterest[] itemsOfInterest;
    
    [SerializeField] float visionConeRadius = 8f;
    [SerializeField][Range(0, 360)] float visionConeAngleDegrees = 30f;
    [SerializeField][Range(0, 1)] float visionConeAlpha = 0.1f;
    
    [SerializeField] Vector2 startingDirection;

    [SerializeField] float defaultSpeed;
    [SerializeField] float chaseSpeed;

    [SerializeField] float detectWaitTime;
    [SerializeField] float inspectionTime;

    [SerializeField] Animator animator;
    int animationActionEndPermissionHash;

    Sprite[] itemSprites;
    float[] itemScores;
    int itemToAddScoreOfIndex;

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
    int pathIndex = 1;
    int repeatCount = 0;
    float elapsedTime = 0f;
    bool startedAction = true;

    //All of these members are the "action record", they represent where the patrol was in its
    //action sequence before being distracted
    Vector2 prevPos;
    Vector2 prevDirection;

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

        animator.SetBool(animationActionEndPermissionHash, false);
    }

    public void NextAction()
    {
        actionIndex++;
        ResetActionContext();
    }

    void StoreActionContext()
    {
        prevPos = (Vector2)transform.position;
        prevDirection = visionConeDirection; 
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
            Vector2 direction = (destination - (Vector2)transform.position).normalized;
            SetVisionConeDirection(direction);

            transform.position += (Vector3)direction * defaultSpeed * Time.deltaTime;

            //if ((Vector2)transform.position == destination)
            if (V2ApproxEquals((Vector2)transform.position, destination, 0.02f))
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
            NextAction();
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
                    NextAction();
                }
            } break;

            case PatrolActionKind.FaceDirection:
            {
                startedAction = false;

                SetVisionConeDirection(action.directionToFace);

                NextAction();
            } break;

            case PatrolActionKind.PlayAnimation:
                {
                    bool usingOwnAnimator = (action.animation.alternativeAnimator == null);
                    Animator anim = usingOwnAnimator ? animator : action.animation.alternativeAnimator;

                    if (startedAction)
                    {
                        anim.Play(action.animation.stateName);
                        startedAction = false;
                    }
                    if (usingOwnAnimator)
                    {
                        elapsedTime += Time.deltaTime;

                        anim.SetBool(animationActionEndPermissionHash, (elapsedTime >= action.animation.minDuration));

                        // Handle NextAction() in animator
                    } else
                    {
                        NextAction(); // After triggering alt animator, go right to next action
                    }
                }
                break;
            case PatrolActionKind.ExitDungeon: 
            {
                onExitDungeon.Invoke();
                startedAction = false; 
            } break;
        }
    }

    void ReturnToAction()
    {
        Vector2 direction = (prevPos - (Vector2)transform.position).normalized;
        SetVisionConeDirection(direction);
        transform.position += (Vector3)direction * defaultSpeed * Time.deltaTime;

        if (V2ApproxEquals((Vector2)transform.position, prevPos, 0.02f))
        {
            SetVisionConeDirection(prevDirection);
            state = PatrolState.PerformingScriptedActions;
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

    IEnumerator ShowSuspicion(string prompt, Color color)
    {
        SetVisionConeDirection((Vector2)(player.transform.position - transform.position));
        suspicionIndicator.GetComponent<TextMesh>().text = prompt;
        suspicionIndicator.GetComponent<TextMesh>().color = color;
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

        yield return StartCoroutine(ShowSuspicion("!", Color.red));
        yield return FollowPlayer(chaseSpeed, () => endFollow);  
        
        currentlyInCoroutine = false;
        endFollow = false; 
    }

    IEnumerator Inspect()
    {
        currentlyInCoroutine = true;

        yield return StartCoroutine(ShowSuspicion("?", inspectColor));
        yield return FollowPlayer(defaultSpeed, () => endFollow);
         
        player.Crumch();
        GlobalState.addToScore?.Invoke(itemScores[itemToAddScoreOfIndex]);
        GlobalState.showEndScreen?.Invoke();
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

    #if (UNITY_EDITOR)

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 lastLocation = transform.position;
        for (int a = 0; a < actions.Count; a++)
        {
            PatrolAction action = actions[a];
            switch (action.kind)
            {
                case PatrolActionKind.FollowPath:
                    UnityEngine.Random.InitState(action.path.points.GetHashCode());
                    Handles.color = UnityEngine.Random.ColorHSV(0,1,0,1,0,1,1,1);
                    for (int i = 0; i < action.path.points.Length; i++)
                    {
                        lastLocation = action.path.points[i];
                        Gizmos.DrawSphere((Vector3)lastLocation, 0.1f);
                        if (i > 0)
                        {
                            Handles.DrawLine(action.path.points[i - 1], lastLocation, 0.2f);
                        }

                    }
                    UnityEngine.Random.InitState(new System.Random().Next(int.MinValue, int.MaxValue));
                    break;
                case PatrolActionKind.Wait:
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere((Vector3)lastLocation, 0.5f);
                    break;
                case PatrolActionKind.PlayAnimation:
                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere((Vector3)lastLocation, 0.5f);
                    break;
            }
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

    #endif

    // Start is called before the first frame update
    void Start()
    {
        if(!animator) animator = GetComponent<Animator>();
        animationActionEndPermissionHash = Animator.StringToHash("canEndAction");

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

        visionConeRenderer = visionCone.GetComponent<MeshRenderer>();
        visionConeRenderer.material.color = new Color(0f, 0f, 0f, visionConeAlpha);

        SetVisionConeDirection(startingDirection);

        suspicionIndicator = transform.Find("SuspicionIndicator").gameObject;
        suspicionIndicator.SetActive(false);

        Sprite[] sprites = new Sprite[itemsOfInterest.Length];
        float[] scores = new float[itemsOfInterest.Length];
        for (int i = 0; i < itemsOfInterest.Length; i++)
        {
            sprites[i] = itemsOfInterest[i].sprite;
            scores[i] = itemsOfInterest[i].interestScore;
        }
        itemSprites = sprites;
        itemScores = scores;
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

        visionConeRenderer.material.color = ChangeAlpha(hitPlayer ? Color.red : Color.black, visionConeAlpha);
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

        int currestDisguiseIndex = Array.FindIndex(
            itemSprites,
            x => player.GetMimicComponent().isDisguisedAs(x)
        );
        if (!Inspecting() && currestDisguiseIndex >= 0)
        {
            itemToAddScoreOfIndex = currestDisguiseIndex;
            state = PatrolState.AboutToInspect;
        }
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
