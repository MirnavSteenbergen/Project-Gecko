using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BerryScript : MonoBehaviour
{
    [SerializeField] private float ReappearDuration = 1.0f;

    BoxCollider2D col;
    SpriteRenderer theSprite;
    // Start is called before the first frame update
    void Start()
    {
        col = GetComponent<BoxCollider2D>();
        theSprite = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
     
    }

    //public void BerryPickUp(float powerupDuration)
    //{
    //    col.enabled = false;
    //    theSprite.enabled = false;
    //    StartCoroutine(RespawnBerry());
    //}

    IEnumerator RespawnBerry()
    {
        yield return new WaitForSeconds(ReappearDuration);
        col.enabled = true;
        theSprite.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        Player tempPlayer = collider.GetComponent<Player>();
        if (tempPlayer != null)
        {
            tempPlayer.PickUpBerry();

            col.enabled = false;
            theSprite.enabled = false;
            StartCoroutine(RespawnBerry());
        }
    }
}
