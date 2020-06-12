using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BerryScript : MonoBehaviour
{
    [SerializeField] private float ReappearDuration = 1.0f;

    BoxCollider2D col;
    SpriteRenderer spriteRenderer;
    
    void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    IEnumerator RespawnBerry()
    {
        yield return new WaitForSeconds(ReappearDuration);
        col.enabled = true;
        spriteRenderer.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "Player")
        {
            collider.GetComponent<Player>().PickUpBerry();

            col.enabled = false;
            spriteRenderer.enabled = false;
            StartCoroutine(RespawnBerry());

            AudioManager.instance.Play("Berry_Pickup");
        }
    }
}
