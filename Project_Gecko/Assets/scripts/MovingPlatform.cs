using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private Vector2 localOtherPosition;

    private Vector2 originalPosition;
    private Vector2 otherPosition;
    private BoxCollider2D coll;

    private void Awake()
    {
        coll = GetComponent<BoxCollider2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        originalPosition = transform.position;
        otherPosition = originalPosition + localOtherPosition;

        MoveFromTo(originalPosition, otherPosition);
    }

    IEnumerator MoveToPointRoutine(Vector2 startPoint, Vector2 targetPoint, float speed)
    {
        float duration = localOtherPosition.magnitude / speed;

        float step = 1f / duration;
        for (float i = 0f; i < 1f; i += step * Time.deltaTime)
        {
            transform.position = Vector2.Lerp(startPoint, targetPoint, i);
            yield return null;
        }

        MoveFromTo(targetPoint, startPoint);
    }

    void MoveFromTo(Vector2 from, Vector2 to)
    {
        StartCoroutine(MoveToPointRoutine(from, to, moveSpeed));
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.GetComponent<Player>() != null)
        {
            collider.transform.SetParent(transform);
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.GetComponent<Player>() != null)
        {
            collider.transform.SetParent(null);
        }
    }

    // show the point the platform will move to in the editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube((Vector2)transform.position + localOtherPosition, transform.localScale);
    }
}
