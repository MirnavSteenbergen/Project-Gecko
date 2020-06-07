using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class WallClimbBehaviour : MonoBehaviour
{
    [Header("Wall Climbing")]
    [SerializeField] private float wallCheckDistance = 0.1f;
    [SerializeField] [Range(0.0f, 1.0f)] private float edgeOverhangAllowance = 0.5f;
    [SerializeField] [Range(0.0f, 1.0f)] private float edgeOverhangThreshold = 0.5f;
    [SerializeField] public LayerMask climbableWallMask;
    [SerializeField] public LayerMask UnclimbableWallMask;
    [SerializeField] private float skinWidth = 0.015f;

    private BoxCollider2D coll;

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
        
    }

    bool DetectWall(Vector2 direction, LayerMask mask)
    {
        // TL;DR shoot 2 rays in the given direction. If either ray hits a climbable collider, we have a climbable wall in that direction.

        Bounds bounds = coll.bounds;
        bounds.Expand(skinWidth * -2);

        bool hasWall = false;

        // get the center position of the edge of the bounding box that is pointing in the given direction
        Vector2 bbEdgeCenter = (Vector2)bounds.center + (bounds.extents * direction);
        // get the half length of the egde of the bounding box we want to shoot our rays from (this can become a negative length but it doesn't matter)
        Vector2 bbEdgeHalfLength = bounds.extents * Vector2.Perpendicular(direction);
        // calculate the corner positions of the bounding box edge we want to shoot our rays from
        Vector2 rayAOrigin = bbEdgeCenter - bbEdgeHalfLength;
        Vector2 rayBOrigin = bbEdgeCenter + bbEdgeHalfLength;

        // pew pew
        RaycastHit2D rayAHit = Physics2D.Raycast(rayAOrigin, direction, wallCheckDistance, mask);
        RaycastHit2D rayBHit = Physics2D.Raycast(rayBOrigin, direction, wallCheckDistance, mask);
        // debug the pews
        Debug.DrawRay(rayAOrigin, direction * wallCheckDistance, Color.blue);
        Debug.DrawRay(rayBOrigin, direction * wallCheckDistance, Color.yellow);

        // check whether the rays hit a climbable collider
        if (rayAHit || rayBHit) hasWall = true;

        return hasWall;
    }

    bool DetectEdge(Vector2 direction)
    {
        Bounds bounds = coll.bounds;
        bounds.Expand(skinWidth * -2);

        bool hasEdge = false;

        // get the center position of the back edge of the bounding box, relative to the given direction
        Vector2 bbEdgeCenter = (Vector2)bounds.center + (bounds.extents * -direction);
        // get the half length of the egde of the bounding box we want to shoot our rays from (this can become a negative length but it doesn't matter)
        Vector2 bbEdgeHalfLength = bounds.extents * Vector2.Perpendicular(direction);
        // calculate the corner positions of the bounding box edge we want to shoot our rays from
        Vector2 rayAOrigin = bbEdgeCenter - bbEdgeHalfLength;
        Vector2 rayBOrigin = bbEdgeCenter + bbEdgeHalfLength;

        // Shoot 2 rays of to the side of the edge
        RaycastHit2D rayAHit = Physics2D.Raycast(rayAOrigin, -Vector2.Perpendicular(direction), wallCheckDistance, climbableWallMask);
        RaycastHit2D rayBHit = Physics2D.Raycast(rayBOrigin, Vector2.Perpendicular(direction), wallCheckDistance, climbableWallMask);
        // debug the pews
        Debug.DrawRay(rayAOrigin, -Vector2.Perpendicular(direction) * wallCheckDistance, Color.red);
        Debug.DrawRay(rayBOrigin, Vector2.Perpendicular(direction) * wallCheckDistance, Color.green);

        // if both rays hit there is no edge in sight so we can give up already.
        if (rayAHit && rayBHit) return hasEdge;

        // get the side length of the bb
        Vector2 bbSideLength = bounds.size * new Vector2(Mathf.Abs(direction.x), Mathf.Abs(direction.y));

        if (rayAHit)
        {
            // get the position ray C will shoot from
            Vector2 rayCOrigin = rayAOrigin + direction * bbSideLength * (1.0f - edgeOverhangAllowance);
            // pew
            RaycastHit2D rayCHit = Physics2D.Raycast(rayCOrigin, -Vector2.Perpendicular(direction), wallCheckDistance, climbableWallMask);
            // debug so we can c! haha
            Debug.DrawRay(rayCOrigin, -Vector2.Perpendicular(direction) * wallCheckDistance, Color.yellow);
            if (!rayCHit) hasEdge = true;
        }

        if (rayBHit)
        {
            // get the position ray C will shoot from
            Vector2 rayCOrigin = rayBOrigin + direction * bbSideLength * (1.0f - edgeOverhangAllowance);
            // pew
            RaycastHit2D rayCHit = Physics2D.Raycast(rayCOrigin, Vector2.Perpendicular(direction), wallCheckDistance, climbableWallMask);
            // debug so we can c! Still funny the second time haha
            Debug.DrawRay(rayCOrigin, Vector2.Perpendicular(direction) * wallCheckDistance, Color.yellow);
            if (!rayCHit) hasEdge = true;
        }

        return hasEdge;
    }

    void CheckForCorner(Vector2 direction)
    {
        Bounds bounds = coll.bounds;
        bounds.Expand(skinWidth * -2);

        // get the center position of the edge of the bounding box that is pointing in the given direction
        Vector2 bbEdgeCenter = (Vector2)bounds.center + (bounds.extents * direction);
        // get the half length of the egde of the bounding box we want to shoot our rays from (this can become a negative length but it doesn't matter)
        Vector2 bbEdgeHalfLength = bounds.extents * Vector2.Perpendicular(direction);
        // calculate the corner positions of the bounding box edge we want to shoot our rays from
        Vector2 rayAOrigin = bbEdgeCenter - bbEdgeHalfLength;
        Vector2 rayBOrigin = bbEdgeCenter + bbEdgeHalfLength;

        // pew pew
        RaycastHit2D rayAHit = Physics2D.Raycast(rayAOrigin, direction, wallCheckDistance, climbableWallMask);
        RaycastHit2D rayBHit = Physics2D.Raycast(rayBOrigin, direction, wallCheckDistance, climbableWallMask);

        // if both or neither hit there is no edge so we can give up immediately.
        if ((rayAHit && rayBHit) || (!rayAHit && !rayBHit)) return;

        // the ray that hit is the grounded ray. The ray that didn't hit is the floating ray. The edge direction is from the grounded point towards the floating point.
        // get the position of the floating ray origin, the hit position of the grounded ray, and the direction of the edge
        Vector2 floatingRayOrigin = Vector2.zero;
        Vector2 groundedRayOrigin = Vector2.zero;
        Vector2 groundedRayHitPoint = Vector2.zero;
        Vector2 edgeDirection = Vector2.zero;
        if (!rayAHit) { floatingRayOrigin = rayAOrigin; groundedRayOrigin = rayBOrigin; groundedRayHitPoint = rayBHit.point; edgeDirection = (rayAOrigin - bbEdgeCenter).normalized; }
        if (!rayBHit) { floatingRayOrigin = rayBOrigin; groundedRayOrigin = rayAOrigin; groundedRayHitPoint = rayAHit.point; edgeDirection = (rayBOrigin - bbEdgeCenter).normalized; }
        // debug the grounded ray
        Debug.DrawRay(groundedRayOrigin, direction * wallCheckDistance, Color.white);

        // check if there is an unclimbable surface and not an empty space next to the edge. If so, we don't want to be able to move around the edge
        RaycastHit2D unclimbableRayHit = Physics2D.Raycast(floatingRayOrigin, direction, wallCheckDistance, UnclimbableWallMask);
        if (unclimbableRayHit) return;

        // shoot a ray from the floating position plus an offset, back towards the edge
        RaycastHit2D edgeRayHit = Physics2D.Raycast(floatingRayOrigin + wallCheckDistance * direction, -edgeDirection, Mathf.Infinity, climbableWallMask);
        // debug the edge ray
        Debug.DrawRay(floatingRayOrigin + wallCheckDistance * direction, -edgeDirection * edgeRayHit.distance, Color.white);

        // if the edge ray is shorter than the overhang minimum, we won't be able to move around the edge.
        float overhangMinimum = bbEdgeHalfLength.magnitude * 2 * edgeOverhangThreshold;
        if (edgeRayHit.distance < overhangMinimum) return;

        // calculate which ray hit positions make up the x and y components of the corner position (we don't know which one is x and which one is y, but that doesn't matter)
        Vector2 floatingComponent = edgeRayHit.point * new Vector2(Mathf.Abs(edgeDirection.x), Mathf.Abs(edgeDirection.y));
        Vector2 groundedComponent = groundedRayHitPoint * new Vector2(Mathf.Abs(direction.x), Mathf.Abs(direction.y));

        // calculate the corner position
        Vector2 cornerPos = floatingComponent + groundedComponent;

        //GoAroundCorner(cornerPos, edgeDirection);
    }
}
