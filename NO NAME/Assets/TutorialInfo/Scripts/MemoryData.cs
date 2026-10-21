using UnityEngine;

[CreateAssetMenu(fileName = "NewMemoryData", menuName = "Memory System/Memory Data")]
public class MemoryData : ScriptableObject
{
    [Header("Animation Settings")]
    [Tooltip("The animation clip to play during this memory sequence.")]
    public AnimationClip memoryAnimationClip;

    [Header("Timing Settings")]
    [Tooltip("Duration of the animation display in seconds.")]
    public float memoryDisplayDuration = 8.0f;

    [Tooltip("Duration of the dizzy camera effect after animation completes.")]
    public float dizzyDuration = 3.0f;

    [Tooltip("Intensity of the camera sway/dizzy motion.")]
    public float dizzyIntensity = 3.0f;

    [Header("Subtitle Lines")]
    [Tooltip("Line 1 shown after dizzy effect ends.")]
    public string subtitleLine1 = "Was that me and my family?";

    [Tooltip("Line 2 shown right after Line 1.")]
    public string subtitleLine2 = "I remember 0716. Hmm there has to be more clues around here...";

    [Tooltip("How long each subtitle line remains on screen.")]
    public float subtitleDisplayTime = 3.5f;
}