using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

// Fixed micro sinking into obstacles for a microsecond by turning on "Auto Sync Transforms" under the Physics and Physics 2D tab in the project settings.

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float maxSpeed = 6;
    //[SerializeField] float acceleration = 45;
    //[SerializeField] float deceleration = 45;
    [SerializeField] float gravity = -36;
    [SerializeField] float maxFallSpeed = 30;

    [Header("Wall Climbing")]
    [SerializeField] private float wallCheckDistance = 0.1f;
    [SerializeField] [Range(0.0f, 1.0f)] private float edgeOverhangAllowance = 0.5f;
    [SerializeField] [Range(0.0f, 1.0f)] private float edgeOverhangThreshold = 0.5f;
    [SerializeField] public LayerMask climbableWallMask;
    [SerializeField] public LayerMask UnclimbableWallMask;
    [SerializeField] private float skinWidth = 0.015f;

    [Header("Berry")]
    [SerializeField] private float powerUpDuration = 1.0f;

    [Header("Animation")]
    [SerializeField] float tiltAngle = -15;
    [SerializeField] [Range(0f, 1f)] float squishFactor = 0.25f;

    // private variables
    [HideInInspector] public Vector2 velocity;
    private bool grounded; // Only used for animation
    private bool ignoreMovement = false;
    private bool canGrabWalls = true;

    private Vector2 lastCheckPointPosition;

    // Input
    private Vector2 inputMove;
    private bool inputLetGo;

    // Components
    private Controller2D controller;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D coll;
    private WallDetection wd;
    private Grid tileGrid;
    private CornerDetection cd;

    void Awake()
    {
        controller = GetComponent<Controller2D>();
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        coll = GetComponent<BoxCollider2D>();
        wd = GetComponent<WallDetection>();
        tileGrid = GameObject.FindGameObjectWithTag("Tile Grid").GetComponent<Grid>();
        cd = GetComponent<CornerDetection>();
    }

    private void Start()
    {

    }

    void Update()
    {
        if (!ignoreMovement) MovePlayer();
        //AnimatePlayer();
    }

    void MovePlayer()
    {
        float hor = inputMove.x;
        float ver = inputMove.y;
        bool letGo = inputLetGo;

        bool wallClimbing = false;

        // we use these to check if there is a climbable surface in any direction
        bool hasWallLeft  = DetectWall(Vector2.left, climbableWallMask);
        bool hasWallRight = DetectWall(Vector2.right, climbableWallMask);
        bool hasWallAbove = DetectWall(Vector2.up, climbableWallMask);
        bool hasWallBelow = DetectWall(Vector2.down, climbableWallMask);

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
                    if (DetectEdge(Vector2.left) && velocity.x < 0) velocity.x = 0;
                    // stop de speler aan een edge rechts
                    if (DetectEdge(Vector2.right) && velocity.x > 0) velocity.x = 0;
                }

                if (hasWallLeft || hasWallRight)
                {
                    velocity.y = ver * maxSpeed;

                    // stop de speler aan een egde boven
                    if (DetectEdge(Vector2.up) && velocity.y > 0) velocity.y = 0;
                    // stop de speler aan een egde onder
                    if (DetectEdge(Vector2.down) && velocity.y < 0) velocity.y = 0;
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
            if (letGo)
            {
                wallClimbing = false;
                // start cooldown zodat we niet meteen weer beginnen te klimmen
                ReleaseFromWall();
            }
        }

        // Pass resulting movement to controller
        controller.Move(velocity * Time.deltaTime);
    }

    void AnimatePlayer()
    {
        // Sprite animations
        if (velocity.x < 0)
        { spriteRenderer.flipX = true; }
        else if (velocity.x > 0)
        { spriteRenderer.flipX = false; }

        // Animator
        animator.SetFloat("velY", velocity.y);
        animator.SetBool("isRunning", velocity.x != 0 && !controller.collisions.left && !controller.collisions.right);
        animator.SetBool("isGrounded", controller.collisions.below);

        // Tilt
        if (!controller.collisions.left && !controller.collisions.right)
        {
            float speedPercentage;
            speedPercentage = velocity.x / maxSpeed;
            spriteRenderer.transform.rotation = Quaternion.Euler(0, 0, speedPercentage * tiltAngle);
        }
        else
        {
            spriteRenderer.transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        // Squish
        if (controller.collisions.below && !grounded)
        {
            // Landing
            grounded = true;
            spriteRenderer.transform.localScale = new Vector3(1f + squishFactor, 1f - squishFactor, 1);
        }
        if (grounded && velocity.y > 0)
        {
            // Jumping
            spriteRenderer.transform.localScale = new Vector3(1f - squishFactor, 1f + squishFactor, 1);
        }
        if (!controller.collisions.below && grounded)
        {
            // Jumping, being launched or walking of a ledge
            grounded = false;
        }

        // Lerp back to normal scale
        if (spriteRenderer.transform.localScale != Vector3.one)
        {
            spriteRenderer.transform.localScale = Vector3.Lerp(spriteRenderer.transform.localScale, Vector3.one, 10 * Time.deltaTime);
        }

        // Round lerp when close to one.
        if (Mathf.Round(spriteRenderer.transform.localScale.x * 100) / 100f == 1f && Mathf.Round(spriteRenderer.transform.localScale.y * 100) / 100f == 1f)
        {
            spriteRenderer.transform.localScale = Vector3.one;
        }
    }

    public void GoAroundCorner(Vector2 cornerPos, Vector2 translateDirection)
    {
        StartCoroutine(GoAroundCornerRoutine(cornerPos, translateDirection));
    }

    IEnumerator GoAroundCornerRoutine(Vector2 cornerPos, Vector2 translateDirection)
    {
        ignoreMovement = true;

        //yield return new WaitForSeconds(0.1f);

        coll.transform.position = cornerPos + translateDirection * coll.bounds.extents;

        yield return new WaitForSeconds(0.2f);

        ignoreMovement = false;
        velocity = Vector2.zero;
    }
    
    public void SetCheckPointPosition(Vector2 pos)
    {
        lastCheckPointPosition = pos;
    }

    public void KillPlayer()
    {
        transform.position = lastCheckPointPosition;
        velocity = Vector2.zero;
    }

    public void PickUpBerry()
    {
        maxSpeed = 12;
        StartCoroutine(MsDecayRoutine());
    }

    IEnumerator MsDecayRoutine()
    {
        float step = 1f / powerUpDuration;
        for (float i = 0f; i < 1f; i += step * Time.deltaTime)
        {
            maxSpeed = Mathf.Lerp(12, 6, i);
            yield return null;
        }
    }

    public void ReleaseFromWall()
    {
        StartCoroutine(releaseCooldown());
    }

    IEnumerator releaseCooldown()
    {
        canGrabWalls = false;
        yield return new WaitForSeconds(0.05f);
        canGrabWalls = true;
    }

    // INPUT

    public void InputMove(InputAction.CallbackContext context)
    {
        inputMove = context.ReadValue<Vector2>();
    }

    public void InputLetGo(InputAction.CallbackContext context)
    {
        if (context.started) inputLetGo = true;
        if (context.canceled) inputLetGo = false;
    }

    // WALL CLIMBING

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

        GoAroundCorner(cornerPos, edgeDirection);
    }
}