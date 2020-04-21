using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallDetector : MonoBehaviour
{
   [SerializeField] private LayerMask collisionLayer;
    public bool overlappingWall;

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.layer == LayerMask.NameToLayer("Solids"))
        {
            overlappingWall = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.layer == LayerMask.NameToLayer("Solids"))
        {
            overlappingWall = false;
        }
    }
}
