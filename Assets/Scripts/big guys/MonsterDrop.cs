using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class MonsterDrop : MonoBehaviour
{
    [SerializeField] GameObject drop;
    public void SpawnDrop ()
    {
        drop.SetActive(true);
    }

    private void Start()
    {
        Assert.IsNotNull(drop);
        drop.SetActive(false);
    }
}
