using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    public AudioClip BGMusic;
    public AudioClip deathSFX;
    public AudioClip jumpSFX;

    private void Start()
    {
        if (BGMusic == null)
            return;
        musicSource.clip = BGMusic;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }

}
