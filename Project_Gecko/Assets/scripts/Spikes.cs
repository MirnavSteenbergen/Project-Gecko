using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikes : MonoBehaviour
{
    private BoxCollider2D coll;

    [SerializeField] [Range(0, 360)] private float hitCone = 90f;

    private void Awake()
    {
        coll = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        // check if player has hit us
        Player player = collider.GetComponent<Player>();
        if (player != null)
        {
            // get the rotation of the spike in a vector
            Vector2 direction = new Vector2(Mathf.Cos((transform.rotation.eulerAngles.z + 90) * Mathf.Deg2Rad), Mathf.Sin((transform.rotation.eulerAngles.z + 90) * Mathf.Deg2Rad));
            // calculate the angle difference between the negative direction of the spike and the motion direction of the player
            float angleDif = Vector2.SignedAngle(-direction, player.velocity);
            // if the player entered the hitbox moving towards the pointy side of the spike (z rotation 0), it will kill them.
            if (angleDif > -(hitCone * 0.5f) && angleDif < (hitCone * 0.5f))
            {
                player.KillPlayer();
            }
        }

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, new Vector2(Mathf.Cos((transform.rotation.eulerAngles.z - hitCone * 0.5f + 90) * Mathf.Deg2Rad), Mathf.Sin((transform.rotation.eulerAngles.z - hitCone * 0.5f + 90) * Mathf.Deg2Rad)) * 0.5f);
        Gizmos.DrawRay(transform.position, new Vector2(Mathf.Cos((transform.rotation.eulerAngles.z + hitCone * 0.5f + 90) * Mathf.Deg2Rad), Mathf.Sin((transform.rotation.eulerAngles.z + hitCone * 0.5f + 90) * Mathf.Deg2Rad)) * 0.5f);
    }
}
