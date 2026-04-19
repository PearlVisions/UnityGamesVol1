using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // ===== Liikkuminen =====

    [Header("Movement")]
    [SerializeField] float _movementSpeed = 3f;          // Hahmon normaali liikkumisnopeus

    // ===== Viittaukset muihin komponentteihin =====

    [Header("References")]
    [SerializeField] SpriteRenderer _characterBody;      // Sprite, jota voidaan k‰‰nt‰‰ vasen/oikea
    [SerializeField] Animator _animator;                 // Pelaajan animaattori
    [SerializeField] AudioClip _footstep;                // Askel‰‰ni
    [SerializeField] Player_Combat player_Combat;        // Pelaajan hyˆkk‰yskomponentti
    [SerializeField] Transform _playerVisuals;           // Child-objekti, jota nostetaan visuaalisesti hypyn aikana

    // ===== Hyppy =====

    [Header("Jump")]
    [SerializeField] float _jumpDistance = 2.5f;         // Kuinka pitk‰lle hahmo hypp‰‰ maatasossa
    [SerializeField] float _jumpDuration = 0.4f;         // Kuinka kauan hyppy kest‰‰
    [SerializeField] float _jumpVisualHeight = 0.9f;     // Kuinka korkealle sprite nousee visuaalisesti
    [SerializeField] string _jumpAnimationTrigger = "Jump"; // Animator Trigger -parametrin nimi

    // ===== Esteiden tarkistus =====

    [Header("Obstacle Check")]
    [SerializeField] LayerMask _obstacleLayer;           // Layerit, joita pidet‰‰n estein‰ (esim. Obstacle, Water)
    [SerializeField] float _landingCheckRadius = 0.35f;  // Kohdepisteen tarkistusalueen koko

    // ===== Layer-vaihto hypyn ajaksi =====
    // Normaalisti Player tˆrm‰‰ esteisiin.
    // Hypyn aikana Jump-layer ei tˆrm‰‰ esteisiin, jotta hahmo voi hyp‰t‰ niiden yli.

    [Header("Layers")]
    [SerializeField] string _normalLayerName = "Player"; // Pelaajan normaali layer
    [SerializeField] string _jumpLayerName = "Jump";     // Pelaajan hyppylayer

    // ===== Sis‰iset muuttujat =====

    float _nextFootstepAudio = 0f;                       // Seuraavan askel‰‰nen ajankohta
    Rigidbody2D _rb;                                     // Pelaajan Rigidbody2D

    bool _isJumping = false;                             // Onko hahmo parhaillaan hypp‰‰m‰ss‰
    float _jumpTimer = 0f;                               // Hypyn kulunut aika
    Vector2 _jumpStartPosition;                          // Hypyn l‰htˆpiste
    Vector2 _jumpTargetPosition;                         // Hypyn kohdepiste
    Vector2 _lastMoveDirection = Vector2.down;           // Viimeisin liikesuunta, johon hyp‰t‰‰n

    Vector3 _visualStartLocalPosition;                   // PlayerVisuals-objektin alkuper‰inen localPosition

    int _normalLayer;                                    // Normaalin layerin numero
    int _jumpLayer;                                      // Jump-layerin numero

    void Start()
    {
        // Haetaan Rigidbody2D samalta objektilta
        _rb = GetComponent<Rigidbody2D>();

        // Top-down-peliss‰ ei k‰ytet‰ painovoimaa
        _rb.gravityScale = 0f;

        // Tallennetaan visuaaliobjektin alkuper‰inen sijainti,
        // jotta se voidaan palauttaa hypyn lopussa
        _visualStartLocalPosition = _playerVisuals.localPosition;

        // Haetaan layerien numerot niiden nimien perusteella
        _normalLayer = LayerMask.NameToLayer(_normalLayerName);
        _jumpLayer = LayerMask.NameToLayer(_jumpLayerName);
    }

    void Update()
    {
        // Jos hahmo on hypyss‰, hoidetaan vain hypyn p‰ivitys
        // eik‰ sallita normaalia liikkumista samaan aikaan
        if (_isJumping)
        {
            HandleJump();
            return;
        }

        // Normaali liikkuminen
        HandlePlayerMovement();

        // Hyˆkk‰ys
        if (Input.GetButtonDown("Fire1"))
        {
            // Huom: k‰yt‰n samaa metodia kuin sinun alkuper‰isess‰ koodissa
            player_Combat.Attact();
        }

        // Hyppy v‰lilyˆnnill‰
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartJump();
        }
    }

    void HandleWalkingSounds()
    {
        // Soitetaan askel‰‰ni vain tietyin v‰liajoin
        if (Time.time >= _nextFootstepAudio)
        {
            AudioManager.Instance.PlayAudio(_footstep, AudioManager.SoundType.SFX, 1f, false);

            // Arvioidaan askel‰‰nen v‰li animaation pituuden perusteella
            float audioFrequency = _animator.GetCurrentAnimatorClipInfo(0)[0].clip.length / 2f;
            _nextFootstepAudio = Time.time + audioFrequency;
        }
    }

    void HandlePlayerMovement()
    {
        // Luetaan pelaajan liikesyˆtteet
        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");

        Vector2 movement = new Vector2(moveHorizontal, moveVertical);

        // Est‰‰ diagonaaliliikkeen muuttumisen liian nopeaksi
        movement = Vector2.ClampMagnitude(movement, 1f);

        // Asetetaan rigidbodyn nopeus liikesuunnan mukaan
        _rb.linearVelocity = movement * _movementSpeed;

        // Tarkistetaan liikkuuko hahmo
        bool characterIsWalking = movement.magnitude > 0.1f;
        _animator.SetBool("IsWalking", characterIsWalking);

        // Soitetaan askel‰‰net vain liikkuessa
        if (characterIsWalking)
        {
            HandleWalkingSounds();

            // Tallennetaan viimeisin liikesuunta, jotta siihen voidaan hyp‰t‰
            _lastMoveDirection = movement.normalized;
        }

        // K‰‰nnet‰‰n sprite vasemmalle jos liikutaan vasemmalle
        bool flipSprite = movement.x < 0f;
        _characterBody.flipX = flipSprite;
    }

    bool CanJumpToPosition(Vector2 targetPosition)
    {
        // Tarkistetaan onko laskeutumispaikassa estett‰.
        // Jos lˆytyy collider obstacle-layereista, hyppy‰ ei sallita.
        Collider2D hit = Physics2D.OverlapCircle(targetPosition, _landingCheckRadius, _obstacleLayer);
        return hit == null;
    }

    void StartJump()
    {
        // M‰‰ritet‰‰n hypyn suunta viimeisimm‰n liikkeen perusteella
        Vector2 jumpDirection = _lastMoveDirection;

        // Jos suuntaa ei ole viel‰ tallennettu, hyp‰t‰‰n oletuksena alasp‰in
        if (jumpDirection.magnitude < 0.1f)
        {
            jumpDirection = Vector2.down;
        }

        // Tallennetaan hypyn alku- ja kohdepiste
        _jumpStartPosition = _rb.position;
        _jumpTargetPosition = _jumpStartPosition + jumpDirection * _jumpDistance;

        // Jos kohdepisteess‰ on este, hyppy‰ ei tehd‰
        if (!CanJumpToPosition(_jumpTargetPosition))
        {
            return;
        }

        // Aloitetaan hyppy
        _isJumping = true;
        _jumpTimer = 0f;

        // Pys‰ytet‰‰n normaali liike hypyn alussa
        _rb.linearVelocity = Vector2.zero;

        // Vaihdetaan layer hypyn ajaksi,
        // jotta hahmo voi kulkea esteiden yli
        gameObject.layer = _jumpLayer;

        // K‰ynnistet‰‰n hyppyanimaatio
        _animator.SetTrigger(_jumpAnimationTrigger);

        // Varmistetaan ettei k‰velyanimaatio j‰‰ p‰‰lle
        _animator.SetBool("IsWalking", false);
    }

    void HandleJump()
    {
        // P‰ivitet‰‰n hypyn ajastin
        _jumpTimer += Time.deltaTime;

        // Normalisoitu aika 0 -> 1
        float t = _jumpTimer / _jumpDuration;
        t = Mathf.Clamp01(t);

        // SmoothStep tekee maatasossa etenemisest‰ sulavamman
        float smoothT = Mathf.SmoothStep(0f, 1f, t);

        // Liikutaan hypyn l‰htˆpisteest‰ kohdepisteeseen
        Vector2 newPosition = Vector2.Lerp(_jumpStartPosition, _jumpTargetPosition, smoothT);
        _rb.MovePosition(newPosition);

        // Tehd‰‰n visuaalinen hyppykaari PlayerVisuals-objektille:
        // alussa 0, keskell‰ korkein kohta, lopussa taas 0
        float jumpArc = 4f * _jumpVisualHeight * t * (1f - t);

        Vector3 visualPos = _visualStartLocalPosition;
        visualPos.y += jumpArc;
        _playerVisuals.localPosition = visualPos;

        // Kun hyppy on valmis
        if (t >= 1f)
        {
            _isJumping = false;

            // Palautetaan visuaali takaisin normaaliasentoon
            _playerVisuals.localPosition = _visualStartLocalPosition;

            // Palautetaan normaali layer takaisin
            gameObject.layer = _normalLayer;
        }
    }

    void OnDrawGizmosSelected()
    {
        // N‰ytt‰‰ Scene-n‰kym‰ss‰ laskeutumispaikan tarkistusympyr‰n,
        // jos peli on k‰ynniss‰
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_jumpTargetPosition, _landingCheckRadius);
    }
}