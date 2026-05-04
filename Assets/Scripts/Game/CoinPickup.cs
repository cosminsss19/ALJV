using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    private bool _collected;
    public AudioClip PickupSfx;
    public float PickupSfxVolume = 100f;

    private void OnTriggerEnter(Collider other)
    {
        if (_collected) return;
        if (!other.CompareTag("Player")) return;

        _collected = true;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CollectCoin();
        }

        if (PickupSfx != null)
        {
            AudioSource.PlayClipAtPoint(PickupSfx, transform.position, PickupSfxVolume);
        }

        Destroy(gameObject);
    }
}
