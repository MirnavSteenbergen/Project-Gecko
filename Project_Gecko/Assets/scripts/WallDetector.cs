using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallDetector : MonoBehaviour
{
   [SerializeField] private LayerMask collisionLayer;
    public bool overlappingWall;
    public Collider2D wallCollider;

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.layer == LayerMask.NameToLayer("Solids"))
        {
            overlappingWall = true;
            wallCollider = collider;
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.layer == LayerMask.NameToLayer("Solids"))
        {
            overlappingWall = false;
            wallCollider = null;
        }
    }
}
