using UnityEngine;

[DisallowMultipleComponent]
public class SatelliteThrusterController : MonoBehaviour
{
    [Header("Particle systems (assign child systems)")]
    public ParticleSystem coreSystem; // bright, short-lived
    public ParticleSystem glowSystem; // softer, larger
    public ParticleSystem smokeSystem; // optional smoke

    [Header("Light")]
    public Light thrusterLight;

    [Header("Tuning")]
    public float maxCoreEmission = 500f;
    public float maxGlowEmission = 120f;
    public float maxSmokeEmission = 30f;
    public float coreBaseSpeed = 6f;
    public float glowBaseSpeed = 2f;
    public float smokeBaseSpeed = 0.5f;
    public float lightMaxIntensity = 6f;

    [Header("Smoothing")]
    public float responseSpeed = 8f;

    float current = 0f; // smoothed 0..1

    void Start()
    {
        // Ensure modules use unscaled time for UI reliability
        if (coreSystem != null)
        {
            var m = coreSystem.main;
            m.useUnscaledTime = true;
        }
        if (glowSystem != null)
        {
            var m = glowSystem.main;
            m.useUnscaledTime = true;
        }
        if (smokeSystem != null)
        {
            var m = smokeSystem.main;
            m.useUnscaledTime = true;
        }
    }

    /// <summary>
    /// Update visuals. position and rotation should be in world space.
    /// norm is 0..1 normalized thrust level.
    /// </summary>
    public void UpdateVisuals(Vector3 position, Quaternion rotation, float norm)
    {
        norm = Mathf.Clamp01(norm);
        current = Mathf.MoveTowards(current, norm, responseSpeed * Time.deltaTime);

        // position/rotation
        transform.position = position;
        transform.rotation = rotation;

        UpdateParticle(coreSystem, current, maxCoreEmission, coreBaseSpeed);
        UpdateParticle(glowSystem, current, maxGlowEmission, glowBaseSpeed);
        UpdateParticle(smokeSystem, current, maxSmokeEmission, smokeBaseSpeed);

        if (thrusterLight != null)
        {
            thrusterLight.intensity = Mathf.Lerp(0f, lightMaxIntensity, current);
            thrusterLight.enabled = thrusterLight.intensity > 0.01f;
        }
    }

    void UpdateParticle(ParticleSystem ps, float norm, float maxEmission, float baseSpeed)
    {
        if (ps == null) return;

        var main = ps.main;
        var emission = ps.emission;

        emission.rateOverTime = new ParticleSystem.MinMaxCurve(Mathf.Lerp(0f, maxEmission, norm));

        // scale speed/size/lifetime softly
        main.startSpeed = baseSpeed * Mathf.Lerp(0.5f, 1.5f, norm);
        main.startSize = main.startSize.constant * Mathf.Lerp(0.6f, 1.4f, norm);
        main.startLifetime = main.startLifetime.constant * Mathf.Lerp(0.6f, 1.4f, norm);

        if (norm <= 0.001f)
        {
            if (ps.isPlaying) ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
        else
        {
            if (!ps.isPlaying) ps.Play();
        }
    }

    public void ForceStop()
    {
        current = 0f;
        UpdateParticle(coreSystem, 0f, maxCoreEmission, coreBaseSpeed);
        UpdateParticle(glowSystem, 0f, maxGlowEmission, glowBaseSpeed);
        UpdateParticle(smokeSystem, 0f, maxSmokeEmission, smokeBaseSpeed);
        if (thrusterLight != null) thrusterLight.enabled = false;
    }
}
