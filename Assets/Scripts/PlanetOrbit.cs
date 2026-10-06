using UnityEngine;

/// <summary>
/// Makes a planet slowly orbit around a fixed center point in world space.
/// Attach to any planet object; set center and parameters from PlanetManager.
/// The orbit is paused while the player is inside the planet's gravity well
/// (isPlayerAtCore) so the planet doesn't drift away mid-interaction.
/// </summary>
public class PlanetOrbit : MonoBehaviour
{
    [Tooltip("World-space point the planet orbits around.")]
    public Vector3 centerPoint = Vector3.zero;

    [Tooltip("Degrees per second the planet orbits.")]
    public float orbitSpeed = 2f;

    [Tooltip("Axis of orbital rotation (normalized).")]
    public Vector3 orbitAxis = Vector3.up;

    // Pause orbit when player is inside the gravity well
    [HideInInspector] public bool isPaused = false;

    void Update()
    {
        if (isPaused) return;

        // RotateAround keeps the planet on its orbit path
        transform.RotateAround(centerPoint, orbitAxis, orbitSpeed * Time.deltaTime);
    }
}
