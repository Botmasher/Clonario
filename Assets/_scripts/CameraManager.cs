using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour {
	/**
	 * 	This script's object needs a trigger set up around the body of the level.
	 * 	When player enters trigger, camera follows player.
	 * 	When player leaves trigger, camera returns to ground level.
	 * 
	 * 	Classical case: beginning and end screens of level have no trigger. Rest of level surrounded by box trigger.
	 * 
	 */

	public GameObject camera;				// the main scene camera controlled by this script
	public GameObject player;				// the player character

	private bool parented = false;			// whether camera following player
	

	void Update () {
		/**
		 * when not following player, slowly return to y-axis
		 *	(avoids camera hanging in sky when player leaves trigger collider)
		 */
		if (!parented) {
			camera.transform.position = Vector3.Lerp (camera.transform.position, new Vector3(camera.transform.position.x, 0f, camera.transform.position.z), 2f * Time.deltaTime);
		}
	}


	// start following player when enters cam trigger zone
	void OnTriggerEnter2D (Collider2D other) {
		if (other.gameObject.tag == "Player") {
			camera.transform.parent = player.transform;		// parent to player empty
			parented = true;								// checked in Update
		}
	}


	// stop following player when leaves cam trigger zone
	void OnTriggerExit2D (Collider2D other) {
		if (other.gameObject.tag == "Player") {
			camera.transform.parent = null;					// unparent from player empty
			parented = false;								// checked in Update
		}
	}

}
