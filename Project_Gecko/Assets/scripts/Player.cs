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

        // update alle info voor wall detection
        wd.UpdateWallInfo();

        // check alle corner cases
        cd.CheckForCorner(new Vector2(hor, 0));
        cd.CheckForCorner(new Vector2(0, ver));

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

                // langs verticale muren kun je naar boven of naar beneden
                if (wd.wallLeft || wd.wallRight)
                {
                    velocity.y = ver * maxSpeed;

                    // stop de speler aan een egde boven
                    if (!wd.wallRightTop && !wd.wallLeftTop && velocity.y > 0) velocity.y = 0;
                    // stop de speler aan een egde onder
                    if (!wd.wallRightBottom && !wd.wallLeftBottom && velocity.y < 0) velocity.y = 0;
                }

                // langs horizontale muren kun je naar links of naar rechts
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

            //if (controller.collisions.below)
            //{
            //    wallClimbing = false;
            //    ReleaseFromWall();
            //}
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

    public void GoAroundCorner(Vector2 cornerPos, Vector2 translateDirection)
    {
        StartCoroutine(GoAroundCornerRoutine(cornerPos, translateDirection));
    }

    IEnumerator GoAroundCornerRoutine(Vector2 cornerPos, Vector2 translateDirection)
    {
        ignoreMovement = true;

        yield return new WaitForSeconds(0.1f);

        coll.transform.position = cornerPos + translateDirection * coll.bounds.extents;

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
}