using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float defaultSpeed = 1f;

    Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        SetSpeed(defaultSpeed);
    }

    public void SetDirection(float angle)
    {
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void SetSpeed(float speed)
    {
        rb.velocity = transform.right * speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Solids"))
        {
            Destroy(gameObject);
        }
        
        if (collision.collider.tag == "Player")
        {
            collision.collider.GetComponent<Player>().KillPlayer();
            Destroy(gameObject);
        }
    }
}
