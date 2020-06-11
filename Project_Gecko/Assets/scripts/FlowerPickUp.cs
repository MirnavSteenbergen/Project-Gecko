using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerPickUp : MonoBehaviour
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
        if (collider.GetComponent<Player>() != null)
        {
            FlowerCounter.addFlower();
            Destroy(gameObject);
        }
    }
}
