using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacklePlayer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        Player player = collider.GetComponent<Player>();
        if (player != null)
        {
            AudioManager.instance.Play("Beehive");
        }
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        Player player = collider.GetComponent<Player>();
        if (player != null)
        {
            player.ReleaseFromWall();
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        Player player = collider.GetComponent<Player>();
        if (player != null)
        {
            AudioManager.instance.Stop("Beehive");
        }
    }
}
