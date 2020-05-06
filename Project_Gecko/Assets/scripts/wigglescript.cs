using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wigglescript : MonoBehaviour
{
    SpriteRenderer theSprite;
    private int wiggleCount = 0;
    public int fall = 50;

    // Start is called before the first frame update
    void Start()
    {
        theSprite = GetComponent<SpriteRenderer>();
    }


    public void Wiggle()
    {
        if (wiggleCount == fall)
        {
            GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        }

        if (theSprite.flipY == false)
        {
            theSprite.flipY = true;
            wiggleCount += 1;
        }
        else
        {
            theSprite.flipY = false;
            wiggleCount += 1;
        }

    }
}
