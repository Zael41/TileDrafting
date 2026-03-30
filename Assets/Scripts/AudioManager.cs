using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    #region "Variables"
    [Header("Assets")]
    [SerializeField] private AudioClip music;
    [SerializeField] private Sounds[] sounds;

    private AudioSource soundsSource;
    private AudioSource musicSource;

    public static AudioManager instance;
    #endregion

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        soundsSource = GetComponents<AudioSource>()[0];
        musicSource = GetComponents<AudioSource>()[1];
        musicSource.clip = music;
        musicSource.loop = true;
        musicSource.volume = 0.1f;
        musicSource.Play();
    }

    public void PlaySound(string name, float volume)
    {
        List<Sounds> availableSounds = sounds.Where(x => x.name == name).ToList();
        AudioClip clipToPlay = null;
        if (availableSounds.Count > 1)
        {
            int randomIndex = Random.Range(0, availableSounds.Count());
            clipToPlay = availableSounds[randomIndex].clip;
        }
        else
        {
            clipToPlay = availableSounds[0].clip;
        }
            soundsSource.PlayOneShot(clipToPlay, volume);
    }
}

[System.Serializable]
public class Sounds
{
    public string name;
    public AudioClip clip;
}
