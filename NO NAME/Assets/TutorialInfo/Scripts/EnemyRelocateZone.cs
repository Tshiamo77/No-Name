using UnityEngine;

/// <summary>
/// Pairs with a PlayerZoneTrigger (on the same object) via its onPlayerEntered event.
/// When the player enters, the enemy relocates to its upstairs or downstairs home and
/// resumes wandering there - so the enemy is always on the opposite floor from the player.
///
/// Put one of these at the top of the stairs with Relocate To Upstairs = FALSE (sends the
/// enemy downstairs when the player goes up), and another at the bottom with
/// Relocate To Upstairs = TRUE (sends the enemy upstairs when the player comes back down).
/// Both reference the SAME Enemy and reuse its own Upstairs Home / Downstairs Home fields,
/// so there's nothing else to configure here.
/// </summary>
public class EnemyRelocateZone : MonoBehaviour
{
    [SerializeField] private Enemy enemy;
    [Tooltip("True: send the enemy to its Upstairs Home. False: send it to its Downstairs Home.")]
    [SerializeField] private bool relocateToUpstairs;

    /// <summary>Wire this to a PlayerZoneTrigger's On Player Entered () event in the Inspector.</summary>
    public void Relocate()
    {
        if (enemy == null) return;

        // Don't interrupt an active catch sequence
        if (enemy.currentState == Enemy.EnemyState.Caught) return;

        if (relocateToUpstairs) enemy.GoUpstairs();
        else enemy.GoDownstairs();
    }
}