/// Hint: Commenting or uncommenting in VS
/// On Mac: CMD + SHIFT + 7
/// On Windows: CTRL + K and then CTRL + C

using UnityEngine;
using DualPantoFramework;
using SpeechIO;
using System.Threading.Tasks;

public class PlayerController : MonoBehaviour
{
    private Rigidbody playerRb;
    public float speed = 5f;
    public GameObject focalPoint;
    public bool hasPowerup;
    private float powerupStrength = 30f;
    public int powerupTime = 7;
    public GameObject powerupIndicator;
    private SpeechIn speechIn;
    private SpeechOut speechOut;
    private bool movementFrozen;
    private UpperHandle upperHandle;
    
    private PlayerSoundEffect soundEffects;
    private bool playerFellDown;
    
    private int powerupAmmo = 2;
    public float explosionRadius = 10f;
    public float explosionPower = 2000f;
    public float explosionUpwardForce = 5f;
    public LayerMask explosionAffected;

    async void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        //await ActivatePlayer();
        speechIn = new SpeechIn(onRecognized);
        speechIn.StartListening(new string[]{"help", "resume"});
        speechOut = new SpeechOut();
        
        soundEffects = GetComponent<PlayerSoundEffect>();
        
        // BIS TODO: uncomment
        // PowerUpListener();
    }
    
    void Update()
    {
        // BIS TODO: uncomment
        if (transform.position.x * transform.position.x + transform.position.z * transform.position.z > 14.5 * 14.5f && !playerFellDown)
        {
            playerFellDown = true;
            float clipTime = soundEffects.PlayerFellDown();
            Destroy(gameObject, clipTime);
        }

        if (!GameObject.FindObjectOfType<SpawnManager>().gameStarted) return;
        powerupIndicator.transform.position = transform.position + new Vector3(0f, -0.5f, 0f);
        
        if (Input.GetKeyDown (KeyCode.Space))
        {
            ExplosionPowerup();
        }
        if (Input.GetButtonDown("Cancel"))
            speechIn.StopListening();
    }
    
    async void PowerUpListener()
    {
        string powerup = await speechIn.Listen(new string[] { "boom", "jump", "hide" });
        switch (powerup)
        {
            case "boom":
                if (powerupAmmo <= 0)
                {
                    await soundEffects.speechOut.Speak("no ammo for:" + powerup);
                    return;
                }
                ExplosionPowerup();
                break;
            case "jump":
                //JumpPowerup();
                break;
            case "hide":
                //HidePowerup();
                break;
        }
        PowerUpListener();
    }
    
    void FixedUpdate()
    {
        if (!GameObject.FindObjectOfType<SpawnManager>().gameStarted) return;
        //float forwardInput = Input.GetAxis("Vertical");
        //playerRb.AddForce(focalPoint.transform.forward * forwardInput * speed);
        PantoMovement();
    }

    void PantoMovement()
    {
        float rotation = upperHandle.GetRotation();
        transform.eulerAngles = new Vector3(0, rotation, 0);
        playerRb.velocity = speed * transform.forward;
    }

    async void onSpeechRecognized(string command) {
        if (command == "resume" && movementFrozen) {
            ResumeAfterPause();
        } else if (command == "help" && !movementFrozen) {
            ToggleMovementFrozen();
            var powerups = GameObject.FindGameObjectsWithTag("Powerup");
            if (powerups.Length > 0) {
                await GameObject.Find("Panto").GetComponent<LowerHandle>().SwitchTo(powerups[0]);
            }
        }
    }

    void ToggleMovementFrozen() {
        playerRb.constraints = movementFrozen ? RigidbodyConstraints.None : RigidbodyConstraints.FreezeAll;
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            enemy.GetComponent<Rigidbody>().constraints = movementFrozen
                                           ? RigidbodyConstraints.None
                                           : RigidbodyConstraints.FreezeAll;
        }
        movementFrozen = !movementFrozen;
    }

    async void ResumeAfterPause() {
        GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");
        if (enemy != null)
        {
            await GameObject.Find("Panto").GetComponent<LowerHandle>().SwitchTo(enemy);
        }
        ToggleMovementFrozen();
    }

    public async Task ActivatePlayer()
    {
        upperHandle = GameObject.Find("Panto").GetComponent<UpperHandle>();
        await upperHandle.SwitchTo(gameObject);
        upperHandle.FreeRotation();
    }



    async void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Powerup"))
        {
            hasPowerup = true;
            powerupIndicator.gameObject.SetActive(true);
            Destroy(other.gameObject);
            CancelInvoke("PowerupCountdown"); // if we previously picked up an powerup
            Invoke("PowerupCountdown", powerupTime);
            await speechOut.Speak("You got the power up");
            // GameObject.FindObjectOfType<SpawnManager>().SpawnEnemyWave();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;
        
        if (other.CompareTag("Enemy"))
        {
            // BIS TODO: uncomment
            soundEffects.PlayHit();

            // BIS TODO: uncomment
            Enemy enemy = other.GetComponent<Enemy>();
            soundEffects.PlayEnemyHitClip(enemy.nameClip, other);

            Rigidbody enemyRigidbody = other.GetComponent<Rigidbody>();
            Vector3 awayFromPlayer = other.transform.position - transform.position;
            Vector3 scaledDirection = awayFromPlayer.normalized * powerupStrength * 0.4f;
            if (hasPowerup)
            {
                scaledDirection = awayFromPlayer.normalized * powerupStrength;
            }
            enemyRigidbody.AddForce(scaledDirection, ForceMode.Impulse);
        }
    }
    
    public void ExplosionPowerup()
    {
        powerupAmmo--;
        Vector3 explosionPos = transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, explosionRadius, explosionAffected);
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
                rb.AddExplosionForce(explosionPower, explosionPos, explosionRadius, explosionUpwardForce);
        }
    }

    void PowerupCountdown()
    {
        hasPowerup = false;
        powerupIndicator.gameObject.SetActive(false);
    }
    
    void onRecognized(string message)
    {
        Debug.Log("recognized " + message);
        // switch (message)
        // {
        //     case "repeat":
        //         await soundEffects.speechOut.Repeat();
        //         break;
        //     case "quit":
        //         await soundEffects.speechOut.Speak("Thanks for using our application. Closing down now");
        //         OnApplicationQuit();
        //         Application.Quit();
        //         break;
        //     case "options":
        //         string commandlist = "";
        //         foreach (string command in speechIn.GetActiveCommands())
        //         {
        //             commandlist += command + ", ";
        //         }
        //         await soundEffects.speechOut.Speak("currently available commands: " + 												commandlist);
        //         break;
        // }

    }

    void OnApplicationQuit() {
        speechOut.Stop();
        speechIn.StopListening();
    }
}