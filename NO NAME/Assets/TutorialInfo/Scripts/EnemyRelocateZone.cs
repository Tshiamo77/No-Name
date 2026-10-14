using UnityEngine;


public class EnemyRelocateZone : MonoBehaviour
{
    [SerializeField] private Enemy enemy;
    [SerializeField] private Transform relocatePoint;

    /// <summary>Wire this to a PlayerZoneTrigger's On Player Entered () event in the Inspector.</summary>
    public void Relocate()
    {
        if (enemy == null || relocatePoint == null) return;

        // Don't interrupt an active catch sequence
        if (enemy.currentState == Enemy.EnemyState.Caught) return;

        enemy.GoDormantAt(relocatePoint);
    }
}