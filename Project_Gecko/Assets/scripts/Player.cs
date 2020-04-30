using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

// Fixed micro sinking into obstacles for a microsecond by turning on "Auto Sync Transforms" under the Physics and Physics 2D tab in the project settings.

[RequireComponent(typeof(Controller2D))]
[RequireComponent(typeof(WallDetection))]
public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float maxSpeed = 6;
    //[SerializeField] float acceleration = 45;
    //[SerializeField] float deceleration = 45;
    [SerializeField] float gravity = -36;
    [SerializeField] float maxFallSpeed = 30;

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

    void Awake()
    {
        controller = GetComponent<Controller2D>();
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        coll = GetComponent<BoxCollider2D>();
        wd = GetComponent<WallDetection>();
        tileGrid = GameObject.FindGameObjectWithTag("Tile Grid").GetComponent<Grid>();
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

        // update alle info voor wall detection
        wd.UpdateWallInfo();

        // check alle corner cases
        CornerCLimbDetection(hor, ver);

        // Stop when hitting collider above or below
        if (controller.collisions.above || controller.collisions.below)
        {
            velocity.y = 0;
        }

        bool wallClimbing = false;

        if (canGrabWalls)
        {
            // als de speler een muur aanraakt wallclimben we
            if (wd.wallLeft || wd.wallRight || wd.wallTop || wd.wallBottom)
            {
                wallClimbing = true;

                // langs verticale muren kun je omhoog of beneden
                if (wd.wallLeft || wd.wallRight)
                {
                    velocity.y = ver * maxSpeed;

                    // stop de speler aan een egde boven
                    if (!wd.wallRightTop && !wd.wallLeftTop && velocity.y > 0) velocity.y = 0;
                    // stop de speler aan een egde onder
                    if (!wd.wallRightBottom && !wd.wallLeftBottom && velocity.y < 0) velocity.y = 0;
                }

                // langs horizontale muren kune je naar links of naar rechts
                if (wd.wallTop || wd.wallBottom)
                {
                    velocity.x = hor * maxSpeed;

                    // stop de speler aan een edge links
                    if (!wd.wallTopLeft && !wd.wallBottomLeft && velocity.x < 0) velocity.x = 0;
                    // stop de speler aan een edge rechts
                    if (!wd.wallTopRight && !wd.wallBottomRight && velocity.x > 0) velocity.x = 0;
                }
            }
        }

        if (!wallClimbing)
        {
            // Apply gravity
            velocity.y += gravity * Time.deltaTime;

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

        // Limit fall speed
        velocity.y = Mathf.Min(velocity.y, maxFallSpeed);

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

    void CornerCLimbDetection(float moveX, float moveY)
    {
        bool movingLeft = moveX < 0;
        bool movingRight = moveX > 0;
        bool movingUp = moveY > 0;
        bool movingDown = moveY < 0;

        if (movingLeft)
        {
            // corner left bottom
            if (wd.wallLeftBottom && !wd.wallLeftTop) StartCoroutine(GoAroundCorner(new Vector2Int(-1, 0), new Vector2Int(-1, -1)));
            // corner left top
            if (!wd.wallLeftBottom && wd.wallLeftTop) StartCoroutine(GoAroundCorner(new Vector2Int(-1, 0), new Vector2Int(-1, 1)));
        }
        if (movingRight)
        {
            // if corner right bottom
            if (wd.wallRightBottom && !wd.wallRightTop) StartCoroutine(GoAroundCorner(new Vector2Int(1, 0), new Vector2Int(1, -1)));
            // if corner right top
            if (!wd.wallRightBottom && wd.wallRightTop) StartCoroutine(GoAroundCorner(new Vector2Int(1, 0), new Vector2Int(1, 1)));
        }
        if (movingUp)
        {
            // corner top left
            if (wd.wallTopLeft && !wd.wallTopRight) StartCoroutine(GoAroundCorner(new Vector2Int(0, 1), new Vector2Int(-1, 1)));
            // corner top right
            if (!wd.wallTopLeft && wd.wallTopRight) StartCoroutine(GoAroundCorner(new Vector2Int(0, 1), new Vector2Int(1, 1)));
        }
        if (movingDown)
        {
            // corner bottom left
            if (wd.wallBottomLeft && !wd.wallBottomRight) StartCoroutine(GoAroundCorner(new Vector2Int(0, -1), new Vector2Int(-1, -1)));
            // corner bottom right
            if (!wd.wallBottomLeft && wd.wallBottomRight) StartCoroutine(GoAroundCorner(new Vector2Int(0, -1), new Vector2Int(1, -1)));
        }
    }

    // Corner direction is from direction from the corner into the corner tile
    IEnumerator GoAroundCorner(Vector2Int moveDirection, Vector2Int cornerDir)
    {
        ignoreMovement = true;

        yield return new WaitForSeconds(0.1f);

        // find the tile the player is on, the tile the player is moving to and the tile the corner is on
        Vector2Int playerTile = (Vector2Int)tileGrid.WorldToCell(transform.position);
        Vector2Int targetTile = new Vector2Int(playerTile.x + moveDirection.x, playerTile.y + moveDirection.y);
        Vector2Int cornerTile = new Vector2Int(playerTile.x + cornerDir.x, playerTile.y + cornerDir.y);

        // get real world position of target tile (NOTE: tile world positions are bottom left)
        Vector2 targetTileWorldPos = tileGrid.CellToWorld(new Vector3Int(targetTile.x, targetTile.y, 0));
        // get direction from target tile to corner tile
        Vector2Int targetToCornerDirection = cornerTile - targetTile;
        // get half tile width and half tile height
        float tileWidthHalf = tileGrid.cellSize.x * 0.5f;
        float tileHeightHalf = tileGrid.cellSize.y * 0.5f;
        // get position of the center of the target tile
        Vector2 targetTileCenter = targetTileWorldPos + new Vector2(tileWidthHalf, tileHeightHalf);

        float newX = targetTileCenter.x - (moveDirection.x * tileWidthHalf) - (targetToCornerDirection.x * (coll.bounds.extents.x - tileWidthHalf));
        float newY = targetTileCenter.y - (moveDirection.y * tileHeightHalf) - (targetToCornerDirection.y * (coll.bounds.extents.y - tileHeightHalf));
        Vector2 newPosition = new Vector2(newX, newY);

        transform.position = newPosition;

        yield return new WaitForSeconds(0.1f);

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
        yield return new WaitForSeconds(0.1f);
        canGrabWalls = true;
    }

    // INPUT

    public void InputMove(InputAction.CallbackContext context)
    {
        inputMove = context.ReadValue<Vector2>();
    }

    public void InputLetGo(InputAction.CallbackContext context)
    {
        inputLetGo = context.ReadValue<float>() == 1;
    }
}