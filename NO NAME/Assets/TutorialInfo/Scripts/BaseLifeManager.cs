using UnityEngine;

public class BaseLifeManager : MonoBehaviour
{
    [Header("Life Settings")]
    [SerializeField] public int maxLives = 3;
    public int currentLives;

    public virtual void Awake()
    {
        // Initialize lives at start
        currentLives = maxLives;
        Debug.Log($"<color=green>[LIVES] Initialised lives to: {currentLives}</color>");
    }

    // Virtual method allows derived classes to customize what happens when a life is lost
    public virtual void LoseLife()
    {
        currentLives--;
        Debug.Log($"[LIFE SYSTEM] Life lost! Current lives remaining: {currentLives}");

        if (currentLives <= 0)
        {
            OnDeath();
        }
    }

    // Virtual method for death behavior
    public virtual void OnDeath()
    {
        Debug.Log("[LIFE SYSTEM] Entity has run out of lives.");
    }
}