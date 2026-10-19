using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class OpeningThought : MonoBehaviour
{
    [Header("Internal Thought Audio")]
    [SerializeField] private AudioClip wakingUpThought;
    [SerializeField] private AudioClip whoAmIThought;
    [SerializeField] private AudioClip whereAmIThought;
    [SerializeField] private AudioClip findCluesThought;

    [Header("Timing")]
    [SerializeField] private float initialDelay = 3f;
    [SerializeField] private float pauseAfterWakingUp = 4f;
    [SerializeField] private float pauseAfterWhoAmI = 3f;
    [SerializeField] private float pauseAfterWhereAmI = 5f;

    [Header("Audio")]
    [SerializeField, Range(0f, 1f)]
    private float voiceVolume = 1f;

    private AudioSource audioSource;
    private bool sequenceStarted;

    private IEnumerator Start()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = voiceVolume;
        audioSource.Stop();

        if (wakingUpThought == null ||
            whoAmIThought == null ||
            whereAmIThought == null ||
            findCluesThought == null)
        {
            Debug.LogWarning(
                "OpeningThought: Assign all four voice recordings.",
                this
            );
            yield break;
        }

        if (sequenceStarted)
            yield break;

        sequenceStarted = true;

        // Let the player wake up before speaking.
        yield return new WaitForSeconds(initialDelay);

        yield return PlayThought(wakingUpThought);
        yield return new WaitForSeconds(pauseAfterWakingUp);

        yield return PlayThought(whoAmIThought);
        yield return new WaitForSeconds(pauseAfterWhoAmI);

        yield return PlayThought(whereAmIThought);
        yield return new WaitForSeconds(pauseAfterWhereAmI);

        yield return PlayThought(findCluesThought);
    }

    private IEnumerator PlayThought(AudioClip thought)
    {
        if (thought == null)
            yield break;

        audioSource.PlayOneShot(thought, voiceVolume);

        // Wait for the current recording to finish.
        yield return new WaitForSeconds(thought.length);
    }
}