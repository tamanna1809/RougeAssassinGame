using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Ammo")]
    public int ammo = 0;
    public int shotgunAmmo = 0;

    private bool isDead;

    public bool IsDead => isDead;

    public void AddAmmo(int amount)
    {
        ammo += amount;
    }

    public void AddShotgunAmmo(int amount)
    {
        shotgunAmmo += amount;
    }

    public bool UseAmmo()
    {
        if (ammo <= 0) return false;
        ammo--;
        return true;
    }

    public bool UseShotgunAmmo()
    {
        if (shotgunAmmo <= 0) return false;
        shotgunAmmo--;
        return true;
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        DeathEffect.SpawnAt(transform.position, new Color(0f, 1f, 0.53f));
        if (ScreenShake.Instance != null)
            ScreenShake.Instance.Shake(0.3f, 0.3f);

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null) controller.enabled = false;
        PlayerCombat combat = GetComponent<PlayerCombat>();
        if (combat != null) combat.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.velocity = Vector2.zero;

        Invoke(nameof(TriggerDeath), 0.5f);
    }

    public void Respawn()
    {
        isDead = false;
        ammo = 0;
        shotgunAmmo = 0;
    }

    void TriggerDeath()
    {
        GameManager.Instance?.OnPlayerDeath();
    }
}
