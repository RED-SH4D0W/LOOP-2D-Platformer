using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

public class Player : MonoBehaviour
{

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    private bool isDead;
    [HideInInspector] public bool playerUnlocked;
    [HideInInspector] public bool extraLife;

    [Header("VFX")]
    [SerializeField] private ParticleSystem dustFX;
    [SerializeField] private ParticleSystem bloodFX;

    [Header("Knockback info")]
    [SerializeField] private Vector2 knockbackDir;
    private bool isKnocked;
    private bool canBeKnocked = true;

    [Header("Move info")]
    [SerializeField] private float speedToSurvive = 18;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float speedMultiplier;
    private float defaultSpeed;
    [Space]
    [SerializeField] private float milestoneIncreaser;
    private float defaultMilestoneIncrease;
    private float speedMilestone;

    private bool readyToLand;

    [Header("Jump info")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float doubleJumpForce;
    private bool canDoubleJump;



    [Header("Slide info")]
    [SerializeField] private float slideSpeed;
    [SerializeField] private float slideTime;
    [SerializeField] private float slideCooldown;
    private float slideCooldownCounter;
    private float slideTimeCounter;
    private bool isSliding;

    [Header("Collision info")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float ceilingCheckDistance;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private Vector2 wallCheckSize;
    private bool isGrounded;
    private bool wallDetected;
    private bool ceilingDetected;
    [HideInInspector] public bool ledgeDetected;

    [Header("Ledge info")]
    [SerializeField] private Vector2 offset1; // offset for position before climb
    [SerializeField] private Vector2 offset2; // offset for position AFTER climb

    private Vector2 climbBegunPosition;
    private Vector2 climbOverPosition;

    private bool canGrabLedge = true;
    private bool canClimb;




    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        speedMilestone = milestoneIncreaser;
        defaultSpeed = moveSpeed;
        defaultMilestoneIncrease = milestoneIncreaser;
    }


    void Update()
    {
        CheckCollision();
        AnimatorControllers();

        slideTimeCounter -= Time.deltaTime;
        slideCooldownCounter -= Time.deltaTime;

        extraLife = moveSpeed >= speedToSurvive;

   	    //if(Input.GetKeyDown(KeyCode.K))
        	//Knockback();

        //if (Input.GetKeyDown(KeyCode.O) && !isDead)
            //StartCoroutine(Die());


        if (isDead)
            return;

        if (isKnocked)
            return;

        if (playerUnlocked)
            SetupMovement();

        if (isGrounded)
            canDoubleJump = true;

        SpeedController();
        CheckForLanding();
        CheckForLedge();
        CheckForSlideCancel();
        CheckInput();
    }

    private void CheckForLanding()
    {
        if(rb.velocity.y < -5 && !isGrounded)
            readyToLand = true;

        if(readyToLand && isGrounded)
        {
            dustFX.Play();
            readyToLand = false;
        }
    }

    public void Damage()
    {
        bloodFX.Play();

        if(extraLife)
            Knockback();
        else
            StartCoroutine(Die());
    }

    private IEnumerator Die()
    {
        AudioManager.instance.PlaySFX(4);
        isDead = true;
        canBeKnocked = false;
        rb.velocity = knockbackDir;
        anim.SetBool("isDead", true);

        Time.timeScale = .6f;

        yield return new WaitForSeconds(.5f);
        rb.velocity = new Vector2(0, 0);
        yield return new WaitForSeconds(1f);
        GameManager.instance.GameEnded();
    }


    #region Knockback
    private IEnumerator Invincibility()
    {
        Color originalColor = sr.color;
        Color darkenColor = new Color(sr.color.r, sr.color.g, sr.color.b, .5f);

        canBeKnocked = false;
        sr.color = darkenColor;
        yield return new WaitForSeconds(.1f);

        sr.color = originalColor;
        yield return new WaitForSeconds(.1f);

        sr.color = darkenColor;
        yield return new WaitForSeconds(.15f);

        sr.color = originalColor;
        yield return new WaitForSeconds(.15f);

        sr.color = darkenColor;
        yield return new WaitForSeconds(.25f);

        sr.color = originalColor;
        yield return new WaitForSeconds(.25f);

        sr.color = darkenColor;
        yield return new WaitForSeconds(.3f);

        sr.color = originalColor;
        yield return new WaitForSeconds(.35f);

        sr.color = darkenColor;
        yield return new WaitForSeconds(.4f);

        sr.color = originalColor;
        canBeKnocked = true;
    }

    private void Knockback()
    {
        if (!canBeKnocked)
            return;

        StartCoroutine(Invincibility());
        isKnocked = true;
        AudioManager.instance.PlaySFX(5);
        rb.velocity = knockbackDir;
    }

    private void CancelKnockback() => isKnocked = false;

    #endregion

    #region SpeedControll
    private void SpeedReset()
    {
        if(isSliding)
            return;

        moveSpeed = defaultSpeed;
        milestoneIncreaser = defaultMilestoneIncrease;
    }

    private void SpeedController()
    {
        if (moveSpeed == maxSpeed)
            return;


        if (transform.position.x > speedMilestone)
        {
            speedMilestone = speedMilestone + milestoneIncreaser;

            moveSpeed = moveSpeed * speedMultiplier;
            milestoneIncreaser = milestoneIncreaser * speedMultiplier;

            if (moveSpeed > maxSpeed)
                moveSpeed = maxSpeed;
        }
    }
    #endregion

    #region Ledge Climb Region

    private void CheckForLedge()
    {
        if (ledgeDetected && canGrabLedge)
        {
            canGrabLedge = false;
            rb.gravityScale = 0;

            Vector2 ledgePosition = GetComponentInChildren<LedgeDetection>().transform.position;

            climbBegunPosition = ledgePosition + offset1;
            climbOverPosition = ledgePosition + offset2;

            canClimb = true;
        }

        if (canClimb)
            transform.position = climbBegunPosition;
    }

    private void LedgeClimbOver()
    {

        canClimb = false;
        rb.gravityScale = 5;
        transform.position = climbOverPosition;
        Invoke("AllowLedgeGrab", .1f);
    }

    private void AllowLedgeGrab() => canGrabLedge = true;


    #endregion

    private void CheckForSlideCancel()
    {
        if (slideTimeCounter < 0 && !ceilingDetected)
            isSliding = false;
    }
    private void SetupMovement()
    {

        if (wallDetected)
        {
            SpeedReset();
            return;
        }

        if (isSliding)
            rb.velocity = new Vector2(slideSpeed, rb.velocity.y);
        else
            rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
    }


    #region Inputs
    private void SlideButton()
    {
        if(isDead)
            return;

        if (rb.velocity.x != 0 && slideCooldownCounter < 0)
        {
            dustFX.Play();
            isSliding = true;
            slideTimeCounter = slideTime;
            slideCooldownCounter = slideCooldown;
        }
    }

    private void JumpButton()
    {
        if (isSliding || isDead)
            return;

    RollAnimFinished();

        if (isGrounded)
        {
            dustFX.Play();
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            AudioManager.instance.PlaySFX(1);
            //Jump(jumpForce);
        }
        else if (canDoubleJump)
        {
            canDoubleJump = false;
            //Jump(doubleJumpForce);
            rb.velocity = new Vector2(rb.velocity.x, doubleJumpForce);
            AudioManager.instance.PlaySFX(2);
        }
    }

    //private void Jump(float force)
    //{
        //rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        //AudioManager.instance.PlaySFX(1);
    //}
    private void CheckInput()
    {
        //if (Input.GetButtonDown("Jump"))
        //   playerUnlocked = true;

        if (Input.GetButtonDown("Jump"))
            JumpButton();

        if (Input.GetKeyDown(KeyCode.LeftShift))
            SlideButton();
    }
    #endregion
    #region Animations
    private void AnimatorControllers()
    {
        anim.SetFloat("xVelocity", rb.velocity.x);
        anim.SetFloat("yVelocity", rb.velocity.y);

        anim.SetBool("canDoubleJump", canDoubleJump);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isSliding", isSliding);
        anim.SetBool("canClimb", canClimb);
        anim.SetBool("isKnocked", isKnocked);

        if (rb.velocity.y < -20)
            anim.SetBool("canRoll", true);

    }

    private void RollAnimFinished() => anim.SetBool("canRoll", false);

    #endregion

    private void CheckCollision()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
        ceilingDetected = Physics2D.Raycast(transform.position, Vector2.up, ceilingCheckDistance, whatIsGround);
        wallDetected = Physics2D.BoxCast(wallCheck.position, wallCheckSize, 0, Vector2.zero, 0, whatIsGround);
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheckDistance));
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y + ceilingCheckDistance));
        Gizmos.DrawWireCube(wallCheck.position, wallCheckSize);
    }
}