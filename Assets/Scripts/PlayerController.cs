 using UnityEngine;

public class PlayerController : MonoBehaviour
{

    // rigidbody variable for speed
    [SerializeField] float _movementSpeed;
    [SerializeField] SpriteRenderer _characterBody;
    [SerializeField] Animator _animator;
    [SerializeField] AudioClip _footstep;

    float _nextFootstepAudio = 0f;
    Rigidbody2D _rb;

    void HandleWalkingSounds()
    {
        if(Time.time >= _nextFootstepAudio)
        {
            AudioManager.Instance.PlayAudio(_footstep, AudioManager.SoundType.SFX, 1f, false);

            float audioFrequency = _animator.GetCurrentAnimatorClipInfo(0)[0].clip.length / 2f;
            _nextFootstepAudio = Time.time + audioFrequency;
        }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();


    }

    // Update is called once per frame
    void Update()
    {
        HandlePlayerMovement();

    }
    private void HandlePlayerMovement()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector2 movement = new Vector2(moveHorizontal, moveVertical);
        movement = Vector2.ClampMagnitude(movement, 1.0f);
        _rb.linearVelocity = movement * _movementSpeed;

        bool characterIsWalking = movement.magnitude > 0.1f;
        _animator.SetBool("IsWalking", characterIsWalking);

        // t‰nne tulee ‰‰net

        if (characterIsWalking ) 
        {
           HandleWalkingSounds();   
        }

        bool flipSprite = movement.x < 0f;
        _characterBody.flipX = flipSprite;

    }
}
