using UnityEngine;
using System.Collections;

public class PlayerInput : MonoBehaviour {
	
	public GameObject playerSprite;		// main platformer sprite to animate (script accesses its controller/state machine variables)

	/**
	 * 		-  speed counter for run <-> prun check
	 * 		-  prun jump
	 * 		-  slide
	 * 		-  mushroom->big
	 * 		-  hit->small
	 * 		-  die
	 */

	// control flow variables
	private bool isJumping = false;
	private bool isRunning = false;
	private bool isGrounded = false;
	private bool isSkidding = false;
	private bool isDucking = false;

	// global variables checked by other scripts
	public static bool isBig = false;
	public static bool isHurt = false;

	private int health = 1;				// current health (0: dead, 1: small, 2: big, 3: super)
	private int coins = 0;				// current coin count

	//private float jumpTime = 0f;		// how long button held to influence jump
	private float horiz;				// -1 to 1 axis value along x/y axis (for gradual walk/run start/stop)
	private float extraJumpTime;		// max length of time button can influence jump
	
	private float speed;			// current movement speed (adjusted during play)
	private float minSpeed = 2f;	// compared for walking, etc.
	private float maxSpeed = 5.6f;	// compared for running, P meter, etc.
	
	

	void Awake () {
		// initial walk & run speed
		speed = minSpeed;
	}
	

	void Update () {

		// grab left/right input axis
		if (isDucking) {
			horiz = horiz*0.97f;
		} else {
			horiz = Input.GetAxis ("Horizontal");
		}

		if (Input.GetKey (KeyCode.RightArrow)) {
			XInput("right");
		}
		
		if (Input.GetKey (KeyCode.LeftArrow)) {
			XInput("left");
		}

		/** left/right code DRYed in separate XInput function below
		if (Input.GetKey (KeyCode.RightArrow)) {	
			// skid if already moving fast enough in opposite direction
			if (-horiz > 0f && speed > minSpeed && isSkidding == false) {
				// add force to give skid effect (see Update movement code below - isSkidding temporarily stops opposing movement)
				GetComponent<Rigidbody2D> ().AddForceAtPosition (new Vector2 (-1000f - speed * 5f, 0f), new Vector2 (1.25f, 0f));
				StartCoroutine ("Skid");
			// move right if not skidding
			} else if (isSkidding == false) {
				StartCoroutine (Flip ("right"));
				WalkRun ();
			}
		}
		if (Input.GetKey (KeyCode.LeftArrow)) {
			// skid if already going fast enough in the other direction
			if (horiz > 0f && speed > minSpeed && isSkidding == false) {
				// add force to give skid effect (see Update movement code below - isSkidding temporarily stops opposing movement)
				GetComponent<Rigidbody2D> ().AddForceAtPosition (new Vector2 (1000f + speed * 5f, 0f), new Vector2 (-1.25f, 0f));
				StartCoroutine ("Skid");
			// move left if not skidding
			} else if (isSkidding == false) {
				StartCoroutine (Flip ("left"));
				WalkRun ();
			}
		}
		*/
		if (Input.GetKey (KeyCode.DownArrow)) {
			playerSprite.GetComponent<Animator> ().SetBool ("isDucking", true);
			isDucking = true;
		} else {
			playerSprite.GetComponent<Animator> ().SetBool ("isDucking", false);
			isDucking = false;
		}

		// if no horizontal input, stop
		if (Mathf.Abs (horiz) < 0.01f) {
			StopWalkRun();
		}
		
		// use axis with speed to move forward over time
		if (!isSkidding) {
			transform.Translate (new Vector2 (horiz * 2f, 0f) * speed * Time.deltaTime);
			GetComponent<Rigidbody2D> ().AddForceAtPosition (new Vector2 ((12f * horiz) + (2.4f * horiz * speed), 0f), new Vector2 (-horiz * 2f, 0f));
		}
		
		/**
		 * 	jump and highjump button press, hold and release
		 * 		- jump by adding force below player
		 * 		- damp jump by adding force above player in proportion to jump button hold time
		 */
		if (Input.GetKeyDown (KeyCode.Space) && !isJumping) {
			StartCoroutine ("Jump");
			extraJumpTime = 5f;
		} else if (Input.GetKey (KeyCode.Space) && isJumping) {
			extraJumpTime -= Time.deltaTime;
		} else if (Input.GetKeyUp (KeyCode.Space)) {
			JumpDamp (extraJumpTime);
		}
		
	}


	/**
	 * 	Control left/right actions - check for slide momentum, do walk and flip
	 */
	void XInput (string direction) {
		// positive or negative multiple for x-direction math
		int dirMath = (direction == "left") ? -1 : 1;

		// check for running key
		if (Input.GetKey (KeyCode.LeftShift)) {
			isRunning = true;
		} else {
			isRunning = false;
		}

		// skid if already going fast enough in the other direction
		if (dirMath*-horiz > 0f && speed > minSpeed && isSkidding == false) {
			// add force to give skid effect (see Update movement code below - isSkidding temporarily stops opposing movement)
			GetComponent<Rigidbody2D> ().AddForceAtPosition (new Vector2 (-dirMath * 1000f + -dirMath * speed * 5f, 0f), new Vector2 (dirMath*1.25f, 0f));

			StartCoroutine ("Skid");
		
		// move if not skidding
		} else if (!isSkidding) {
			StartCoroutine (Flip (direction));
			WalkRun ();
		}
	}


