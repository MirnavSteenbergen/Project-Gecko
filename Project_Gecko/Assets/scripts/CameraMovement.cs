using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private Camera cam;
    private BoxCollider2D roomColl;
    //public Vector2 aspectRatio = new Vector2(16, 9);
    //public Vector2 roomSize;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        roomColl = GetComponent<BoxCollider2D>();

        //cam.orthographicSize = roomSize.y * 0.5f;
        //roomColl.size = new Vector2(roomSize.x, roomSize.y);
    }

    // Start is called before the first frame update
    void Start()
    {
        if (Screen.width < Screen.height)
        {
            Screen.SetResolution(Screen.height / 7 * 12, Screen.height, FullScreenMode.MaximizedWindow);
        }
        else
        {
            Screen.SetResolution(Screen.width, Screen.width / 12 * 7, FullScreenMode.MaximizedWindow);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.GetComponent<Player>() != null)
        {
            // Get the directions the player moved to
            int dirX = 0;
            int dirY = 0;
            if (collider.transform.position.x < roomColl.bounds.min.x) dirX -= 1;
            if (collider.transform.position.x > roomColl.bounds.max.x) dirX += 1;
            if (collider.transform.position.y > roomColl.bounds.max.y) dirY += 1;
            if (collider.transform.position.y < roomColl.bounds.min.y) dirY -= 1;

            transform.position += new Vector3(dirX * collider.bounds.size.x, dirY * collider.bounds.size.y);
        }
    }
}
