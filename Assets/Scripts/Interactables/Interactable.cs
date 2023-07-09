using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CircleCollider2D))]
public class Interactable : MonoBehaviour
{
    [SerializeField] GameObject prompt;
    [SerializeField] ParticleSystem triggerInteractionEffect;

    protected virtual void Start()
    {
        if (prompt == null) prompt = transform.GetChild(0).gameObject;
        prompt.SetActive(false);
    }

    public virtual void Interact(GameObject interactor)
    {
        Debug.Log($"{this.name} Interacted with by {interactor.name}");
        if (triggerInteractionEffect)
        {
            triggerInteractionEffect.Play();
        }
    }

    public void DisplayInteractPrompt()
    {
        prompt.SetActive(true);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.transform.parent.GetComponent<Player>();
        if (player != null)
        {
            player.interactTargets.Add(this);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        Player player = other.transform.parent.GetComponent<Player>();
        if (player != null)
        { 
            player.interactTargets.Remove(this);
            prompt.SetActive(false);
        }
    }
}
