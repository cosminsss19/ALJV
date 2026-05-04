using UnityEngine;
using AI;

public class PlayerCollisionHandler : MonoBehaviour
{
    private bool _dead;
    public AudioClip CaughtSfx;
    public float CaughtSfxVolume = 1f;

    private void OnCollisionEnter(Collision collision)
    {
        if (_dead) return;
        if (IsEnemy(collision.collider))
        {
            _dead = true;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PlayerDied();
            }

            if (CaughtSfx != null)
            {
                AudioSource.PlayClipAtPoint(CaughtSfx, transform.position, Mathf.Clamp01(CaughtSfxVolume));
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_dead) return;
        if (IsEnemy(other))
        {
            _dead = true;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PlayerDied();
            }

            if (CaughtSfx != null)
            {
                AudioSource.PlayClipAtPoint(CaughtSfx, transform.position, Mathf.Clamp01(CaughtSfxVolume));
            }
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (_dead) return;
        if (IsEnemy(hit.collider))
        {
            _dead = true;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PlayerDied();
            }

            if (CaughtSfx != null)
            {
                AudioSource.PlayClipAtPoint(CaughtSfx, transform.position, Mathf.Clamp01(CaughtSfxVolume));
            }
        }
    }

    private bool IsEnemy(Component other)
    {
        if (other == null) return false;
        return other.GetComponentInParent<EnemyController>() != null;
    }
}
