using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Mimicer : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Shader spriteShaderValidator;

    public List<Mimicable> mimicTargets;
    [SerializeField] Collider2D mimicArea;

    Mimicable? previousMimicable = null;


    void Start()
    {
        Assert.IsNotNull(spriteRenderer);
        Assert.IsNotNull(spriteShaderValidator);
        mimicArea.enabled = false;
    }

    // Get Nearest Mimicable in mimicTargets in direction of cursor
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

    public void TriggerMimic()
    {
        if (!mimicArea.enabled) // If Mimic Area is disabled, Enable it for mimic selection
        {
            mimicArea.enabled = true;
        }
        else
        {
            // If Mimic Area was already enabled, Disable it and confirm mimicry

            // Transform into nearest mimicable
            if (GetNearestMimicable() is Mimicable nearest)
            {
                TransformIntoMimicable(nearest);
            }

            // disable mimic area & clear mimic targets
            mimicArea.enabled = false;
            mimicTargets.Clear();
            // If previous mimicable was highlighted, untrack and unhighlight
            if (previousMimicable is Mimicable) SetHighlightOnSprite(previousMimicable.GetComponent<SpriteRenderer>(), false);
            previousMimicable = null;
        }
    }

    void TrackPreviousMimicable()
    {
        if (GetNearestMimicable() is Mimicable nearestMimicable)
        {
            // Unhighlight previous last tracked mimicable
            if (previousMimicable is Mimicable prevMimicable)
            {
                SetHighlightOnSprite(previousMimicable.GetComponent<SpriteRenderer>(), false);
            }
            // Update tracked previousMimicable
            previousMimicable = nearestMimicable;
            // Update material to highlight
            SpriteRenderer nearestSR = nearestMimicable.GetComponent<SpriteRenderer>();
            SetHighlightOnSprite(nearestSR, true);
        }
    }

    void SetHighlightOnSprite(SpriteRenderer target, bool highlightEnabled)
    {
        if (target.material.shader != spriteShaderValidator) Debug.LogError("Tried to Highlight Sprite without valid material");

        const string HIGHLIGHT_KEYWORD = "_HIGHLIGHTENABLED";
        if (highlightEnabled)
        {
            target.material.EnableKeyword(HIGHLIGHT_KEYWORD);
        }else
        {
            target.material.DisableKeyword(HIGHLIGHT_KEYWORD);
        }
        
    }

    void TransformIntoMimicable(Mimicable mimicable)
    {
        MimicableData mimicableData = mimicable.GetMimicableData();
        spriteRenderer.sprite = mimicableData.spriteRenderer.sprite;
        Vector3 mimicableScale = mimicableData.spriteRenderer.transform.lossyScale;
        spriteRenderer.transform.localScale = mimicableScale;
        spriteRenderer.transform.localPosition += new Vector3(0, -spriteRenderer.transform.localPosition.y + (mimicableData.spriteRenderer.sprite.pivot.y - mimicableScale.y));
    }

    public void AddMimicTarget(Mimicable mimicable)
    {
        mimicTargets.Add(mimicable);
    }

    public void RemoveMimicTarget(Mimicable mimicable)
    {
        if(mimicTargets.Count>0) mimicTargets.Remove(mimicable);
    }

    public bool isDisguisedAs(Sprite sprite)
    {
        return spriteRenderer.sprite == sprite;
    }

    void Update()
    {
        TrackPreviousMimicable();
    }
}