	/**
	 * 	Player jumps by placing upward force below
	 */
	IEnumerator Jump () {
		// play file in this object's audio component (should be jumping sfx)
		GetComponent<AudioSource> ().Play ();
		// tell controller to animate jumping
		playerSprite.GetComponent<Animator> ().SetBool ("isJumping", true);

		// access jump behaviors in Update
		isJumping = true;

		// apply jump force below player
		GetComponent<Rigidbody2D>().AddForceAtPosition( new Vector2(0f,(1000f+(115f*speed))), new Vector2(transform.position.x, transform.position.y-0.3f) );

		yield return null;
	}
	
	
	/**
	 * 	Player jumps less high when jump button held shorter
	 */
	void JumpDamp (float extraJumping) {
		// place force above character to push back down
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
	 *	Time and animate player skid when changing directions at speed
	 *		-  toggle bool used for movement control (temporarily avoids opposite movement)
	 *		-  toggle animation controller's skid clip
	 *		-  wait, then turn skid values off
	 */
	IEnumerator Skid () {
		isSkidding = true;
		playerSprite.GetComponent<Animator>().SetBool ("isSkidding", true);
		yield return new WaitForSeconds (0.01f + 0.04f*speed);
		playerSprite.GetComponent<Animator>().SetBool ("isSkidding", false);
		speed = minSpeed;
		isSkidding = false;
	}
	
	
	/**
	 * 	Sprite lateral movement:
	 * 		-  check for running
	 * 		-  if running, slowly adjust speed up and set animator state
	 * 		-  if walking, adjust speed down and set animator state
	 * 		-  move character with adjusted speed
	 */
	void WalkRun () {
		playerSprite.GetComponent<Animator> ().SetBool ("isWalking", true);
		if (isRunning && !isDucking) {
			if (speed < maxSpeed) {
				speed += 0.4f;
			}
			playerSprite.GetComponent<Animator> ().SetBool ("isRunning", true);
		} else {
			if (speed > minSpeed) {
				speed -= 0.6f;
			} else if (speed < minSpeed) {
				speed = minSpeed;
			}
			playerSprite.GetComponent<Animator> ().SetBool ("isRunning", false);
		}

		// make sure that feet don't keep wiggling when off platforms
		if (isGrounded == false) {
			playerSprite.GetComponent<Animator> ().SetBool ("isWalking", false);
			playerSprite.GetComponent<Animator> ().SetBool ("isRunning", false);	
		}
	}


	/**
	 * 	Turn off animator controller walking animations
	 */
	void StopWalkRun () {
		playerSprite.GetComponent<Animator> ().SetBool ("isWalking", false);
		playerSprite.GetComponent<Animator> ().SetBool ("isRunning", false);
		speed = minSpeed;
	}

	/**
	 * 	Get hit or die routine
	 */
	IEnumerator GetHurt (GameObject enemy) {

		health --;

		// die if no health left
		if (health == 0) {
			LevelManager.playDead = true;
			Time.timeScale = 0f;
			playerSprite.GetComponent<Animator> ().SetBool ("isDead", true);

		// get hit
		} else {
			isHurt = true;
			isBig = false;

			LevelManager.playPipe = true;	// play shrinking hit sfx

			playerSprite.GetComponent<Animator>().SetBool("isBig", false);

			// temporary invul
			for (int i = 0; i<6; i++) {
				playerSprite.GetComponent<SpriteRenderer>().enabled = false;
				yield return new WaitForSeconds (0.05f);
				playerSprite.GetComponent<SpriteRenderer>().enabled = true;
				yield return new WaitForSeconds (0.05f);
			}

			isHurt = false;

		}

		yield return null;
	}


	/**
	 * 	Switch player facing direction (y-flip the sprite)
	 */
	IEnumerator Flip (string direction) {
		// set numerical value of L or R flip (passed in as string)
		float flip = (direction == "left") ? 180f: 0f;
		
		// flip L or R
		playerSprite.transform.rotation = Quaternion.Euler(new Vector3 (0f,flip,0f));

		// avoid traveling along z depth from some 180 y-rotation quirk I haven't solved
		playerSprite.transform.position = new Vector3 (this.transform.position.x, this.transform.position.y, 0f);
		
		yield return null;
	}
	
	
	void OnCollisionEnter2D (Collision2D other) {
		// check ground collision to stop jumping animation and allow jump again!
		if (other.gameObject.tag == "Ground") {
			// if the collision contacts on the top side of the ground, land
			if (other.contacts [0].point.y > other.transform.position.y - 0.1f) {
				if (isJumping) {
					playerSprite.GetComponent<Animator> ().SetBool ("isJumping", false);
					isJumping = false;
				}
				isGrounded = true;
			}
		// pickup coin and tell level manager to play coin sound
		} else if (other.gameObject.tag == "Coin") {
			coins++;
			LevelManager.playCoin = true;
			Destroy (other.gameObject);
		// pickup mushroom and gain health
		} else if (other.gameObject.tag == "Mushroom") {
			if (!isBig) {
				isBig = true;
				health = 2;
			}
		// get hurt by enemies
		} else if (other.gameObject.tag == "Enemy" && isHurt == false) {
			// lower hp by one and take effect
			StartCoroutine (GetHurt(other.gameObject));
		}
	}

	void OnCollisionExit2D (Collision2D other) {
		if (other.gameObject.tag == "Ground" && other.contacts[0].point.y > other.transform.position.y) {
			isGrounded = false;
		}
	}

}