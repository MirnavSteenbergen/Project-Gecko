using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Controller2D))]
public class MovingEnemy : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float maxSpeed = 6;
    [SerializeField] float gravity = -36;
    [SerializeField] float maxFallSpeed = 30;
    [SerializeField] private Vector2 moveDirection;

    [Header("Wall Climbing")]
    [SerializeField] private float wallCheckDistance = 0.1f;
    [SerializeField] [Range(0.0f, 1.0f)] private float edgeOverhangAllowance = 0.5f;
    [SerializeField] [Range(0.0f, 1.0f)] private float edgeOverhangThreshold = 0.5f;
    [SerializeField] public LayerMask climbableWallMask;
    [SerializeField] public LayerMask UnclimbableWallMask;
    [SerializeField] private float skinWidth = 0.015f;

    [HideInInspector] public Vector2 velocity;
    private bool canGrabWalls = true;

    private Controller2D controller;
    private BoxCollider2D coll;
    private SpriteRenderer sprite;

    private void Awake()
    {
        controller = GetComponent<Controller2D>();
        coll = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        moveDirection = Vector2.right;
    }

    // Update is called once per frame
    void Update()
    {
        MoveEnemy();
    }

    void MoveEnemy()
    {
        float hor = moveDirection.x;
        float ver = moveDirection.y;
        //bool letGo = inputLetGo;

        bool wallClimbing = false;

        // we use these to check if there is a climbable surface in any direction
        bool hasWallLeft = DetectWall(Vector2.left, climbableWallMask);
        bool hasWallRight = DetectWall(Vector2.right, climbableWallMask);
        bool hasWallAbove = DetectWall(Vector2.up, climbableWallMask);
        bool hasWallBelow = DetectWall(Vector2.down, climbableWallMask);

        bool hasEdgeDown = DetectEdge(Vector2.down);
        bool hasEdgeLeft = DetectEdge(Vector2.left);
        bool hasEdgeRight = DetectEdge(Vector2.right);

        // ENEMY AI

        if (hor < 0 && (controller.collisions.left || hasEdgeLeft)) moveDirection = Vector2.right;
        if (hor > 0 && (controller.collisions.right || hasEdgeRight)) moveDirection = Vector2.left;

        if (canGrabWalls)
        {
            if (hasWallAbove || hasWallBelow || hasWallLeft || hasWallRight)
            {
                wallClimbing = true;

                // check edges to move around them
                //cd.CheckForCorner(new Vector2(hor, 0));
                //cd.CheckForCorner(new Vector2(0, ver));

                if (hasWallAbove || hasWallBelow)
                {
                    velocity.x = hor * maxSpeed;

                    // stop de speler aan een edge links
                    if (hasEdgeLeft && velocity.x < 0) velocity.x = 0;
                    // stop de speler aan een edge rechts
                    if (hasEdgeRight && velocity.x > 0) velocity.x = 0;
                }

                if (hasWallLeft || hasWallRight)
                {
                    velocity.y = ver * maxSpeed;

                    // stop de speler aan een egde boven
                    if (DetectEdge(Vector2.up) && velocity.y > 0) velocity.y = 0;
                    // stop de speler aan een egde onder
                    if (hasEdgeDown && velocity.y < 0) velocity.y = 0;
                }

                // als de speler unclimbable surface onder zich heeft, mag hij er wel overheen bewegen
                if (DetectWall(Vector2.down, UnclimbableWallMask))
                {
                    velocity.x = hor * maxSpeed;
                }

                if (hasWallLeft && hor < 0) CheckForCorner(Vector2.left);
                if (hasWallRight && hor > 0) CheckForCorner(Vector2.right);
                if (hasWallAbove && ver > 0) CheckForCorner(Vector2.up);
                if (hasWallBelow && ver < 0) CheckForCorner(Vector2.down);
            }
        }


        if (!wallClimbing)
        {
            // Stop when hitting collider above or below
            if (controller.collisions.above || controller.collisions.below)
            {
                velocity.y = 0;
            }

            // Apply gravity
            velocity.y += gravity * Time.deltaTime;

            // Limit fall speed
            velocity.y = Mathf.Max(velocity.y, -maxFallSpeed);

            // we kunnen in de lucht ook naar links of rechts
            velocity.x = hor * maxSpeed;
        }
        else
        {
            // loslaten
            //if (letGo)
            //{
            //    wallClimbing = false;
            //    // start cooldown zodat we niet meteen weer beginnen te klimmen
            //    ReleaseFromWall();
            //}
        }

        // Pass resulting movement to controller
        controller.Move(velocity * Time.deltaTime);

        // animation

        sprite.flipX = velocity.x < 0;
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
