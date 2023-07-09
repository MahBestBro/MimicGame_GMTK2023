using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DemoMimicry : MonoBehaviour
{
    [SerializeField] Material shaderMaterial;

    // Start is called before the first frame update
    void Start()
    {
        //Image image = GetComponent<Image>();
        //image.material = shaderMaterial;
        //const string HIGHLIGHT_KEYWORD = "_HIGHLIGHTENABLED";
        //image.material.EnableKeyword(HIGHLIGHT_KEYWORD);
    }

    //void SetHighlightOnSprite(SpriteRenderer target, bool highlightEnabled)
    //{
    //    if (target.material.shader != spriteShaderValidator) Debug.LogError("Tried to Highlight Sprite without valid material");
//
    //    
    //    if (highlightEnabled)
    //    {
    //        
    //    }else
    //    {
    //        target.material.DisableKeyword(HIGHLIGHT_KEYWORD);
    //    }
    //    
    }
