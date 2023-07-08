using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CircleCollider2D))]
public class Interactable : MonoBehaviour
{
    public UnityEvent onInteract;

    GameObject prompt;

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

    // Start is called before the first frame update
    void Start()
    {
        prompt = transform.GetChild(0).gameObject;
        prompt.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
