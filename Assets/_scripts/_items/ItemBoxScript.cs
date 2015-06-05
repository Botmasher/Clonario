using UnityEngine;
using System.Collections;

public class ItemBoxScript : MonoBehaviour {

	public GameObject item;			// object contained in this box

	// control flow variables for box interaction
	private bool hittable = true;
	private int itemCount = 1;


	/**
	 * 	Interact with player and spawn items
	 */
	void OnTriggerEnter2D (Collider2D other) {
		if (hittable == true) {
			// player hits from below, empties the box and spawns item
			if (other.gameObject.tag == "Player") {
				GetComponent<Animator> ().SetTrigger ("isEmpty");
				SpawnItem();
			}
		}
	}


	/**
	 * 	Bounce box and deliver contained items to player
	 */
	void SpawnItem () {
		// animate bounce up
		StartCoroutine ("Bounce");

		// if there are still items in the box, get them
		if (itemCount > 0) {
			Instantiate (item, new Vector3 (transform.position.x + 0.02f, transform.position.y, transform.position.z + 0.5f), Quaternion.identity);
			itemCount--;
		// if not, play empty sound
		} else {
			GetComponent<AudioSource>().Play();
		}

	}


	/**
	 *	Start bounce animation and make player unable to hit during bounce animation
	 */
	IEnumerator Bounce () {
		hittable = false;
		GetComponent<Animator>().SetTrigger ("isBouncing");
		yield return new WaitForSeconds (0.14f);
		hittable = true;
	}

}
