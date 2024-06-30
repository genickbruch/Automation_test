using UnityEngine;
using DualPantoFramework;
using SpeechIO;
using System.Threading.Tasks;

public class SpawnManager : MonoBehaviour
{
    public GameObject enemyPrefab;
    public GameObject powerupPrefab;
    public int waveNumber = 0;  
    public bool gameStarted = false;
    public bool playIntro = false;
    private int enemyCount;
    private SpeechOut speechOut;
    private float spawnRange = 9f;
    
    public string[] nameList;

    void Start()
    {
         StartGame();
        //StartGameWithOutIntro();
        speechOut = new SpeechOut();
    }

    async void StartGameWithOutIntro()
    {
        GameObject powerup = Instantiate(powerupPrefab, GenerateSpawnPosition(), powerupPrefab.transform.rotation);
        await GameObject.Find("Panto").GetComponent<LowerHandle>().SwitchTo(powerup);
        GameObject enemy = Instantiate(enemyPrefab, GenerateSpawnPosition(), enemyPrefab.transform.rotation);
        await GameObject.Find("Panto").GetComponent<LowerHandle>().SwitchTo(enemy);
        GameObject.FindObjectOfType<Enemy>().ActivateEnemy();
        GameObject.FindObjectOfType<PlayerController>().ActivatePlayer();
        gameStarted = true;
    }

    async void StartGame() {
        if (playIntro) 
        {
            Level room = GameObject.Find("Panto").GetComponent<Level>();
            await room.PlayIntroduction();
        }
        await SpawnPowerup();
        await SpawnEnemy();

        await GameObject.FindObjectOfType<PlayerController>().ActivatePlayer();
        await GameObject.FindObjectOfType<Enemy>().ActivateEnemy();
        gameStarted = true;
    }
    

    void OnApplicationQuit()
    {
        speechOut.Stop();
    }

    void Update()
    {
        if (!gameStarted) return;
        enemyCount = FindObjectsOfType<Enemy>().Length;
        if (enemyCount == 0)
        {
            waveNumber++;
            SpawnEnemyWave(waveNumber);
            SpawnPowerup();
        }
    }

    /// challenge: spawn specified numberOfEnemies using Instantiate(...)
    async void SpawnEnemyWave(int numberOfEnemies)
    {
        for (int i = 0; i < numberOfEnemies; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab, GenerateSpawnPosition(), enemyPrefab.transform.rotation);
            enemy.GetComponent<Enemy>().displayName = nameList[i % nameList.Length];
            if (i == 0)
            {
                await GameObject.Find("Panto").GetComponent<LowerHandle>().SwitchTo(enemy);
                GameObject.FindObjectOfType<Enemy>().ActivateEnemy();
            }
        }
    }

    // public async void SpawnEnemyWave() {
    //     await speechOut.Speak("Spawning " + waveNumber + " enemies");
    //     SpawnEnemyWave(waveNumber);
    //     waveNumber++;
    // }

    private Vector3 GenerateSpawnPosition()
    {
        float randomPosX = Random.Range(-spawnRange, spawnRange);
        float randomPosZ = Random.Range(-spawnRange, spawnRange);
        Vector3 randomPos = new Vector3(randomPosX, 0, randomPosZ);
        return randomPos;
    }


    public GameObject GetClosestGameObject(string tag, Vector3 position) 
    {
        GameObject[] gos = GameObject.FindGameObjectsWithTag(tag);

        GameObject closest = null;
        float distance = Mathf.Infinity;

        foreach (GameObject go in gos) {
            float currentDistance = Vector3.Distance(go.transform.position, position);
     
            if (currentDistance < distance)
            {
                closest = go;
                distance = currentDistance;
            }
        }
        return closest;
    }
    async public void FindOtherEnemy()
    {
        Vector3 playerPosition = GameObject.Find("Player").transform.position;

        /*
         * TODO2: Make the it-handle track the closest enemy
         */

        GameObject closestEnemy = GetClosestGameObject("Enemy", playerPosition);
        if (closestEnemy != null)
            await GameObject.Find("Panto").GetComponent<LowerHandle>().SwitchTo(closestEnemy);
    }

    async Task SpawnPowerup()
    {
        GameObject powerup = Instantiate(powerupPrefab, GenerateSpawnPosition(), powerupPrefab.transform.rotation);
        await GameObject.Find("Panto").GetComponent<LowerHandle>().SwitchTo(powerup);
        // await speechOut.Speak("Here is the power up");
    }

    async Task SpawnEnemy()
    {
        GameObject enemy = Instantiate(enemyPrefab, GenerateSpawnPosition(), enemyPrefab.transform.rotation);
        await GameObject.Find("Panto").GetComponent<LowerHandle>().SwitchTo(enemy);
        // await speechOut.Speak("Here is Enemy");
        GameObject.FindObjectOfType<Enemy>().ActivateEnemy();
        waveNumber++;
    }
}
