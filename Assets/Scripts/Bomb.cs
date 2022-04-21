using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public int despawnTimeMS;
    private Stopwatch despawn = new();
    private GameObject bomb;
    public float explosionRange;

    public GameObject explosion;
    // Start is called before the first frame update
    void Start()
    {
        bomb = GetComponentInParent<GameObject>();
        despawn.Restart();
    }

    // Update is called once per frame
    void Update()
    {
        if (despawn.ElapsedMilliseconds > despawnTimeMS)
        {
            Destroy(bomb);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Enemy") || collision.collider.CompareTag("Floor"))
        {
            Detonate();
        }
    }

    [SuppressMessage("ReSharper", "CommentTypo")]
    private void Detonate()
    {
        Instantiate(explosion).GetComponent<ParticleSystem>().Play();
        // List<GameObject> enemies = new();
        foreach (var gameObjects in FindObjectsOfType<GameObject>().ToList())
        { // Get all gameobjects with the tag Enemy and within range
            float distance = Vector3.Distance(gameObjects.transform.position, bomb.transform.position);
            if (gameObjects.CompareTag("Enemy") &&  distance < explosionRange)
            {
                // enemies.Add(gameObject);
                gameObjects.SendMessage("nearbyExplosion", distance);
            }
        }
        Destroy(bomb);
    }
}
