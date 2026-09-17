using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class HideOnStart : MonoBehaviour
{
    private SpriteRenderer selfSprtRend;

    private void Awake()
    {
        selfSprtRend = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        selfSprtRend.enabled = false;
    }
}
