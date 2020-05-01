using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CornerDetection : MonoBehaviour
{
    [SerializeField] private float wallCheckDistance = 0.1f;
    [SerializeField] [Range(0.0f, 1.0f)] private float overhangThreshold = 0.5f; // percentage of the collider that must be hanging over the edge before it is allowed to move around the corner
    [SerializeField] public LayerMask wallMask;
    [SerializeField] private float skinWidth = 0.015f;

    BoxCollider2D coll;

    private void Awake()
    {
        coll = GetComponent<BoxCollider2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            Vector2 left = new Vector2(-1, 0);
            Vector2 right = new Vector2(1, 0);
            Vector2 above = new Vector2(0, 1);
            Vector2 below = new Vector2(0, -1);

            //CheckForCorner(left);
        }
    }

    public void CheckForCorner(Vector2 direction)
    {
        Bounds bounds = coll.bounds;
        bounds.Expand(skinWidth * -2);

        // CHECK IF THERE IS AN EDGE

        // TL;DR: Shoot 2 rays from the corners, if one hits and the other does not, there is a corner. Else return false.

        bool hasEdge = false;

        // get position of the center of the side of the bounding box we want to shoot our rays from
        Vector2 sideCenter = (Vector2)bounds.center + (bounds.extents * direction);
        // get the extends (half-length) of the side of the bounding box we want to shoot our rays from
        Vector2 sideExtends = bounds.extents * new Vector2(Mathf.Abs(Vector2.Perpendicular(direction).x), Mathf.Abs(Vector2.Perpendicular(direction).y));
        // calculate the positions we will shoot our rays from (basicly the corners of the bounding box on the side that's pointing in the given direction)
        Vector2 rayAOrigin = sideCenter - sideExtends;
        Vector2 rayBOrigin = sideCenter + sideExtends;

        // pew pew
        RaycastHit2D rayAHit = Physics2D.Raycast(rayAOrigin, direction, wallCheckDistance, wallMask);
        RaycastHit2D rayBHit = Physics2D.Raycast(rayBOrigin, direction, wallCheckDistance, wallMask);
        //debug for debug sake
        Debug.DrawRay(rayAOrigin, direction * wallCheckDistance, Color.green);
        Debug.DrawRay(rayBOrigin, direction * wallCheckDistance, Color.magenta);

        // if either ray does not hit a collider, there is an edge
        if ((rayAHit && !rayBHit) || (rayBHit && !rayAHit))
        {
            hasEdge = true;
        }

        // if there is no edge, we can give up already
        if (!hasEdge) return;

        // CHECK IF THE COLLIDER IS OVER THE EDGE FAR ENOUGH TO GO AROUND IT

        // TL:DR shoot a ray from somewhere along the bounding box edge, then check if it hit.

        // get the direction away from the edge, seen from the direction we shot our rays towards. (if the given direction was below (0, -1), edgeDirection will be either left (-1, 0) or right (1, 0).
        Vector2 edgeDirection = Vector3.zero;
        if (rayAHit) edgeDirection = (sideCenter - rayAOrigin).normalized;
        if (rayBHit) edgeDirection = (sideCenter - rayBOrigin).normalized;
        // get the position of the corner that is against the wall. I call it grounded, but it could be a floor, a wall or a ceiling.
        Vector2 groundedCornerPos = sideCenter - (sideExtends * edgeDirection);
        // calculate the position from where the overhang test ray will be shot from
        Vector2 overhangRayOrigin = groundedCornerPos + (edgeDirection * (1.0f - overhangThreshold) * sideExtends * 2);

        // pew
        RaycastHit2D overhangRayHit = Physics2D.Raycast(overhangRayOrigin, direction, wallCheckDistance, wallMask);
        // debug the pew
        Debug.DrawRay(overhangRayOrigin, direction * wallCheckDistance, Color.yellow);

        // if the overhang test ray hit, the collider is not allowed to go around the edge and there's really no point in continuing this function. Else, go on!
        if (overhangRayHit)
        {
            return;
        }

        // FIND THE CORNER POSITION

        // TL;DR shoot 2 rays, one from the tip of the overhang test ray towards the corner, one from the grounded corner towards the original direction. Their contact x and y make the position of the corner.

        // ray C is shot from the tip of the overhang test ray
        // ray D is shot from the grounded corner (essentially the same as ray A or ray B)

        // get the positions of the origins of the rays
        Vector2 rayCOrigin = overhangRayOrigin + (direction * wallCheckDistance);
        Vector2 rayDOrigin = groundedCornerPos;
        // shoot the rays
        RaycastHit2D rayCHit = Physics2D.Raycast(rayCOrigin, -edgeDirection, Mathf.Infinity, wallMask);
        RaycastHit2D rayDhit = Physics2D.Raycast(rayDOrigin, direction, wallCheckDistance, wallMask);
        // debug the pews
        Debug.DrawRay(rayCOrigin, -edgeDirection * rayCHit.distance, Color.white);
        Debug.DrawRay(rayDOrigin, direction * rayDhit.distance, Color.white);

        // calculate the x and y components of the corner position based on the contact points and the given direction.
        Vector2 componentC = rayCHit.point * new Vector2(Mathf.Abs(Vector2.Perpendicular(direction).x), Mathf.Abs(Vector2.Perpendicular(direction).y));
        Vector2 componentD = rayDhit.point * new Vector2(Mathf.Abs(direction.x), Mathf.Abs(direction.y));

        // calculate the corner position with the components ((x, 0) and (0, y), but we don't know which one is C and D)
        Vector2 cornerPos = componentC + componentD;

        // debug the corner position for fun!
        Debug.DrawLine(transform.position, cornerPos, Color.red);

        // MOVING AROUND THE CORNER

        // the time has come. We know all the variables. Let's move around that corner!
        //GoAroundCorner(cornerPos, edgeDirection);

        if (GetComponent<Player>() != null)
        {
            GetComponent<Player>().GoAroundCorner(cornerPos, edgeDirection);
        }
    }

    void GoAroundCorner(Vector2 cornerPos, Vector2 translateDirection)
    {
        coll.transform.position = cornerPos + translateDirection * coll.bounds.extents;
    }
}
