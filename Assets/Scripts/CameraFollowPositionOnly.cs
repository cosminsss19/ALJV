using UnityEngine;

public class CameraFollowPositionOnly : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 8.7f, 0f);
    [SerializeField] private Vector3 fixedEuler = new Vector3(79.19f, 0f, 0f);

    private void LateUpdate()
    {
        if (target == null) return;
        transform.position = target.position + offset;
        transform.rotation = Quaternion.Euler(fixedEuler);
    }
}