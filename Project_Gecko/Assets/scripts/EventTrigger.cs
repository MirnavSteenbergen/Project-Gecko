using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventTrigger : MonoBehaviour
{
    [SerializeField] private UnityEvent triggerEvent = new UnityEvent();

    private void OnTriggerEnter2D(Collider2D collider)
    {
        {
            Debug.Log("a thing collided");
            triggerEvent.Invoke();
        }
    }
}
