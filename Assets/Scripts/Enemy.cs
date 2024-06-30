using UnityEngine;
using System.Threading.Tasks;

public class Enemy : MonoBehaviour
{
    public float speed = 8f;
    private Rigidbody enemyRb;
    private GameObject player;
    private bool enemyActive = false;
    
    public string displayName;
    public AudioClip nameClip;


    void Start()
    {
        enemyRb = GetComponent<Rigidbody>();
        player = GameObject.Find("Player");
    }

    void Update()
    {
        if (transform.position.y < -2)
        {
            GameObject.FindObjectOfType<SpawnManager>().FindOtherEnemy();
            Destroy(gameObject);
        }
    }
    
    public async Task ActivateEnemy()
    {
        enemyActive = true;
    }



    void FixedUpdate()
    {
        if (enemyActive)
        {
            /// challenge: set lookDirection to "enemy to player" vector
            Vector3 lookDirection = player.transform.position - transform.position;
            enemyRb.AddForce(lookDirection.normalized * speed);
        }
    }
}
