using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MimicableData
{
    public SpriteRenderer spriteRenderer;
    // public Transform transform;
    public MimicableData(SpriteRenderer _spriteRenderer)
    {
        spriteRenderer = _spriteRenderer;
    }
}

[RequireComponent(typeof(CircleCollider2D))]
public class Mimicable : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    Material defaultMaterial;

    void Start()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        defaultMaterial = spriteRenderer.material;
    }

    public MimicableData GetMimicableData()
    {
        return new MimicableData(spriteRenderer);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Mimicer mimicer = other.transform.parent.GetComponent<Mimicer>();
        if (mimicer != null)
        {
            mimicer.AddMimicTarget(this);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        Mimicer mimicer = other.transform.parent.GetComponent<Mimicer>();
        if (mimicer != null)
        {
            mimicer.RemoveMimicTarget(this);
            spriteRenderer.material = defaultMaterial;
        }
    }
}
