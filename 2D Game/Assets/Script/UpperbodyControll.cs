using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpperbodyControll : MonoBehaviour
{
    private Rigidbody2D rb;
    private BoxCollider2D coll;
    private SpriteRenderer sprite;
    private Animator anim;

    Vector2 movement;
    Vector2 lastMoveDirection = Vector2.right;
    [SerializeField] private float moveSpeed = 7f;

    [SerializeField] private AudioSource jumpSoundEffect;

    // Add a reference to the upper body Animator
    [SerializeField] private Animator upperBodyAnim;

    // Add a boolean variable to track combat state
    private bool isInCombat = false;

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
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Normalize the movement vector
        if (movement != Vector2.zero)
        {
            movement.Normalize();
        }

        // Calculate the X difference between the player and the mouse position
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float xDifference = mousePosition.x - transform.position.x;

        // Set the Horizontal animator parameter for both the player and upper body Animator
        anim.SetFloat("Horizontal", Mathf.Sign(xDifference));

        // Only update the upper body animation when in combat
        if (isInCombat)
        {
            upperBodyAnim.SetFloat("Horizontal", Mathf.Sign(xDifference));
        }

        anim.SetFloat("Vertical", movement.y);
        anim.SetFloat("Speed", movement.sqrMagnitude);

        // Update lastMoveDirection based on the X difference
        lastMoveDirection = new Vector2(Mathf.Sign(xDifference), 0);

        // Update combat state based on input (replace "Fire1" with your desired input)
        isInCombat = Input.GetButton("Fire1");
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        anim.SetFloat("LastMoveX", lastMoveDirection.x);
        anim.SetFloat("LastMoveY", lastMoveDirection.y);
    }
}
