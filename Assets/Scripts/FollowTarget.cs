using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    // Follows exact position, need to add deadzone behavior

    [SerializeField] private Transform target_transform;
    private Transform self_transform;

    private void Awake()
    {
        self_transform = GetComponent<Transform>();
    }

    private void FixedUpdate()
    {
        self_transform.position = target_transform.position;

        self_transform.position += new Vector3(0,0,-10f);
    }
}
