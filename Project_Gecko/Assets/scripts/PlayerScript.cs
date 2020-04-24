using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public float StartMovespeed = 1;
    public float Movespeed = 1;
    public float MovespeedBoostMultiplier = 2;
    public float powerupDuration = 10;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    //private void OnTriggerEnter2D(Collider2D other)
    //{
    //    if (other.gameObject.CompareTag("Berry"))
    //    {
    //        other.gameObject.GetComponent<BerryScript>().BerryPickUp(powerupDuration);
    //        Movespeed *= MovespeedBoostMultiplier;
    //        StartCoroutine( MsDecayRoutine());
    //    }
    //}

    //IEnumerator MsDecayRoutine()
    //{
    //    float step = 1f / powerupDuration;
    //    for (float i = 0f; i < 1f; i += step * Time.deltaTime)
    //    {
    //        //Player. = Mathf.Lerp(Movespeed, StartMovespeed, i);
    //        yield return null;
    //    }
    //}
}
