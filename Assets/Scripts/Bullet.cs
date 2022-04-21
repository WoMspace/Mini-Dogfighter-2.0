using System.Diagnostics;
using UnityEngine;

public class Bullet : MonoBehaviour
{
	public int despawnTimeMS;
	private Stopwatch despawn = new();
	private GameObject bullet;

	public bool playerTeam;

	public GameObject explosion;
	// Start is called before the first frame update
	void Start()
	{
		bullet = GetComponentInParent<GameObject>();
		
		despawn.Restart();
	}

	// Update is called once per frame
	void Update()
	{
		if (despawn.ElapsedMilliseconds > despawnTimeMS)
		{
			Destroy(bullet);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		string collisionTag = playerTeam ? "Enemy" : "Player";
		if (collision.collider.CompareTag(collisionTag) || collision.collider.CompareTag("Floor"))
		{
			BulletCollision(collision.collider.gameObject);
		}
	}

	private void BulletCollision(GameObject victim)
	{
		Instantiate(explosion).GetComponent<ParticleSystem>().Play();
		victim.SendMessage("BulletHit");
		Destroy(bullet);
	}
}