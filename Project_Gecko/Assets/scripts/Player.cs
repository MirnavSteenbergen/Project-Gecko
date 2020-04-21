using UnityEngine;
using System.Collections;

// Fixed micro sinking into obstacles for a microsecond by turning on "Auto Sync Transforms" under the Physics and Physics 2D tab in the project settings.

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float maxSpeed = 6;
    [SerializeField] float acceleration = 45;
    [SerializeField] float deceleration = 45;
    [SerializeField] float jumpStrength = 12;
    [SerializeField] float gravity = -36;
    [SerializeField] float maxFallSpeed = 30;

    [Header("Wall Climbing")]
    //[SerializeField] private WallDetector wallDetectorTL;
    //[SerializeField] private WallDetector wallDetectorTR;
    //[SerializeField] private WallDetector wallDetectorBL;
    //[SerializeField] private WallDetector wallDetectorBR;

    [SerializeField] private WallDetector wallDetectorLB;
    [SerializeField] private WallDetector wallDetectorLT;

    bool wallLeftTop;
    bool wallLeftBottom;
    bool wallRightTop;
    bool wallRightBottom;
    bool wallTopLeft;
    bool wallTopRight;
    bool wallBottomLeft;
    bool wallBottomRight;

    private bool ignoreMovement = false;

    [Header("Animation")]
    [SerializeField] float tiltAngle = -15;
    [SerializeField] [Range(0f, 1f)] float squishFactor = 0.25f;

    private Vector2 velocity;
    private bool grounded; // Only used for animation

    private Controller2D controller;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D coll;

    void Awake()
    {
        controller = GetComponent<Controller2D>();
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        coll = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        if (!ignoreMovement) MovePlayer();
        //AnimatePlayer();
    }

    void MovePlayer()
    {
        float hor = Input.GetAxisRaw("Horizontal");
        float ver = Input.GetAxisRaw("Vertical");
        bool jump = Input.GetButtonDown("Jump");
        bool grab = Input.GetKey(KeyCode.Z);



        wallLeftTop = wallDetectorLT.overlappingWall;
        wallLeftBottom = wallDetectorLB.overlappingWall;


        // Y movement

        // Stop when hitting collider above or below
        if (controller.collisions.above || controller.collisions.below)
        {
            velocity.y = 0;
        }

        if (controller.collisions.below && jump)
        {
            velocity.y = jumpStrength;
        }

        WallDetection(grab, hor, ver);

        if ((wallLeftBottom || wallLeftTop) && grab)
        {
            velocity.y = ver * maxSpeed;
            //Debug.Log("grabbing");
        }
        else
        {
            // Apply gravity
            velocity.y += gravity * Time.deltaTime;
        }

        // Limit fall speed
        velocity.y = Mathf.Min(velocity.y, maxFallSpeed);

        // X movement
        if (hor != 0)
        {
            // Accelerate
            velocity.x = Mathf.Sign(hor) * Mathf.Min(Mathf.Abs(velocity.x) + (acceleration * Time.deltaTime), maxSpeed);
        }
        else
        {
            // Decelerate
            velocity.x = Mathf.Sign(velocity.x) * Mathf.Max(Mathf.Abs(velocity.x) - (deceleration * Time.deltaTime), 0);
        }

        // Pass resulting movement to controller
        controller.Move(velocity * Time.deltaTime);


        Debug.Log(velocity);
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
        animator.SetBool("isRunning", velocity.x != 0 && !controller.collisions.left && !controller.collisions.right );
        animator.SetBool("isGrounded", controller.collisions.below );

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

    void WallDetection(bool grab, float moveX, float moveY)
    {
        bool movingLeft     = moveX < 0;
        bool movingRight    = moveX > 0;
        bool movingUp       = moveY > 0;
        bool movingDown     = moveY < 0;

        bool wallLeft;
        bool wallRight;
        bool wallTop;
        bool wallBottom;


        if (grab)
        {
            if (!wallLeftTop && wallLeftBottom && movingLeft)
            {
                StartCoroutine(GoAroundCorner(new Vector2(-1, 1)));
            }
        }
    }

    IEnumerator GoAroundCorner(Vector2 dir)
    {
        Debug.Log("GOING AROUND THE CORNER");
        ignoreMovement = true;

        yield return new WaitForSeconds(0.1f);

        transform.position = new Vector2(transform.position.x + dir.x * 0.5f * coll.size.x, transform.position.y + dir.y * 0.5f * coll.size.y);

        yield return new WaitForSeconds(0.1f);

        ignoreMovement = false;
        velocity = Vector2.zero;
    }
}