using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovingObject : MonoBehaviour {

	public float moveTime = 0.1f;
	public LayerMask blockingLayer;

	private BoxCollider2D boxCollider;
	private Rigidbody2D rb2D;
	private float inverseMoveTime;

	// Use this for initialization
	protected virtual void Start () {
		boxCollider = GetComponent<BoxCollider2D>();
		rb2D = GetComponent<Rigidbody2D>();
		inverseMoveTime = 1f / moveTime;
	}

	// Checks if the desired move is valid or would be blocked.  True if successful, False if blocked.
	protected bool Move(int xDir, int yDir, out RaycastHit2D hit) {
		// Get a Vector2 of our current position.
		Vector2 start = transform.position;
		// Calculate our end position based on the xDir,yDir of movement.
		Vector2 end = start + new Vector2(xDir, yDir);

		// Temporarily disable our own collider so we don't accidentally compute a collision with ourselves.
		boxCollider.enabled = false;
		// Cast a line out from where we are to where we are going and see if any other member of the blocking layer will cause a collision.
		hit = Physics2D.Linecast(start, end, blockingLayer);
		// Turn that boxCollider back on!
		boxCollider.enabled = true;

		// Check if we collided
		if (hit.transform == null) {
			// If not, move our ass and return true
			StartCoroutine(SmoothMovement(end));
			return true;
		}

		// We couldn't move cus something is in our way.  Return false.
		return false;
	}

	// Ho boy.  This function performs the actual movement itself of the sprites.  
	protected IEnumerator SmoothMovement(Vector3 end) {
		// We do this to calculate the remaining distance from where we started to where we want to be.
		float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

		// Bro...  The fuck is float.Epsilon?  I guess as long as our distance is greater than that we're animating
		while (sqrRemainingDistance > float.Epsilon) {
			// Calculate how far we should move this frame based on current position, linear movement to our destination, determined by the amount of time between last frame and this one
			Vector3 newPosition = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);
			// Actually do the move.
			rb2D.MovePosition(newPosition);
			// Recalculate the remaining distance
			sqrRemainingDistance = (transform.position - end).sqrMagnitude;
			// Hold off until the next frame before doing the loop again.
			yield return null;
		}
	}

	protected virtual void AttemptMove <T> (int xDir, int yDir)
		where T : Component {

		RaycastHit2D hit;
		bool canMove = Move(xDir, yDir, out hit);

		if (hit.transform == null)
			return;

		T hitComponent = hit.transform.GetComponent<T>();

		if (!canMove && hitComponent != null)
			OnCantMove(hitComponent);
	}
	
	// This shit should get called when the object can't move for some reason but is completely abstract for this class and will get defined in the child classes.
	protected abstract void OnCantMove<T>(T component)
		where T : Component;
}
