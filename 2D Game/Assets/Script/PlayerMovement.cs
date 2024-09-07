using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public HealthBar healthBar;
    public float invulnerabilityDuration = 2.0f; // Set this to your desired invulnerability duration
    private bool isInvulnerable = false;
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
    // Add a boolean variable to track the crouch state
    private bool isCrouching = false;
    private bool isDead = false;


    // Start is called before the first frame update
    private void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (isDead)
        {
            return; // Ignore the rest of the code in Update if the player is dead
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage(20);
        }
        // Check for melee attack input (left mouse button)
        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            StartCoroutine(PlayMeleeAttackAnimation());
        }
        if (Input.GetKeyUp(KeyCode.LeftControl) && !isAttacking)
        {
            if (!isCrouching)
            {
                isCrouching = true;
                rb.bodyType = RigidbodyType2D.Static;
                StartCoroutine(RegenerateHealthOverTime(5, 1.0f));
            }
            else
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                isCrouching = false;
                StopCoroutine(RegenerateHealthOverTime(5, 1.0f));
            }

            anim.SetBool("Crouch", isCrouching);
        }

        if (!isAttacking && !isCrouching)
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
        if (isDead)
        {
            return; // Ignore the rest of the code in FixedUpdate if the player is dead
        }
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

    private IEnumerator RegenerateHealthOverTime(int amount, float delay)
    {
        while (isCrouching)
        {
            if (currentHealth + amount > maxHealth)
            {
                currentHealth = maxHealth;
            }
            else
            {
                currentHealth += amount;
            }

            healthBar.SetHealth(currentHealth);
            yield return new WaitForSeconds(delay);
        }
    }

    void TakeDamage(int damage)
    {
        if (isInvulnerable)
        {
            return; // Ignore the rest of the method if the player is invulnerable
        }
        if (currentHealth - damage <= 0)
        {
            currentHealth = 0;
        }
        else
        {
            currentHealth -= damage;
        }
        healthBar.SetHealth(currentHealth);

        if (currentHealth <= 0 && !isDead)
        {
            isDead = true;
            anim.SetTrigger("Die"); // Make sure to add a Die trigger in your Animator
            rb.bodyType = RigidbodyType2D.Static; // Or any other actions you want to perform on death
        }
        else if (!isDead) // Call hurt animation only if player is not dead
        {
            StartCoroutine(PlayHurtAnimation());
            isInvulnerable = true;
            StartCoroutine(EndInvulnerability());
            StartCoroutine(Blink());
        }
    }


    private IEnumerator PlayHurtAnimation()
    {
        rb.bodyType = RigidbodyType2D.Static;
        anim.SetTrigger("Hurt");

        // Wait for the length of the attack animation
        float animationLength = anim.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animationLength);

        rb.bodyType = RigidbodyType2D.Dynamic;
    }

    private IEnumerator EndInvulnerability()
    {
        yield return new WaitForSeconds(invulnerabilityDuration);
        isInvulnerable = false;
    }

    private IEnumerator Blink()
    {
        while (isInvulnerable)
        {
            sprite.enabled = !sprite.enabled; // Toggle the SpriteRenderer's enabled property
            yield return new WaitForSeconds(0.15f);
        }
        sprite.enabled = true; // Make sure the sprite is enabled after the blinking ends
    }

}
