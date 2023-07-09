using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Mimicer))]
public class Player : MonoBehaviour
{
    [HideInInspector] public List<Interactable> interactTargets;

    [SerializeField] float speed;

    Rigidbody2D rigidBody;

    Mimicer mimicry;

    public void Shart()
    {
        Debug.Log("pfft");
    }

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        mimicry = GetComponent<Mimicer>();
    }



    void Update()
    {
        //Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //mouseWorldPos.z = 0f;

        Camera.main.transform.position = transform.position + Vector3.back * 10f;

        if (Input.GetKeyDown(KeyCode.E))
        {
            mimicry.TriggerMimic();
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
