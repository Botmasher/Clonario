using UnityEngine;
using System.Collections;

public class MarioInput : MonoBehaviour {

	/**
	 * 		-  speed counter for walk <-> run check
	 * 		-  speed counter for run <-> prun check
	 * 		-  jump (responsive to speed)
	 * 			- while running
	 * 			- while walking
	 * 			- while standing
	 * 		-  prun jump
	 * 		-  slide
	 * 		-  mushroom->big
	 * 		-  hit->small
	 */

	// control flow variables
	private bool isJumping = false;
	private bool isRunning = false;
	//private float jumpTime = 0f;		// how long button held to influence jump
	private float horiz;				// -1 to 1 axis value along x/y axis (for gradual walk/run start/stop)
	private float extraJumpTime;		// max length of time button can influence jump

	private float speed;			// current movement speed (adjusted during play)
	private float minSpeed = 2f;	// compared for walking, etc.
	private float maxSpeed = 5.6f;	// compared for running, P meter, etc.


	// Use this for initialization
	void Awake () {
		// initial walk & run speed
		speed = minSpeed;
	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetKey (KeyCode.LeftShift)) {
			isRunning = true;
		} else {
			isRunning = false;
		}

		// running or walking left
		if (Input.GetKey (KeyCode.LeftArrow)) {
			// flip and move left
			StartCoroutine(Flip("left"));
			WalkRun();
		
		// running or walking right
		} else if (Input.GetKey (KeyCode.RightArrow)) {	
			// flip and move right
			StartCoroutine(Flip("right"));
			WalkRun();
		
		// not running or walking
		} else {
			StopWalkRun();
		}

		// get the axis and use it, with speed, to move forward over time
		horiz = Input.GetAxis ("Horizontal");
		transform.Translate (new Vector2 (0.8f, 0f) * Mathf.Abs(horiz*2.8f) * speed * Time.deltaTime);

		// jump and highjump button press, hold and release
		if (Input.GetKeyDown (KeyCode.Space) && !isJumping) {
			StartCoroutine ("Jump");
			extraJumpTime = 5f;
			/**
			 * 
			 * if button held down, make jump modulate higher
			 * 		/!\  BROKEN - can double jump(ish), can keep jumping when hit head, floatiness, timing off, ...
			 * 		|?|  WHY NOT - gather up key data and send it during jump (Translate?). 
			 * 	If key off, stop adding height. if key gets to max, stop adding height
			 * once at height, kill translate and drop w gravity
			 */
		} else if (Input.GetKey (KeyCode.Space) && isJumping) {
			extraJumpTime -= Time.deltaTime;
		} else if (Input.GetKeyUp (KeyCode.Space)) {
			JumpDamp (extraJumpTime);
		}

	}


	/**
	 * 	Player jumps by placing upward force below
	 */
	IEnumerator Jump () {
		GetComponent<Animator> ().SetBool ("isJumping", true);
		isJumping = true;
		GetComponent<Rigidbody2D>().AddForceAtPosition( new Vector2(0f,(750f+(110f*speed))), new Vector2(transform.position.x, transform.position.y-0.3f) );
		yield return null;
	}


	/**
	 * 	Player jumps less high when jump button held shorter
	 */
	void JumpDamp (float extraJumping) {
		// place force ABOVE character head to push back down
		GetComponent<Rigidbody2D>().AddForceAtPosition( new Vector2(0f,-(100f*extraJumping)), new Vector2(transform.position.x, transform.position.y+0.3f) );
	}


	/*	2 broken ways to make character jump higher with more button press
	IEnumerator JumpInfluence (float buttonStrength) {

		//GetComponent<Rigidbody2D>().AddForceAtPosition( new Vector2(0f,(200f*buttonStrength)), new Vector2(transform.position.x, transform.position.y-0.3f) );
		//transform.Translate (new Vector2 (0f, Mathf.Abs(5f*Mathf.Log(buttonStrength)*Time.deltaTime)));

		yield return null;
	}
	*/


	/**
	 * 	Sprite lateral movement:
	 * 		-  check for running
	 * 		-  if running, slowly adjust speed up and set animator state
	 * 		-  if walking, adjust speed down and set animator state
	 * 		-  move character with adjusted speed
	 */
	void WalkRun () {
		GetComponent<Animator> ().SetBool ("isWalking", true);
		if (isRunning) {
			if (speed < maxSpeed) {
				speed += 0.4f;
			}
			GetComponent<Animator> ().SetBool ("isRunning", true);
		} else {
			if (speed > minSpeed) {
				speed -= 0.6f;
			} else if (speed < minSpeed) {
				speed = minSpeed;
			}
			GetComponent<Animator> ().SetBool ("isRunning", false);
		}
	}

	void StopWalkRun () {
		GetComponent<Animator> ().SetBool ("isWalking", false);
		GetComponent<Animator> ().SetBool ("isRunning", false);
		speed = minSpeed;
	}
	

	IEnumerator Flip (string direction) {
		// set numerical value of L or R flip (passed in as string)
		float flip = (direction == "left") ? 180f: 0f;

		/* SKID BEFORE FLIPPING WHEN MOVING FAST ENOUGH
		if (speed > minSpeed * 1.4f && Mathf.Abs(flip - transform.rotation.y) > 10f) {
			GetComponent<Animator> ().SetBool ("isSkidding", true);
			yield return new WaitForSeconds (0.005f * speed);
			GetComponent<Animator> ().SetBool ("isSkidding", false);
		}
		*/

		// flip L or R
		transform.rotation = Quaternion.Euler(new Vector3 (0f,flip,0f));

		// avoid traveling along z depth from some 180 y-rotation quirk I haven't solved
		transform.position = new Vector3 (this.transform.position.x, this.transform.position.y, 0f);

		yield return null;
	}
	

	// check ground collision to stop jumping animation and allow jump again!
	void OnCollisionEnter2D (Collision2D other) {
		if (isJumping && other.gameObject.tag == "Ground") {
			// if the collision contacts on the top side of the ground, land
			if (other.contacts[0].point.y > other.transform.position.y) {
				GetComponent<Animator> ().SetBool ("isJumping", false);
				isJumping = false;
			}
		}
	}

}
