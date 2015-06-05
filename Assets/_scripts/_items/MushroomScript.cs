using UnityEngine;
using System.Collections;

public class MushroomScript : MonoBehaviour {

	private float speed = 2f;			// item travel speed
	private int directionX;				// whether item travels left or right

	private bool spawning = true;		// control flow var for item to pop out of box
	private float spawnPopTime = 1f;	// amount of time to spend popping out of item box

	/**
	 * 	Set horizontal direction and start spawn popout animation
	 */
	void Start () {
		// randomly select left or right direction when enters world
		directionX = new int[2]{-1,1}[Random.Range (0,2)];

		StartCoroutine ("Spawn");
	}


	/**
	 * 	Move along horizontally (see collider messages for advanced interactions)
	 */
	void Update () {
		// once spawn thread finishes, move horizontally
		if (!spawning) {
			transform.Translate (new Vector2 (directionX, 0f) * speed * Time.deltaTime);
		}
	}


	/**
	 * 	Start out moving slowly up out of powerup container
	 */
	IEnumerator Spawn() {
		// turn on rigidbody animated/kinematic control
		GetComponent<Rigidbody2D> ().isKinematic = true;
		// spend certain amt of time popping out of container
		while (spawnPopTime > 0f) {
			transform.Translate (Vector2.up * 0.6f * Time.deltaTime);	// move up
			spawnPopTime -= Time.deltaTime;						// count down to spawn finish
			yield return null;
		}
		// turn off rigidbody animated/kinematic control
		GetComponent<Rigidbody2D> ().isKinematic = false;
		// let control flow know spawn pop up has finished
		spawning = false;
	}


	IEnumerator PowerUp (GameObject other) {
		this.GetComponent<SpriteRenderer>().enabled = false;

		// turn off movement components to make player freeze
		other.GetComponent<PlayerInput>().enabled = false;
		other.GetComponent<Rigidbody2D>().isKinematic = true;

		// tell level manager to play powerup sfx
		LevelManager.playPower = true;

		// time temporary "growing" animation
		yield return new WaitForSeconds (0.1f);
		other.transform.GetChild(0).GetComponent<Animator>().SetBool("isBig", true);
		yield return new WaitForSeconds (0.1f);
		other.transform.GetChild(0).GetComponent<Animator>().SetBool("isBig", false);
		yield return new WaitForSeconds (0.1f);
		other.transform.GetChild(0).GetComponent<Animator>().SetBool("isBig", true);
		yield return new WaitForSeconds (0.1f);
		other.transform.GetChild(0).GetComponent<Animator>().SetBool("isBig", false);
		yield return new WaitForSeconds (0.1f);
		other.transform.GetChild(0).GetComponent<Animator>().SetBool("isBig", true);

		// turn on movement components to return player to normal states
		other.GetComponent<Rigidbody2D>().isKinematic = false;
		other.GetComponent<PlayerInput>().enabled = true;



		Destroy (this.gameObject);
		yield return null;
	}


	/**
	 * 	Interact with other objects once spawned:
	 * 		- change direction when hit side of ground block
	 * 		- power up player when hit player
	 */
	void OnCollisionEnter2D (Collision2D other) {

		// change direction when hit side of ground block
		if (other.gameObject.tag == "Ground" && other.contacts [0].point.y < other.transform.position.y && spawning == false) {
			directionX = -directionX;

		// power up player when hit player
		} else if (other.gameObject.tag == "Player" && spawning == false) {
			// make player big, then remove this object from world
			StartCoroutine(PowerUp(other.gameObject));
		}
	}

}
