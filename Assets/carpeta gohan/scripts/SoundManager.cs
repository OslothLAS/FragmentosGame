using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] AudioClip[] audioClips;
    AudioSource audioSource;
    public static SoundManager instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
        audioSource = GetComponent<AudioSource>();
    }
    public void SeleccionarAudio(int index, float volume = 1f)
    {
      audioSource.PlayOneShot(audioClips[index], volume);  
    }
        
}
