using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerID
{
    Player1,
    Player2
}

public class PlayerController : MonoBehaviour
{
    [Header("PLAYER")]
    [SerializeField] public PlayerID playerID;

    [Header("MOVEMENT SETTINGS")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.6f, 0.1f);

    [Header("REFERENCES")] 
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform spriteRoot;
    [SerializeField] private Transform animatorRoot;
    [SerializeField] private Transform particleRoot;
    [SerializeField] private Transform particleRootBottom;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("NOTE SHEET")] 
    [SerializeField] private BubbleRow bubbleRow;
    [SerializeField] private Sprite[] noteSprites;

    [Header("NOTE SEQUENCE")] 
    [SerializeField] private NoteSequenceData sequenceData;
    [SerializeField] private PlayerController otherPlayer;
    [SerializeField] private float sequenceTimeout = 2f;
    [SerializeField] private GameObject sparklePS;
    [SerializeField] private GameObject sweatPS;
    [SerializeField] private GameObject angryPS;
    [SerializeField] private GameObject cooldownPS;
    [SerializeField] private GameObject effect0PS;

    private bool isInputEnabled = true;
    
    private bool isGrounded;
    private LayerMask groundLayer;

    private bool isMusicMode;
    
    private List<int> currentSequence = new List<int>();
    private float lastNoteTime;
    private bool isOnGoodEffectCooldown;
    private float goodEffectCooldownEndTime;
    private bool isOnBadEffectCooldown;
    private float badEffectCooldownEndTime;
    private bool isSequenceLocked;

    private bool canDoubleJump;
    private bool hasUsedDoubleJump;
    public event System.Action<bool> OnBoxStateChanged;
    private bool _isBox;
    
    private bool isInvertControls;
    private Material spriteMaterial;

    private void OnEnable()
    {
        groundLayer = LayerMask.GetMask("Ground");
        spriteMaterial = spriteRenderer.material;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
    
    private void Update()
    {
        HandleMovement();
        HandleJump();
        HandleModeToggle();
        HandleMusicActions();
        UpdateGoodEffectCooldown();
        UpdateBadEffectCooldown();
        CheckSequenceTimeout();
    }

    private void FixedUpdate()
    {
        CheckGrounded();
    }
    
    #region Movement
    
    private void HandleMovement()
    {
        if (!isInputEnabled) return;
        
        if (isMusicMode)
        {
            animator.SetBool("IsMoving", false);
            return;
        }
        
        float move = 0f;

        if (playerID == PlayerID.Player1)
        {
            if (Input.GetKey(KeyCode.A)) move = -1f;
            if (Input.GetKey(KeyCode.D)) move =  1f;
        }
        else 
        {
            if (Input.GetKey(KeyCode.LeftArrow))  move = -1f;
            if (Input.GetKey(KeyCode.RightArrow)) move =  1f;
        }
        
        if (isInvertControls)
        {
            move *= -1f;
        }
        
        rb.linearVelocity = new Vector2(move * moveSpeed, rb.linearVelocity.y);
        
        animator.SetBool("IsMoving", Mathf.Abs(move) > 0.01f && isGrounded);
        
        if (move != 0) spriteRoot.localScale = new Vector3(Mathf.Sign(-move), 1f, 1f);
    }

    private void HandleJump()
    {
        if (!isInputEnabled) return;
        if (isMusicMode) return;

        bool jumpPressed =
            (playerID == PlayerID.Player1 && Input.GetKeyDown(KeyCode.W)) ||
            (playerID == PlayerID.Player2 && Input.GetKeyDown(KeyCode.UpArrow));

        if (!jumpPressed)
            return;
        
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            return;
        }
        
        if (canDoubleJump && !hasUsedDoubleJump)
        {
            AudioManager.PlaySound(SoundType.DoubleJump);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            StartCoroutine(DoubleJumpAnimation());
            hasUsedDoubleJump = true;
        }
    }

    private void CheckGrounded()
    {
        bool wasGrounded = isGrounded;        
        
        isGrounded = Physics2D.OverlapBox(
            groundCheck.position,
            groundCheckSize,
            0f,
            groundLayer
        );
        
        if (isGrounded && !wasGrounded)
        {
            hasUsedDoubleJump = false;
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
    }

    public void ToggleControls(bool toggle)
    {
        isInputEnabled = toggle;
    }
    
    #endregion
    
    #region Music Mode
    
    private void HandleModeToggle()
    {
        if (!isInputEnabled) return;
        
        bool togglePressed =
            (playerID == PlayerID.Player1 && Unlocks.IsMusicModeUnlockedP1() && Input.GetKeyDown(KeyCode.LeftShift)) ||
            (playerID == PlayerID.Player2 && Unlocks.IsMusicModeUnlockedP2() && Input.GetKeyDown(KeyCode.RightShift));

        if (togglePressed)
        {
            AudioManager.PlaySound(SoundType.ToggleMode);
            isMusicMode = !isMusicMode;
            animator.SetBool("MusicMode", isMusicMode);
        }
    }
    
    private void HandleMusicActions()
    {
        if (!isInputEnabled) return;
        if (!isMusicMode) return;
        
        if (playerID == PlayerID.Player1)
        {
            PlayNote(KeyCode.W, "Note0", SoundType.P1Note0, 0);
            PlayNote(KeyCode.A, "Note1", SoundType.P1Note1, 1);
            PlayNote(KeyCode.S, "Note2", SoundType.P1Note2, 2);
            PlayNote(KeyCode.D, "Note3", SoundType.P1Note3, 3);
        }
        else
        {
            PlayNote(KeyCode.UpArrow, "Note0", SoundType.P2Note0, 0);
            PlayNote(KeyCode.LeftArrow, "Note1", SoundType.P2Note1, 1);
            PlayNote(KeyCode.DownArrow, "Note2", SoundType.P2Note2, 2);
            PlayNote(KeyCode.RightArrow, "Note3", SoundType.P2Note3,3);
        }
    }

    private void PlayNote(KeyCode key, string anim, SoundType sound, int noteIndex)
    {
        if (!Input.GetKeyDown(key)) return;
        
        animator.Play(anim);
        AudioManager.PlaySound(sound);
        
        TrackSequence(noteIndex);
    }

    #endregion

    #region SequenceTracking
    
    private void TrackSequence(int noteIndex)
    {
        if (isSequenceLocked) return;

        lastNoteTime = Time.time;

        currentSequence.Add(noteIndex);
        bubbleRow.AddBubble(noteSprites[noteIndex]);

        if (currentSequence.Count < 4) return;

        bool matched = false;

        foreach (var seq in sequenceData.sequences)
        {
            if (!IsMatch(seq.notes))
                continue;

            if (!Unlocks.IsEffectUnlocked(seq.unlockKey))
            {
                continue;
            }

            matched = true;
            SpawnReactionPS(sparklePS, particleRoot);
            otherPlayer.TriggerEffect(seq.sequenceName);
            break;
        }
        
        if (!matched)
        {
            otherPlayer.TriggerBadEffect();
            SpawnReactionPS(sweatPS, particleRoot);
        }

        isSequenceLocked = true;
        bubbleRow.Resolve(matched, () =>
        {
            isSequenceLocked = false;
            currentSequence.Clear();
        });
    }
    
    private bool IsMatch(int[] target)
    {
        for (int i = 0; i < 4; i++)
        {
            if (currentSequence[i] != target[i])
                return false;
        }
        return true;
    }
    
    private void CheckSequenceTimeout()
    {
        if (currentSequence.Count == 0)
            return;

        if (Time.time - lastNoteTime > sequenceTimeout)
        {
            currentSequence.Clear();
            bubbleRow.ClearImmediate();
        }
    }
    
    private void SpawnReactionPS(GameObject ps, Transform particleSpawn, float duration = 2.5f)
    {
        GameObject fx = Instantiate(ps, particleSpawn);
        
        Vector3 localPos = Vector3.zero;
        
        if (isMusicMode)
        {
            localPos.y += 0.45f;
        }

        fx.transform.localPosition = localPos;
        
        Destroy(fx, duration);
    }
    
    #endregion
    
    #region Good Effects
    
    private void UpdateGoodEffectCooldown()
    {
        if (!isOnGoodEffectCooldown) return;

        if (Time.time >= goodEffectCooldownEndTime)
        {
            isOnGoodEffectCooldown = false;
            SpawnReactionPS(cooldownPS, particleRoot);
        }
    }
    
    private void StartGoodEffectCooldown(float effectCooldown)
    {
        isOnGoodEffectCooldown = true;
        goodEffectCooldownEndTime = Time.time + effectCooldown;
    }
    
    private void TriggerEffect(string effectName)
    {
        if (isOnGoodEffectCooldown) return;
        
        SpawnReactionPS(sparklePS, particleRoot);

        if (playerID == PlayerID.Player1)
        {
            switch (effectName)
            {
                case "Jump":
                    StartCoroutine(DoubleJumpRoutine());
                    break;
                case "Box":
                    StartCoroutine(BoxRoutine());
                    break;
            }
        }
        else
        {
            switch (effectName)
            {
                case "Flatten":
                {
                    StartCoroutine(FlattenRoutine());
                    break;
                }
                case "Gravity":
                {
                    StartCoroutine(GravityRoutine());
                    break;
                }
            }
        }
        
    }

    private IEnumerator DoubleJumpRoutine()
    {
        canDoubleJump = true;
        hasUsedDoubleJump = false;
        StartGoodEffectCooldown(6f);
        SpawnReactionPS(effect0PS, particleRootBottom, 6f);
        
        yield return new WaitForSeconds(5.95f);
        
        canDoubleJump = false;
    }

    private IEnumerator DoubleJumpAnimation()
    {
        float elapsed = 0f;
        float duration = .5f;
        float startRotation = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            float rotation = Mathf.Lerp(0f, -360f, t);
            animatorRoot.localEulerAngles = new Vector3(0f, 0f, startRotation + rotation);

            yield return null;
        }
        
        animatorRoot.localEulerAngles = new Vector3(0f, 0f, startRotation);
    }

    private IEnumerator FlattenRoutine()
    {
        StartGoodEffectCooldown(6f);
        AudioManager.PlaySound(SoundType.Flatten);
        SpawnReactionPS(effect0PS, particleRootBottom, 6f);
        transform.rotation = Quaternion.Euler(-75f, 0f, 0f);

        yield return new WaitForSeconds(5.95f);
        
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }

    private IEnumerator BoxRoutine()
    {
        StartGoodEffectCooldown(6f);
        AudioManager.PlaySound(SoundType.Boxed);
        SpawnReactionPS(effect0PS, particleRootBottom, 6f);
        ToggleControls(false);
        animator.Play("P1Boxed");

        isBox = true;

        yield return new WaitForSeconds(5.95f);

        animator.Play("Idle");
        isBox = false;
        ToggleControls(true);
    }
    
    public bool isBox
    {
        get => _isBox;
        private set
        {
            if (_isBox == value) return;

            _isBox = value;
            OnBoxStateChanged?.Invoke(_isBox);
        }
    }

    private IEnumerator GravityRoutine()
    {
        AudioManager.PlaySound(SoundType.Gravity);
        StartGoodEffectCooldown(6f);
        SpawnReactionPS(effect0PS, particleRootBottom, 6f);
        rb.gravityScale = -3;
        
        Vector3 scale = transform.localScale;
        transform.localScale = new Vector3(scale.x, -Mathf.Abs(scale.y), scale.z);
        
        yield return new WaitForSeconds(5.95f);
        
        scale = transform.localScale;
        transform.localScale = new Vector3(scale.x, Mathf.Abs(scale.y), scale.z);
        rb.gravityScale = 5;
    }
    
    #endregion
    
    #region Bad Effects

    private void UpdateBadEffectCooldown()
    {
        if (!isOnBadEffectCooldown) return;

        if (Time.time >= badEffectCooldownEndTime)
        {
            isOnBadEffectCooldown = false;
            SpawnReactionPS(cooldownPS, particleRoot);
        }
    }
    
    private void StartBadEffectCooldown(float effectCooldown)
    {
        isOnBadEffectCooldown = true;
        badEffectCooldownEndTime = Time.time + effectCooldown;
    }
    
    private void TriggerBadEffect()
    {
        if (isOnBadEffectCooldown) return;
        
        System.Action[] effects = new System.Action[]
        {
            () => StartCoroutine(BackflipRoutine()),
            () => StartCoroutine(InvertControlsRoutine()),
            () => StartCoroutine(MaterialEffectRoutine())
        };
        
        int index = Random.Range(0, effects.Length);
        
        SpawnReactionPS(angryPS, particleRoot);

        if (playerID == PlayerID.Player1)
        {
            AudioManager.PlaySound(SoundType.P1BadEffect);
        }
        else
        {
            AudioManager.PlaySound(SoundType.P2BadEffect);
        }
        
        effects[index].Invoke();
    }

    private IEnumerator InvertControlsRoutine()
    {
        isInvertControls = true;
        StartCoroutine(FlipRoutine());
        StartBadEffectCooldown(5.05f);
        
        yield return new WaitForSeconds(5f);
        
        isInvertControls = false;
    }
    
    private IEnumerator FlipRoutine()
    {
        float elapsed = 0f;
        float duration = .5f;
        float startRotation = animatorRoot.localEulerAngles.y;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            float rotation = Mathf.Lerp(0f, 360f, t);
            animatorRoot.localEulerAngles = new Vector3(0f, startRotation + rotation, 0f);

            yield return null;
        }
        
        animatorRoot.localEulerAngles = new Vector3(0f, startRotation, 0f);
    }

    private IEnumerator BackflipRoutine()
    {
        StartBadEffectCooldown(1f);

        float elapsed = 0f;
        float duration = .75f;
        float startRotation = 0;
        Vector3 startPosition = animatorRoot.localPosition;
        float jumpHeight = 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            float rotation = Mathf.Lerp(0f, 360f, t);
            animatorRoot.localEulerAngles = new Vector3(0f, 0f, startRotation + rotation);
            
            float yOffset = 4f * jumpHeight * t * (1f - t);
            animatorRoot.localPosition = new Vector3(startPosition.x, startPosition.y + yOffset, startPosition.z);

            yield return null;
        }
        
        animatorRoot.localEulerAngles = new Vector3(0f, 0f, startRotation);
    }

    private IEnumerator MaterialEffectRoutine()
    {
        StartBadEffectCooldown(1f);
        
        string[] effects = { "_Emission", "_HueShift", "_Pixelate" };
        
        int randomIndex = Random.Range(0, effects.Length);
        string chosenEffect = effects[randomIndex];
        
        spriteMaterial.SetInt(chosenEffect, 1);

        yield return new WaitForSeconds(4f);
        
        foreach (string effect in effects)
        {
            spriteMaterial.SetInt(effect, 0);
        }
    }
    
    #endregion
    
}


