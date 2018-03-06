using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DigitalRubyShared;
using System.Linq;

public class TetrominoGenerator : MonoBehaviour {

	public GameObject straight;
	public GameObject square;
	public GameObject tee;
	public GameObject rightDog;
	public GameObject leftDog;
	public GameObject rightElbow;
	public GameObject leftElbow;

	public int frameLength = 100;
//	public int cameraMin = 10;

//	[Tooltip("Set the required touches for the swipe.")]
//	[Range(1, 10)]
//	public int SwipeTouchCount = 1;

//	[Tooltip("Controls how the swipe gesture ends. See SwipeGestureRecognizerSwipeMode enum for more details.")]
//	public SwipeGestureRecognizerEndMode SwipeMode = SwipeGestureRecognizerEndMode.EndImmediately;

	private SwipeGestureRecognizer swipe;

	private GameObject activeTetromino;
	private int counter = 0;

	private enum State { Generating, Dropping };
	private State state = State.Generating;

	void Start() {
		swipe = new SwipeGestureRecognizer();
		swipe.StateUpdated += Swipe_Updated;
		FingersScript.Instance.AddGesture(swipe);

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
				if (IsActiveTetrominoColliding ()) {
					state = State.Generating;
				} else {
					counter = 0;
					Vector3 pos = activeTetromino.transform.position;
					pos.y -= 1;
					activeTetromino.transform.position = pos;
				}
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
	}

	bool IsActiveTetrominoColliding(){
		foreach (Transform cube in activeTetromino.transform) {
			RaycastHit hit;
			if (Physics.Raycast (cube.position, Vector3.down, out hit)) {
				if (hit.distance <= 1.0f && !hit.transform.IsChildOf(activeTetromino.transform))
					return true;
			}
		}
		return false;
	}

	void GenerateTetromino() {
		System.Random rnd = new System.Random ();
		int index = rnd.Next (0, 7);
		GameObject[] tetrominoes = { straight, square, tee, rightDog, leftDog, rightElbow, leftElbow };
		activeTetromino = Instantiate (tetrominoes [index], transform.position, transform.rotation);
	}

	private void Tap_Updated(GestureRecognizer gesture)
	{
		if (gesture.State == GestureRecognizerState.Ended)
		{
			Vector3 pos = activeTetromino.transform.position;
			if (gesture.FocusX < Screen.width / 2) {
				pos.x -= 1;
			} else {
				pos.x += 1;
			}
			activeTetromino.transform.position = pos;
		}
	}

	private void Swipe_Updated(GestureRecognizer gesture)
	{
		SwipeGestureRecognizer swipe = gesture as SwipeGestureRecognizer;
		if (swipe.State == GestureRecognizerState.Ended)
		{
			float angle = Mathf.Atan2(-swipe.DistanceY, swipe.DistanceX) * Mathf.Rad2Deg;
			if (Mathf.Abs (angle) >= 90.0f)
				activeTetromino.transform.Rotate(90, 0, 0);
			else
				activeTetromino.transform.Rotate(-90, 0, 0);
		}
	}
}
