using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MovingObject {

	public int wallDamage = 1;  // Amount of damage we do to the wall when we hit it
	public int pointsPerFood = 10;  // Amount of points we get for Food drops
	public int pointsPerSoda = 20;  // Amount of points we get for Soda drops
	public float restartLevelDelay = 1f;  // 1 sec delay from touching exit to loading the next map.
	public Text foodText;
	public AudioClip moveSound1;
	public AudioClip moveSound2;
	public AudioClip eatSound1;
	public AudioClip eatSound2;
	public AudioClip drinkSound1;
	public AudioClip drinkSound2;
	public AudioClip gameOverSound;

	private Animator animator;  // Animator for the Player
	private int food;  //  We store the food during the map on the player, then save the state to the game manager on exit
	private Vector2 touchOrigin = -Vector2.one;

	// Use this for initialization
	protected override void Start () {
		// Retrieve the Animator
		animator = GetComponent<Animator>();

		// Get the food level from the GameManager
		food = GameManager.instance.playerFoodPoints;

		foodText.text = "Food: " + food;

		// Call the base class start function.
		base.Start();
	}

	// Runs when the player object is disabled (At the end of level)
	private void OnDisable() {
		// Save food to the GameManager which does not destroy on load
		GameManager.instance.playerFoodPoints = food;
	}
	
	// Update is called once per frame
	void Update () {
		// If not our turn to do something, don't do anything.
		if (!GameManager.instance.playersTurn)
			return;

		//Declare and retrieve movement from joystick/keyboard
		int horizontal = 0;
		int vertical = 0;


	#if UNITY_STANDALONE || UNITY_WEBPLAYER
		horizontal = (int)Input.GetAxisRaw("Horizontal");
		vertical = (int)Input.GetAxisRaw("Vertical");

		// This prevents diagonal movement
		if (horizontal != 0)
			vertical = 0;

	#else 
		if (Input.touchCount > 0) {
			Touch myTouch = Input.touches[0];

			if (myTouch.phase == TouchPhase.Began) {
				touchOrigin = myTouch.position;
			} else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0) {
				Vector2 touchEnd = myTouch.position;
				float x = touchEnd.x - touchOrigin.x;
				float y = touchEnd.y - touchOrigin.y;
				touchOrigin.x = -1;
				if (Mathf.Abs(x) > Mathf.Abs(y))
					horizontal = x > 0 ? 1 : -1;
				else
					vertical = y > 0 ? 1 : -1;
			}

		}
	#endif

		// Attempt to move, with an assumption that what we are colliding with is a wall.
		if (horizontal != 0 || vertical != 0)
			AttemptMove<Wall>(horizontal, vertical);
	}


	// Triggers when encountering food, soda, or exit.
	private void OnTriggerEnter2D(Collider2D other) {
		if (other.tag == "Exit") {
			Invoke("Restart", restartLevelDelay);  // Calls the Restart function, making it way the delay time (1 sec)
			enabled = false;  // Disables player, which in turn triggers saving state to GameManager
		} else if (other.tag == "Food") {
			food += pointsPerFood;  // Add food score to food
			foodText.text = "+" + pointsPerFood + " Food: " + food;
			SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);
			other.gameObject.SetActive(false);  // Get rid of the pick up
		} else if (other.tag == "Soda") {
			food += pointsPerSoda;
			foodText.text = "+" + pointsPerSoda + " Food: " + food;
			SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);
			other.gameObject.SetActive(false);
		}
	}

	// If we can't move, we run this
	protected override void OnCantMove<T>(T component) {
		// Cast the object we're colliding with (Should be a wall) as a wall.
		Wall hitWall = component as Wall;
		// Apply damage to the wall
		hitWall.DamageWall(wallDamage);
		// Trigger the animator to run the chop animation
		animator.SetTrigger("playerChop");
	}

	// Reloads the current scene (AKA, move on to the next level)
	private void Restart() {
		SceneManager.LoadScene("Main");
	}

	// Causes the player to lose food (AKA, lose health)
	public void LoseFood(int loss) {
		animator.SetTrigger("playerHit");
		food -= loss;
		foodText.text = "-" + loss + " Food: " + food;
		CheckIfGameOver();
	}

	// Lets see if we can move where we are being told to.
	protected override void AttemptMove <T> (int xDir, int yDir) {
		food--;  // It costs us to move every time
		foodText.text = "Food: " + food;

		base.AttemptMove<T>(xDir, yDir);  // Call the base version of this

		// Retrieve what we're colliding with
		RaycastHit2D hit;
		if (Move(xDir, yDir, out hit)) {
			SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);
		}


		// Are we dead?
		CheckIfGameOver();

		// We've tried to move, our turn is over.
		GameManager.instance.playersTurn = false;
	}

	// If we're dead, trigger the GameOver function in GameManager.
	private void CheckIfGameOver() {
		if (food <= 0) {
			SoundManager.instance.PlaySingle(gameOverSound);
			SoundManager.instance.musicSource.Stop();
			GameManager.instance.GameOver();
		}
	}
}
