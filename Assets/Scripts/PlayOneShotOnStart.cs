using UnityEngine;

public class PlayOneShotOnStart : MonoBehaviour
{
    [SerializeField] AudioManager audiM;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audiM.PlaySFX(audiM.jumpSFX);
    }
}
