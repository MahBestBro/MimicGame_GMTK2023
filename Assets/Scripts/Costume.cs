using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Costume : MonoBehaviour
{
    [SerializeField] SpriteRenderer helmet;
    public GameObject helmetInWorld;

    private void Start()
    {
        helmet.gameObject.SetActive(false);
    }

    public void WearHelmet()
    {
        helmet.gameObject.SetActive(true);
        helmetInWorld.SetActive(false);
    }
}
