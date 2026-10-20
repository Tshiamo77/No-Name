using UnityEngine;

[CreateAssetMenu(fileName = "NewMemoryData", menuName = "Memory System/Memory Data")]
public class MemoryData : ScriptableObject
{
    [Header("UI Overlay Settings")]
    [Tooltip("The animation clip to play for this memory.")]
    public AnimationClip memoryAnimation;

    [Header("Timing Settings")]
    [Tooltip("Duration of the animation in seconds.")]
    public float animationDuration = 8.0f;

    [Tooltip("Duration of the camera spin effect after animation completes.")]
    public float spinDuration = 2.0f;

    [Tooltip("Speed of the camera spin rotation.")]
    public float spinSpeed = 180.0f;

    [Header("Subtitle Lines")]
    [Tooltip("First text displayed after screen spin.")]
    public string primarySubtitle = "Was that me and my family?";

    [Tooltip("Second text displayed after the primary text.")]
    public string secondarySubtitle = "There should be more clues around here...";

    [Tooltip("Duration each subtitle line stays on screen.")]
    public float subtitleDisplayTime = 3.0f;
}