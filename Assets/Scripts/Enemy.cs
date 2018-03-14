using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MovingObject {

	public int playerDamage;  // Damage done to player on collison
	public AudioClip enemyAttack1;
	public AudioClip enemyAttack2;


	private Animator animator;  // Animator
	private Transform target;  // Position/transform of the player
	private bool skipMove;  // We'll use this to cause the enemies to move only every other turn

	// Use this for initialization
	protected override void Start () {
		GameManager.instance.AddEnemyToList(this);
		animator = GetComponent<Animator>();  // Get the Animator
		target = GameObject.FindGameObjectWithTag("Player").transform;  // Locate the player by tag
		base.Start();  // Call base Start()
	}

	// Try to move, if possible.
	protected override void AttemptMove<T>(int xDir, int yDir) {
		// Only move every other turn
		if (skipMove) {
			skipMove = false;
			return;
		}

		// Try to move
		base.AttemptMove<T>(xDir, yDir);

		// We moved successfully, skip the next move
		skipMove = true;
	}

	// Perform the actual move here.  This is called by GameManager
	public void MoveEnemy() {
		int xDir = 0;
		int yDir = 0;

		// OK.  First we do math to figure out if we're in the same column as the player
		if (Mathf.Abs(target.position.x - transform.position.x) < float.Epsilon)
			yDir = target.position.y > transform.position.y ? 1 : -1;  // If we are, are we above or below them?  Adjust xDir as needed (1 = up, -1 = down)
		else
			xDir = target.position.x > transform.position.x ? 1 : -1; // If we are not, let's get there (1 = right, -1 = left)

		AttemptMove<Player>(xDir, yDir);  // Move based on the maths above)
	}

	// We can't move, so we've likely hit a player.  Hurt them.
	protected override void OnCantMove<T> (T component) {
		Player hitPlayer = component as Player;  // Fetch the generic component as a player
		animator.SetTrigger("enemyAttack");
		SoundManager.instance.RandomizeSfx(enemyAttack1, enemyAttack2);
		hitPlayer.LoseFood(playerDamage);  // Fuck 'em up.
	}
}
