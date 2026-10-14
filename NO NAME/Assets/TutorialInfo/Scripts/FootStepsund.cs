using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class FootstepSound : MonoBehaviour
{
    [Header("Footstep Audio")]
    [SerializeField] private AudioClip[] footstepSounds;

    [Header("Footstep Settings")]
    [SerializeField] private float stepInterval = 0.5f;
    [SerializeField] private float volume = 0.25f;

    private AudioSource audioSource;
    private CharacterController characterController;
    private float stepTimer;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        characterController = GetComponent<CharacterController>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = volume;
        audioSource.spatialBlend = 0f;
    }

    private void Update()
    {
        if (Keyboard.current == null)
        {
            audioSource.Stop();
            stepTimer = 0f;
            return;
        }

        bool moving =
            Keyboard.current.wKey.isPressed ||
            Keyboard.current.aKey.isPressed ||
            Keyboard.current.sKey.isPressed ||
            Keyboard.current.dKey.isPressed;

        bool grounded = characterController == null ||
                        characterController.isGrounded;

        if (moving && grounded)
        {
            stepTimer += Time.deltaTime;

            if (stepTimer >= stepInterval)
            {
                PlayFootstep();
                stepTimer = 0f;
            }
        }
        else
        {
            stepTimer = 0f;
            audioSource.Stop();
        }
    }

    private void PlayFootstep()
    {
        if (footstepSounds == null || footstepSounds.Length == 0)
            return;

        AudioClip clip =
            footstepSounds[Random.Range(0, footstepSounds.Length)];

        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
}