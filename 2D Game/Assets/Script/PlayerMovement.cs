using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private BoxCollider2D coll;
    private SpriteRenderer sprite;
    private Animator anim;

    Vector2 movement;
    Vector2 lastMoveDirection = Vector2.right;
    [SerializeField] private float moveSpeed = 7f;

    [SerializeField] private AudioSource jumpSoundEffect;

    // Add a boolean variable to track the melee attack state
    private bool isAttacking = false;

    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    private void Update()
    {
        // Check for melee attack input (left mouse button)
        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            StartCoroutine(PlayMeleeAttackAnimation());
        }

        if (!isAttacking)
        {
            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");

            // Normalize the movement vector
            if (movement != Vector2.zero)
            {
                movement.Normalize();
            }
        }
        else
        {
            movement = Vector2.zero;
        }

        // Calculate the X difference between the player and the mouse position
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float xDifference = mousePosition.x - transform.position.x;

        // Set the Horizontal animator parameter based on the X difference
        anim.SetFloat("Horizontal", Mathf.Sign(xDifference));

        anim.SetFloat("Vertical", movement.y);
        anim.SetFloat("Speed", movement.sqrMagnitude);

        // Update lastMoveDirection based on the X difference
        lastMoveDirection = new Vector2(Mathf.Sign(xDifference), 0);
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        anim.SetFloat("LastMoveX", lastMoveDirection.x);
        anim.SetFloat("LastMoveY", lastMoveDirection.y);
    }

    private IEnumerator PlayMeleeAttackAnimation()
    {
        isAttacking = true;
        rb.bodyType = RigidbodyType2D.Static;
        anim.SetTrigger("Melee");

        // Wait for the length of the attack animation
        float animationLength = anim.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animationLength);

        rb.bodyType = RigidbodyType2D.Dynamic;
        isAttacking = false;
    }
}
