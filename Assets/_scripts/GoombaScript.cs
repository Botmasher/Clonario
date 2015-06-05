using UnityEngine;
using System.Collections;

public class GoombaScript : MonoBehaviour {

	// enemy speed and direction
	private int directionX = -1;	// start out moving left (-1) or right (1)
	private float speed = 2f;

	// control flow variables
	public bool isMoving = true;
	private bool isGrounded = false;


	void Update () {
		// move when not stomped by player
		if (isMoving) {
			transform.Translate (Vector2.right * directionX * speed * Time.deltaTime);
		}
	}

	/**
	 * 	Goomba gets stomped (animations and components), pauses for a moment, then dies
	 */
	IEnumerator Stomped () {
		// move down to avoid floating sprite (since stomped goomba sprite dimensions are y-squished)
		transform.Translate (-Vector2.up*0.05f);

		// turn off components that interact with world (avoid movement)
		GetComponent<BoxCollider2D>().enabled = false;
		GetComponent<PolygonCollider2D>().enabled = false;
		GetComponent<Rigidbody2D>().isKinematic = true;

		// make character sprite appear stomped
		GetComponent<Animator>().SetBool("isStomped", true);
		isMoving = false;

		// wait then die
		yield return new WaitForSeconds (0.4f);
		Destroy (this.gameObject);

	}

	void OnCollisionEnter2D (Collision2D other) {
		if (other.gameObject.tag == "Ground") {
			// change direciton when hit side of ground
			if (other.contacts [0].point.y < other.transform.position.y) {
				directionX = -directionX;
			}
		}
	}

	void OnTriggerEnter2D (Collider2D other) {
		if (other.gameObject.tag == "Player" && PlayerInput.isHurt == false) {
			// tell level manager to play bounce sound
			LevelManager.playBounce = true;
			// bounce player into air
			other.GetComponent<Rigidbody2D>().AddForceAtPosition(new Vector2(0f, 1320f), new Vector2(other.transform.position.x, other.transform.position.y-0.2f)); 
			// start own stomp and die routine
			StartCoroutine (Stomped());
		}
	}
	
}
