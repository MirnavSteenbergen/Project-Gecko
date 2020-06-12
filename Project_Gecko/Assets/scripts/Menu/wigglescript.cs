using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wigglescript : MonoBehaviour
{
    SpriteRenderer theSprite;
    private int wiggleCount = 0;
    public int fall = 50;
    

    private void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        theSprite = GetComponent<SpriteRenderer>();
    }

    void OnWiggle()
    {
        Wiggle();
    }

    public void Wiggle()
    {
        if (wiggleCount == fall)
        {
            GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        }

        if (theSprite.flipX == false)
        {
            theSprite.flipX = true;
            wiggleCount += 1;
        }
        else
        {
            theSprite.flipX = false;
            wiggleCount += 1;
        }

    }
}
