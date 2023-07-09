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

    AudioSource audioSource; 

    Mimicer mimicry;

    public void Crumch()
    {
        audioSource.volume = Jukebox.volume;
        audioSource.Play();
    }

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rigidBody = GetComponent<Rigidbody2D>();
        mimicry = GetComponent<Mimicer>();
    }

    public Mimicer GetMimicComponent()
    {
        return mimicry;
    }

    void Update()
    {
        // Camera Follow
        Camera.main.transform.position = transform.position + Vector3.back * 10f;

        // Mimicing
        if (Input.GetKeyDown(KeyCode.E))
        {
            mimicry.TriggerMimic();
        }

        // Interacting
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

            if (Input.GetKeyDown(KeyCode.Space)) nearest.Interact(this.gameObject);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Movement
        Vector2 direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        rigidBody.MovePosition(rigidBody.position + direction.normalized * speed * Time.fixedDeltaTime);
    }


}
