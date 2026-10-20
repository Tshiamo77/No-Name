using UnityEngine;

[CreateAssetMenu(fileName = "NewMemoryData", menuName = "HorrorGame/Memory Data")]
public class MemoryData : ScriptableObject
{
    [Header("Memory Animation Frames")]
    [Tooltip("Assign your 8 sprite frames here")]
    public Sprite[] memoryFrames;
    public float frameRate = 0.15f; // Speed of loop animation

    [Header("Trigger Timing Settings")]
    public float delayBeforeAnimationStarts = 2.0f;
    [Tooltip("Frame index (0-based) where shake, blur, and drop occur. Frame 6 is index 5.")]
    public int triggerFrameIndex = 5;

    [Header("Effects & Dialogue")]
    public float shakeIntensity = 0.3f;
    public float shakeDuration = 1.0f;
    public string subtitleText = "Was that me and my family?";
    public float subtitleDisplayTime = 3.0f;
}