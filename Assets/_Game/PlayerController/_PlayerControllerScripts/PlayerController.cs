using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("NOTE SHEET")] 
    [SerializeField] private BubbleRow bubbleRow;
    [SerializeField] private Sprite[] noteSprites;

    [Header("NOTE SEQUENCE")] 
    [SerializeField] private NoteSequenceData sequenceData;
    [SerializeField] private PlayerController otherPlayer;
    [SerializeField] private float sequenceTimeout = 2f;
    [SerializeField] private float effectCooldown = 5f;
    
    private bool isGrounded;
    private LayerMask groundLayer;

    private bool isMusicMode;
    
    private List<int> currentSequence = new List<int>();
    private float lastNoteTime;
    private bool isOnEffectCooldown;
    private float effectCooldownEndTime;
    private bool isSequenceLocked;

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
                otherPlayer.RecieveEffect(seq.sequenceName);
                break;
            }
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
    
    private void UpdateEffectCooldown()
    {
        if (!isOnEffectCooldown) return;

        if (Time.time >= effectCooldownEndTime)
        {
            isOnEffectCooldown = false;
            Debug.Log($"{playerID} effect cooldown ended.");
        }
    }
    
    public void RecieveEffect(string effectName)
    {
        if (isOnEffectCooldown)
        {
            Debug.Log("On cooldown");
            return;
        }
        
        Debug.Log($"{playerID} received effect: {effectName}");
        StartEffectCooldown();
    }

    private void StartEffectCooldown()
    {
        isOnEffectCooldown = true;
        effectCooldownEndTime = Time.time + effectCooldown;
    }
    
    #endregion
    
}

public enum PlayerID
{
    Player1,
    Player2
}
