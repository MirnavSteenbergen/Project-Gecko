using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BerryScript : MonoBehaviour
{
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

    public void BerryPickUp(float powerupDuration)
    {
        col.enabled = false;
        theSprite.enabled = false;
        StartCoroutine(RespawnBerry(powerupDuration));
    }

    IEnumerator RespawnBerry(float powerupDuration)
    {
        yield return new WaitForSeconds(powerupDuration);
        col.enabled = true;
        theSprite.enabled = true;
    }
}
