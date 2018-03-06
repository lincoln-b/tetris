using UnityEngine;
using System.Collections;
using DigitalRubyShared;

public class TetrominoGenerator : MonoBehaviour {

	public Transform straight;
	public Transform square;
	public Transform tee;
	public Transform rightDog;
	public Transform leftDog;
	public Transform rightElbow;
	public Transform leftElbow;

	public int frameLength = 100;
//	public int cameraMin = 10;

//	[Tooltip("Set the required touches for the swipe.")]
//	[Range(1, 10)]
//	public int SwipeTouchCount = 1;

//	[Tooltip("Controls how the swipe gesture ends. See SwipeGestureRecognizerSwipeMode enum for more details.")]
//	public SwipeGestureRecognizerEndMode SwipeMode = SwipeGestureRecognizerEndMode.EndImmediately;

//	private SwipeGestureRecognizer swipe;

	private Transform activeTetromino;
	private int counter = 0;

	private enum State { Generating, Dropping };
	private State state = State.Generating;

	void Start() {
//		swipe = new SwipeGestureRecognizer();
//		swipe.StateUpdated += Swipe_Updated;
//		swipe.DirectionThreshold = 0;
//		swipe.MinimumNumberOfTouchesToTrack = swipe.MaximumNumberOfTouchesToTrack = SwipeTouchCount;
//		FingersScript.Instance.AddGesture(swipe);

		TapGestureRecognizer tap = new TapGestureRecognizer();
		tap.StateUpdated += Tap_Updated;
		FingersScript.Instance.AddGesture(tap);
	}

	void OnCollisionEnter(Collision collision) {
		Debug.Log (collision);
	}

	void FixedUpdate () {

		// Generate or drop a block
		if (state == State.Dropping) {
			counter++;
			if (counter >= frameLength) {
				counter = 0;
				Vector3 pos = activeTetromino.transform.position;
				pos.y -= 1;
				activeTetromino.transform.position = pos;
			}
		} else if (state == State.Generating) {
			GenerateTetromino ();
			counter = 0;
			state = State.Dropping;
		}

		// Make sure the camera doesn't go too low
//		Vector3 cam = Camera.main.transform.position;
//		if (cam.y < cameraMin) {
//			cam.y = cameraMin;
//			Camera.main.transform.position = cam;
//		}

		// Collisions with the platform
//		if (Physics2D.Raycast (activeTetromino.position, Vector2.down, 1).collider != null) {
//			state = State.Generating;
//		}
			
	}

	void GenerateTetromino() {
		System.Random rnd = new System.Random ();
		int index = rnd.Next (0, 7);
		Transform[] tetrominoes = { straight, square, tee, rightDog, leftDog, rightElbow, leftElbow };
		activeTetromino = (Transform)Instantiate (tetrominoes [index], transform.position, transform.rotation);
	}

	private void Tap_Updated(GestureRecognizer gesture)
	{
		if (gesture.State == GestureRecognizerState.Ended)
		{
			Vector3 pos = activeTetromino.position;
			if (gesture.FocusX < Screen.width / 2) {
				pos.x -= 1;
			} else {
				pos.x += 1;
			}
			activeTetromino.position = pos;
		}
	}

//	private void Swipe_Updated(GestureRecognizer gesture)
//	{
//		SwipeGestureRecognizer swipe = gesture as SwipeGestureRecognizer;
//		if (swipe.State == GestureRecognizerState.Ended)
//		{
//			float angle = Mathf.Atan2(-swipe.DistanceY, swipe.DistanceX) * Mathf.Rad2Deg;
//			Vector3 pos = activeTetromino.position;
//			if (angle > -90f && angle < 90f) {
//				pos.x += 1;
//			} else {
//				pos.x -= 1;
//			}
//			activeTetromino.position = pos;
//		}
//	}
}
