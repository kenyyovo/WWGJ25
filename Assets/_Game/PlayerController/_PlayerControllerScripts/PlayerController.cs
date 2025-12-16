using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("REFERENCES")] 
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform spriteRoot;
    [SerializeField] private Transform particleRoot;

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
    
    private bool isGrounded;
    private LayerMask groundLayer;

    private bool isMusicMode;
    
    private List<int> currentSequence = new List<int>();
    private float lastNoteTime;
    private bool isOnEffectCooldown;
    private float effectCooldownEndTime;
    private bool isSequenceLocked;

    private bool isInvertControls;

    private void OnEnable()
    {
        groundLayer = LayerMask.GetMask("Ground");
    }
    
    private void Update()
    {
        HandleMovement();
        HandleJump();
        HandleModeToggle();
        HandleMusicActions();
        UpdateEffectCooldown();
        CheckSequenceTimeout();
    }

    private void FixedUpdate()
    {
        CheckGrounded();
    }
    
    #region Movement
    
    private void HandleMovement()
    {
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
        if (isMusicMode) return;
        if (!isGrounded) return;

        bool jumpPressed =
            (playerID == PlayerID.Player1 && Input.GetKeyDown(KeyCode.W)) ||
            (playerID == PlayerID.Player2 && Input.GetKeyDown(KeyCode.UpArrow));

        if (jumpPressed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    private void CheckGrounded()
    {
        if (groundCheck == null) return;

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            0.15f,
            groundLayer
        );
    }
    
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, 0.15f);
    }
    
    #endregion
    
    #region Music Mode
    
    private void HandleModeToggle()
    {
        bool togglePressed =
            (playerID == PlayerID.Player1 && Input.GetKeyDown(KeyCode.LeftShift)) ||
            (playerID == PlayerID.Player2 && Input.GetKeyDown(KeyCode.RightShift));

        if (togglePressed)
        {
            isMusicMode = !isMusicMode;
            animator.SetBool("MusicMode", isMusicMode);
        }
    }
    
    private void HandleMusicActions()
    {
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

        if (currentSequence.Count < 4)
            return;

        bool matched = false;

        foreach (var seq in sequenceData.sequences)
        {
            if (IsMatch(seq.notes))
            {
                matched = true;
                otherPlayer.TriggerEffect(seq.sequenceName);
                SpawnReactionPS(sparklePS);
                break;
            }
        }

        if (!matched)
        {
            otherPlayer.TriggerBadEffect();
            SpawnReactionPS(sweatPS);
        }
        
        isSequenceLocked = true;
        bubbleRow.Resolve(matched, () => {
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
    
    private void SpawnReactionPS(GameObject gameObject)
    {
        GameObject fx = Instantiate(gameObject, particleRoot);
        
        Vector3 localPos = Vector3.zero;
        
        if (isMusicMode)
        {
            localPos.y += 0.45f;
        }

        fx.transform.localPosition = localPos;
        
        Destroy(fx, 2.5f);
    }
    
    private void UpdateEffectCooldown()
    {
        if (!isOnEffectCooldown) return;

        if (Time.time >= effectCooldownEndTime)
        {
            isOnEffectCooldown = false;
            SpawnReactionPS(cooldownPS);
        }
    }
    #endregion
    
    #region Good Effects
    
    private void TriggerEffect(string effectName)
    {
        if (isOnEffectCooldown) return;
        
        SpawnReactionPS(sparklePS);
        //Give Effect, move start cooldown in the giveeffect functions
        StartEffectCooldown(3f);
    }
    
    #endregion
    
    #region Bad Effects

    private void TriggerBadEffect()
    {
        if (isOnEffectCooldown) return;
        
        System.Action[] effects = new System.Action[]
        {
            () => StartCoroutine(BackflipRoutine()),
            () => StartCoroutine(FlipRoutine()),
            () => StartCoroutine(InvertControlsRoutine())
        };
        
        int index = Random.Range(0, effects.Length);
        
        SpawnReactionPS(angryPS);
        
        effects[index].Invoke();
    }

    private IEnumerator InvertControlsRoutine()
    {
        isInvertControls = true;
        StartEffectCooldown(5.05f);
        
        yield return new WaitForSeconds(5f);
        
        isInvertControls = false;
    }

    private IEnumerator BackflipRoutine()
    {
        StartEffectCooldown(2f);

        float elapsed = 0f;
        float duration = 1f;
        float startRotation = transform.localEulerAngles.z;
        Vector3 startPosition = transform.localPosition;
        float jumpHeight = 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            float rotation = Mathf.Lerp(0f, 360f, t);
            transform.localEulerAngles = new Vector3(0f, 0f, startRotation + rotation);
            
            float yOffset = 4f * jumpHeight * t * (1f - t);
            transform.localPosition = new Vector3(startPosition.x, startPosition.y + yOffset, startPosition.z);

            yield return null;
        }
        
        transform.localEulerAngles = new Vector3(0f, 0f, startRotation);
    }

    private IEnumerator FlipRoutine()
    {
        StartEffectCooldown(2f);
        
        float elapsed = 0f;
        float duration = 1f;
        float startRotation = transform.localEulerAngles.y;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            float rotation = Mathf.Lerp(0f, 360f, t);
            transform.localEulerAngles = new Vector3(0f, startRotation + rotation, 0f);

            yield return null;
        }
        
        transform.localEulerAngles = new Vector3(0f, startRotation, 0f);
    }

    private void StartEffectCooldown(float effectCooldown)
    {
        isOnEffectCooldown = true;
        effectCooldownEndTime = Time.time + effectCooldown;
    }
    
    #endregion
    
}


