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

    [SerializeField] Collider2D chaseCollider;
    
    Transform visionCone;
    MeshRenderer visionConeRenderer;
    Vector2 currentDirection;

    GameObject suspicionIndicator;

    Vector2 prevPlayerPos;
    Player player;

    int actionIndex = 0;

    IEnumerator coroutine;
    bool shouldChase = false;
    bool chasing = false;
    bool shouldInspect = false;
    bool inspecting = false;

    //All of these members are the "action state"
    //Animation animation;
    int pathIndex = 1;
    int repeatCount = 0;
    float elapsedTime = 0f;
    bool startedAction = true;

    //All of these members are the "action record", they represent where the patrol was in its
    //action sequence before being distracted
    Vector2 prevPos;



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
            Vector2 direction = (destination - path.points[pathIndex - 1]).normalized;
            currentDirection = direction;

            transform.position += (Vector3)direction * defaultSpeed * Time.deltaTime;

            //if ((Vector2)transform.position == destination)
            if (V2ApproxEquals((Vector2)transform.position, destination, 0.05f))
            {
                transform.position = path.points[pathIndex];

                //Vector2 prevDirection = destination - path.points[pathIndex - 1];
                pathIndex++;
                if (pathIndex == path.points.Length) return;

                Vector2 newDirection = (path.points[pathIndex] - path.points[pathIndex - 1]).normalized;
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

                elapsedTime += Time.deltaTime;
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
        suspicionIndicator.GetComponent<TextMesh>().text = suspicionPrompt;
        suspicionIndicator.SetActive(true);
        yield return new WaitForSeconds(detectWaitTime);

        suspicionIndicator.SetActive(false);
    }

    float prevDt = 0f;
    IEnumerator FollowPlayer(float speed, Func<bool> shouldStop)
    {
        while (!shouldStop())
        {
            Vector3 direction = (player.transform.position - transform.position).normalized;
            direction.z = 0f;
            transform.position += speed * direction * Time.deltaTime;
            prevDt = Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator Chase()
    {
        chasing = true;

        yield return StartCoroutine(ShowSuspicion("!"));
        yield return FollowPlayer(chaseSpeed, () => false);
        
        chasing = false;
    }

    IEnumerator Inspect()
    {
        inspecting = true;

        yield return StartCoroutine(ShowSuspicion("?"));
        yield return FollowPlayer(defaultSpeed, () => false);
        
        inspecting = false;
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        //TODO: make use of player member instead
        Player potentialPlayer = other.transform.GetComponent<Player>();
        if (potentialPlayer != null) GlobalState.onGameLoss?.Invoke();
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
        //    Vector2 rayDir = Quaternion.AngleAxis(angleDegrees, Vector3.forward) * currentDirection;
        //    Vector2 rayEnd = (Vector2)transform.position + rayDir * visionConeRadius;
//
        //    Gizmos.DrawLine((Vector2)transform.position, rayEnd);
        //}
    }

    // Start is called before the first frame update
    void Start()
    {
        currentDirection = startingDirection.normalized;

        Mesh visionConeMesh = new Mesh();
        Vector3[] vertices = new Vector3[visionConeResolution + 1]; //+1 for point of origin
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
        visionConeRenderer.material.color = new Color(0f, 0f, 0f, visionConeAlpha);

        suspicionIndicator = transform.Find("SuspicionIndicator").gameObject;
        suspicionIndicator.SetActive(false);
    }

    void Update()
    {
        if (!chasing && !inspecting) HandleAction(); 
    }

    void FixedUpdate()
    {
        //TODO: Move this out of fixed update cause not all of it is needed here
        
        if (chasing) return;
        
        if (inspecting) goto checkForChase;
        
        bool hitPlayer = false;
        for (int i = 0; i < visionConeResolution; i++)
        {
            float t = 2f * i / visionConeResolution - 1f;
            float angleDegrees = Mathf.Lerp(-visionConeAngleDegrees, visionConeAngleDegrees, t);
            Vector2 rayDir = Quaternion.AngleAxis(angleDegrees, Vector3.forward) * currentDirection;
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
                    prevPlayerPos = (Vector2)player.transform.position;
                    break;
                }
            }
        }

        visionConeRenderer.material.color = ChangeAlpha(hitPlayer ? Color.red : Color.white, visionConeAlpha);
        if (!hitPlayer) return; 
        
        checkForChase:
        shouldChase = prevPlayerPos != (Vector2)player.transform.position;
        if (shouldChase) return;

        shouldInspect = !inspecting && Array.Exists(itemsOfInterest, x => x == player.spriteRenderer.sprite);
    }

    void LateUpdate()
    {
        if (shouldChase) 
        {
            Debug.Log("hey");
            if (inspecting) StopCoroutine(coroutine);
            shouldChase = false;
            coroutine = Chase();
            StartCoroutine(coroutine);
        }

        if (shouldInspect)
        {
            Debug.Log("ho");
            shouldInspect = false;
            coroutine = Inspect();
            StartCoroutine(coroutine);
        }
    }
}
