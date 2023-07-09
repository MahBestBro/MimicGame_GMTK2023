using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct MimicConversion
{
    public Sprite from;
    public Sprite into;
}

public class MimicConversionTable : Interactable
{
    [SerializeField] List<MimicConversion> conversionTableRef = new List<MimicConversion>();
    Dictionary<Sprite, Sprite> conversions = new Dictionary<Sprite, Sprite>();

    protected override void Start()
    {
        base.Start();
        RebuildConversionsFromTable();
    }

    void RebuildConversionsFromTable()
    {
        conversions.Clear();
        foreach (MimicConversion conversion in conversionTableRef)
        {
            conversions.Add(conversion.from, conversion.into);
        }
    }

    public override void Interact(GameObject interactor)
    {
        base.Interact(interactor);
        Mimicer mimic = interactor.GetComponent<Mimicer>();
        if (mimic)
        {
            Sprite convertedSprite;
            if(conversions.TryGetValue(mimic.GetCurrentSprite(), out convertedSprite))
            {
                mimic.SwapToSprite(convertedSprite);
            }
        }
    }
}
