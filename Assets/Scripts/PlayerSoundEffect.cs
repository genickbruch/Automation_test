using UnityEngine;
using SpeechIO;
public class PlayerSoundEffect : MonoBehaviour
{
    public AudioClip dropInClip;
    public AudioClip gameOverClip;
    public AudioClip collisionClip;
    public float maxPitch = 1.2f;
    public float minPitch = 0.8f;
    private GameObject previousEnemy;
    private AudioSource audioSource;
    public SpeechOut speechOut;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        // speechOut = new SpeechOut();
    }
    public float PlayerFellDown()
    {
        audioSource.Stop();
        audioSource.PlayOneShot(gameOverClip);
        return gameOverClip.length;
    }
    public void PlayHit()
    {
        PlayClipPitched(collisionClip, minPitch, maxPitch);
    }
    public void PlayDropIn()
    {
        audioSource.PlayOneShot(dropInClip);
    }
    
    public void PlayEnemyHitClip(AudioClip clip, GameObject go = null)
    {
        if (go)
        {
            if (previousEnemy)
            {
                if (go.Equals(previousEnemy))
                    return;
            }
            previousEnemy = go;
        }
        audioSource.PlayOneShot(clip);
        
        // BIS TODO: uncomment
        // previousEnemy = go;
        // audioSource.clip = clip;
        // audioSource.Play();
        
        // BIS TODO: uncomment
        // Enemy enemy = go.GetComponent<Enemy>(); 
        // speechOut.Speak("I was hit by: " + enemy.displayName);
        
        // BIS TODO: uncomment
        // SayName(go.GetComponent<Enemy>());


    }

    // BIS TODO: uncomment
    // public void PlayEnemyHitClip(GameObject go = null)
    // {
    //     if (go)
    //     {
    //         if (previousEnemy)
    //         {
    //             if (go.Equals(previousEnemy))
    //                 return;
    //         }
    //         previousEnemy = go;
    //     }
    //     SayName(go.GetComponent<Enemy>());
    // }
    
    private async void SayName(Enemy e)
    {
  // BIS TODO: uncomment
      speechOut.Stop();
      await speechOut.Speak("I was hit by");
      await speechOut.Speak(e.displayName);

    }
    
    public void StopPlayback()
    {
        audioSource.Stop();
    }

    public void PlayClipPitched(AudioClip clip, float minPitch, float maxPitch)
    {
        // little trick to make clip sound less redundant
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        // plays same clip only once, this way no overlapping
        audioSource.PlayOneShot(clip);
        audioSource.pitch = 1f;
    }
    
    void OnApplicationQuit() {
        speechOut.Stop();
    }

}
