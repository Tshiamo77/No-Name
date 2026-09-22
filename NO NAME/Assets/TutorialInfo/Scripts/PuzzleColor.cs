using UnityEngine;

public enum PuzzleColor
{
    Red,
    Blue,
    Green,
    Pink,
    Yellow
}

public static class PuzzleColorExtensions
{
    public static Color ToColor(this PuzzleColor c)
    {
        switch (c)
        {
            case PuzzleColor.Red: return new Color(0.90f, 0.10f, 0.10f);
            case PuzzleColor.Blue: return new Color(0.15f, 0.35f, 0.95f);
            case PuzzleColor.Green: return new Color(0.10f, 0.75f, 0.20f);
            case PuzzleColor.Yellow: return new Color(1.00f, 0.90f, 0.10f);
            case PuzzleColor.Pink: return new Color(1.00f, 0.75f, 0.80f);
            default: return Color.white;
        }
    }

    /// <summary>Tints every renderer on the object (and its children) with this color.</summary>
    public static void ApplyTo(this PuzzleColor c, GameObject target)
    {
        Color color = c.ToColor();
        foreach (Renderer r in target.GetComponentsInChildren<Renderer>())
        {
            r.material.color = color;
        }
    }
}