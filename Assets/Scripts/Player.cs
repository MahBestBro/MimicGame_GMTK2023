using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [HideInInspector] public List<Mimicable> mimicTargets;
    [HideInInspector] public List<Interactable> interactTargets;

    [SerializeField] Material mimicSelectMaterial;
    [SerializeField] float speed;

    Rigidbody2D rigidBody;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Collider2D mimicArea;

    Material originalMimicMaterial;
    Mimicable? previousMimicable = null;

    public void Shart()
    {
        Debug.Log("pfft");
    }

    // Start is called before the first frame update
    void Start()
    {   
        rigidBody = GetComponent<Rigidbody2D>();
        mimicArea.enabled = false;
    }

    Mimicable? GetNearestMimicable()
    {
        if (mimicTargets.Count == 0) return null;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        Vector3 toMouse = Vector3.Normalize(mouseWorldPos - transform.position);

        Mimicable nearest = mimicTargets[0]; 
        foreach (Mimicable target in mimicTargets)
        {
            Vector3 toNearest = Vector3.Normalize(nearest.transform.position - transform.position); 
            Vector3 toTarget = Vector3.Normalize(target.transform.position - transform.position); 
            //Compare angles by comparing dot products
            if (Vector3.Dot(toTarget, toMouse) > Vector3.Dot(toNearest, toMouse)) nearest = target;
        }
        return nearest;
    }

    void Update()
    {
        //Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //mouseWorldPos.z = 0f;

        Camera.main.transform.position = transform.position + Vector3.back * 10f; 

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!mimicArea.enabled)
            {
                mimicArea.enabled = true;
            }
            else 
            {
                if (GetNearestMimicable() is Mimicable nearest)
                {
                    SpriteRenderer nearestSR = nearest.GetComponent<SpriteRenderer>();
                    spriteRenderer.sprite = nearestSR.sprite;
                }

                mimicArea.enabled = false;
                mimicTargets.Clear();
                ((Mimicable)previousMimicable).GetComponent<SpriteRenderer>().material = originalMimicMaterial;
                previousMimicable = null;
            }
        }

        if (GetNearestMimicable() is Mimicable nearestMimicable)
        {
            if (previousMimicable is Mimicable prevMimicable) 
            {
                prevMimicable.GetComponent<SpriteRenderer>().material = originalMimicMaterial;
            }
            
            previousMimicable = nearestMimicable; 
            SpriteRenderer nearestSR = nearestMimicable.GetComponent<SpriteRenderer>();
            originalMimicMaterial = nearestSR.material;
            nearestSR.material = mimicSelectMaterial;
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
