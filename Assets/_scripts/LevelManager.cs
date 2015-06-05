using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour {

	// access UI elements
	public UnityEngine.UI.Image screenFader;

	// variables for accessing and playing level sounds
	public static bool playCoin = false;
	public static bool playPower = false;
	public static bool playDead = false;
	public static bool playBounce = false;
	public static bool playPipe = false;
	public AudioClip pipeSound;
	public AudioClip coinSound;
	public AudioClip powerSound;
	public AudioClip deadSound;
	public AudioClip bounceSound;

	// list of all sound variables for check
	private List<bool> soundList = new List<bool>();

	// variables for building level
	public GameObject blockWood;		// sprite
	private int levelWidth = 300;		// dimensions


	void Awake() {
		// build a wide block floor
		for (int i = -200; i < levelWidth; i++) {
			Instantiate (blockWood, new Vector3 ((float)i/10f, -2f, 0f), Quaternion.Euler (0f, 0f, 0f));
			i += 5;
		}

		// build a shorter suspended block walkway
		for (int i = 50; i < levelWidth-150; i++) {
			Instantiate (blockWood, new Vector3 ((float)i/10f, 0f, 0f), Quaternion.Euler (0f, 0f, 0f));
			i += 5;
		}
	}
	

	void Update () {

		// player snags coin sfx
		if (playCoin) {
			PlaySFX (coinSound);
			playCoin = false;
		
		// player eats powerup sfx
		} else if (playPower) {
			PlaySFX (powerSound);
			playPower = false;
		
		// player hit or going down pipe sfx
		} else if (playPipe) {
			PlaySFX (pipeSound);
			playPipe = false;

		// play death music and turn off other music 
		} else if (playDead) {
			GameObject.Find ("Camera Manager").GetComponent<AudioSource>().Stop();	// stop game music
			PlaySFX(deadSound);
			playDead = false;

		// stomping and bouncing sfx
		} else if (playBounce) {
			PlaySFX(bounceSound);
			playBounce = false;
		}

	}

	// basic audio load and play
	void PlaySFX (AudioClip sfx) {
		GetComponent<AudioSource>().clip = sfx;
		GetComponent<AudioSource>().Play ();
	}

}
