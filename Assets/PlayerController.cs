using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    SpriteRenderer spriteRenderer;
    Rigidbody2D rb;
    Transform transform;
    Collider2D collider;
    public float playerMoveSpeed = 10f;
    public float playerJumpForce = 10f;

    #region oldParams

   // private bool havePressedJump = false;

    [SerializeField] private string rightDashKey = "d";
    [SerializeField] private string leftDashKey = "a";
    [SerializeField] private string jumpKey = "w";
    [SerializeField] private string blockKey = "s";

    private bool inputWindowOpen = true;

    private bool havePressedRightDash = false;
    private bool havePressedLeftDash = false;

    private bool isBlocking = false;
    private float blockLength = 0.5f;

    private bool isGrounded = true;

    private bool isDashing;

    [SerializeField] private float dashingPower = 128f;

    [SerializeField] float clearDelay = 0.1f;

    private float originalGravity;

    [SerializeField] TrailRenderer trailRenderer;
    [SerializeField] SpriteRenderer iconRenderer;
    #endregion

    private void Awake()
    {
        MusicManager.BeatUpdated += ClearBeatActions;
        isBlocking = false;

    }

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        transform = gameObject.GetComponent<Transform>();
        originalGravity = rb.gravityScale;
        collider = gameObject.GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        Debug.Log(inputWindowOpen);
        // Debug.Log("isGrounded = " + isGrounded);

        if (inputWindowOpen)
        {
            if (Input.GetKeyDown(rightDashKey) && !isDashing && !isBlocking)
            {
                iconRenderer.enabled = true;
                MusicManager.BeatUpdated += DashRight;
                havePressedRightDash = true;
            }

            if (Input.GetKeyDown(leftDashKey) && !isDashing && !isBlocking)
            {
                iconRenderer.enabled = true;
                MusicManager.BeatUpdated += DashLeft;
                havePressedLeftDash = true;
            }
         
            if (Input.GetKeyDown(jumpKey) && isGrounded && !isBlocking)
            {
                iconRenderer.enabled = true;
                MusicManager.BeatUpdated += Jump;
                Debug.Log("JumpKey Pressed");
            }

            if (Input.GetKeyDown(blockKey))
            {
                Debug.Log("BlockKey Pressed");
                iconRenderer.enabled = true;
                MusicManager.BeatUpdated += Block;

            }
     
        }

    }

    private void LateUpdate()
    {
        //MusicManager.BeatUpdated += ClearBeatActions;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }


    private void OnCollisionExit2D(Collision2D collision)
    {        
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    #region beat stuff
    public void ClearBeatActions()
    {

        if (havePressedRightDash)
            Invoke(nameof(ProxyClearRightDash), clearDelay);


        if (havePressedLeftDash)
            Invoke(nameof(ProxyClearLeftDash), clearDelay);

        if (!isGrounded)
            Invoke(nameof(ProxyClearJump), clearDelay);

        if (!isGrounded)
            Invoke(nameof(ProxyClearBlock), clearDelay);
    }

    IEnumerator FlipInputWindow()
    {
        inputWindowOpen = false;
        yield return new WaitForSeconds(clearDelay);
        inputWindowOpen = true;
    }

    private void ProxyClearRightDash()
    {

        MusicManager.BeatUpdated -= DashRight;
        isDashing = false;
        iconRenderer.enabled = false;
        havePressedRightDash = false;
        trailRenderer.emitting = false;

        /*
        if (player1)
            MusicManager.EvenBeatUpdated -= DashRight;

        else
            MusicManager.OddBeatUpdated -= DashRight;
     
        */



    }

    private void ProxyClearLeftDash()
    {
        MusicManager.BeatUpdated -= DashLeft;
        isDashing = false;
        iconRenderer.enabled = false;
        havePressedLeftDash = false;
        rb.gravityScale = originalGravity;
        trailRenderer.emitting = false;
        /*
        if (player1)
            MusicManager.EvenBeatUpdated -= DashLeft;

        else
            MusicManager.OddBeatUpdated -= DashLeft;
        */

    }

    private void ProxyClearJump()
    {
        MusicManager.BeatUpdated -= Jump;
        iconRenderer.enabled = false;
        rb.gravityScale = originalGravity;
        trailRenderer.emitting = false;
    }

    private void ProxyClearBlock()
    {
        MusicManager.BeatUpdated -= Block;
        iconRenderer.enabled = false;
        trailRenderer.emitting = false;
    }

    private void DashRight()
    {
        isDashing = true;
        rb.linearVelocity = new Vector2(transform.localScale.x * dashingPower, 0f);
        trailRenderer.emitting = true;
        iconRenderer.enabled = true;

        if (havePressedRightDash)
            Invoke(nameof(ProxyClearRightDash), clearDelay);
    }

    private void DashLeft()
    {
        isDashing = true;
        rb.linearVelocity = new Vector2((transform.localScale.x * dashingPower) * -1, 0f);
        trailRenderer.emitting = true;

        if (havePressedLeftDash)
            Invoke(nameof(ProxyClearLeftDash), clearDelay);
    }

    private void Jump()
    {
        rb.AddForce(Vector2.up * playerJumpForce, ForceMode2D.Impulse);
        trailRenderer.emitting = true;


        Invoke(nameof(ProxyClearJump), clearDelay);
    }

    private void Block()
    {
        isBlocking = true;
        Debug.Log("Is blocking");
        Invoke(nameof(StopBlock), blockLength);
        Invoke(nameof(ProxyClearBlock), clearDelay);
    }

    private void StopBlock()
    {
        Debug.Log("Stopped blocking");
        isBlocking = false;
    }

    #endregion



}
