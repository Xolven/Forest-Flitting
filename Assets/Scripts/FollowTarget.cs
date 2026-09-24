using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    // Follows exact position, need to add deadzone behavior

    [SerializeField] private Transform target_transform;
    private Transform self_transform;
    private float moveSpeed = 5f;

    private void Awake()
    {
        self_transform = GetComponent<Transform>();
    }

    private void FixedUpdate()
    {
        self_transform.position = new Vector3(Mathf.Lerp(self_transform.position.x,target_transform.position.x, moveSpeed * Time.deltaTime), Mathf.Lerp(self_transform.position.y, target_transform.position.y, moveSpeed * Time.deltaTime), -10f);
        //self_transform.position += new Vector3(0,0,-10f);
    }
}
