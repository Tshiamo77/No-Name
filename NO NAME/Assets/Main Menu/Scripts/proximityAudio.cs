
using UnityEngine;

public class ProximityAudio : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private Transform player;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float activationDistance = 5f;

    private void Start()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    private void Update()
    {
        if (player == null || audioSource == null)
            return;

        float distance = Vector3.Distance(
            player.position,
            transform.position
        );

        if (distance <= activationDistance)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        else
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }
}

