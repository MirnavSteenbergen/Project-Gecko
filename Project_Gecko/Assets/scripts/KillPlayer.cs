using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillPlayer : MonoBehaviour
{
    [SerializeField] Transform spawnPoint;

    private void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.transform.CompareTag("Player"))
            coll.transform.position = spawnPoint.position;

    }
}
