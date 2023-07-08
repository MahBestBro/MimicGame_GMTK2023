using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [HideInInspector] public List<Mimicable> mimicTargets;
    [HideInInspector] public List<Interactable> interactTargets;

    [SerializeField] float speed;

    Rigidbody2D rigidBody;
    SpriteRenderer spriteRenderer;
    Collider2D mimicArea;

    public void Shart()
    {
        Debug.Log("pfft");
    }

    // Start is called before the first frame update
    void Start()
    {   
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        mimicArea = transform.GetChild(0).GetComponent<CircleCollider2D>();

        mimicArea.enabled = false;
    }

    void Update()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!mimicArea.enabled)
            {
                mimicArea.enabled = true;
                spriteRenderer.color = Color.blue;
            }
            else 
            {
                if (mimicTargets.Count > 0)
                {
                    Vector3 toMouse = Vector3.Normalize(mouseWorldPos - transform.position);

                    Mimicable nearest = mimicTargets[0]; 
                    foreach (Mimicable target in mimicTargets)
                    {
                        Vector3 toNearest = Vector3.Normalize(nearest.transform.position - transform.position); 
                        Vector3 toTarget = Vector3.Normalize(target.transform.position - transform.position); 
                        //Compare angles by comparing dot products
                        if (Vector3.Dot(toTarget, toMouse) > Vector3.Dot(toNearest, toMouse)) nearest = target;
                    }

                    SpriteRenderer nearestSR = nearest.GetComponent<SpriteRenderer>();
                    spriteRenderer.sprite = nearestSR.sprite;
                    spriteRenderer.color = nearestSR.color;
                }
                else
                {
                    spriteRenderer.color = Color.white;
                }

                mimicArea.enabled = false;
                mimicTargets.Clear();
            }
        }

        if (interactTargets.Count > 0)
        {
            Interactable nearest = interactTargets[0];
            foreach (Interactable target in interactTargets)
            {
                float nearestSqDist = (nearest.transform.position - transform.position).sqrMagnitude; 
                float targetSqDist = (target.transform.position - transform.position).sqrMagnitude; 
                if (targetSqDist < nearestSqDist) nearest = target;
            }

            nearest.DisplayInteractPrompt();

            if (Input.GetKeyDown(KeyCode.Space)) nearest.onInteract.Invoke();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector2 direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        rigidBody.MovePosition(rigidBody.position + direction * speed * Time.fixedDeltaTime);
    }
}
