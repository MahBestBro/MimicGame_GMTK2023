using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class Mimicable : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.transform.parent.GetComponent<Player>();
        if (player != null)
        { 
            player.mimicTargets.Add(this);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        Player player = other.transform.parent.GetComponent<Player>();
        if (player != null)
        { 
            player.mimicTargets.Remove(this);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
