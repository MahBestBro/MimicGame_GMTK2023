using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patrol : MonoBehaviour
{
    [SerializeField] List<Vector2> path;
    
    [SerializeField] float defaultSpeed;
    [SerializeField] float chaseSpeed;
    [SerializeField] float returnSpeed;
    
    [SerializeField] float waitTime;

    int destinationIndex = 1;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = (Vector3)path[0];
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (destinationIndex < path.Count)
        {
            Vector2 destination = path[destinationIndex];
            Vector2 direction = destination - path[destinationIndex - 1];
            
            if ((Vector2)transform.position == destination)
            {
                transform.position = destination;
                destinationIndex++;
            }
            
            transform.position += (Vector3)direction * defaultSpeed * Time.fixedDeltaTime;
        }
    }
}
