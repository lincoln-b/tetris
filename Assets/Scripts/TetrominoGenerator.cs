using UnityEngine;
using UnityEngine.UI;
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

	public GameObject gameOverPanel;
	public Text scoreText;

//	public int cameraMin = 10;

//	[Tooltip("Set the required touches for the swipe.")]
//	[Range(1, 10)]
//	public int SwipeTouchCount = 1;

//	[Tooltip("Controls how the swipe gesture ends. See SwipeGestureRecognizerSwipeMode enum for more details.")]
//	public SwipeGestureRecognizerEndMode SwipeMode = SwipeGestureRecognizerEndMode.EndImmediately;

	private int score = 0;
	private int highscore = 0;
	private bool gameOver = false;

	private SwipeGestureRecognizer swipe;

	private GameObject activeTetromino;
	private int counter = 0;

	private enum State { Generating, Dropping };
	private State state = State.Generating;

	private GameObject[,] grid;

	private List<GameObject> allTetrominoes;

	void Start() {
		grid = new GameObject[20,10];
		swipe = new SwipeGestureRecognizer();
		swipe.StateUpdated += Swipe_Updated;
		FingersScript.Instance.AddGesture(swipe);

		TapGestureRecognizer tap = new TapGestureRecognizer();
		tap.StateUpdated += Tap_Updated;
		FingersScript.Instance.AddGesture(tap);

		gameOverPanel.SetActive (false);
		allTetrominoes = new List<GameObject> ();

		highscore = PlayerPrefs.GetInt ("HighScore", 0);
	}

	void FixedUpdate () {

		if (!gameOver) {
			// Generate or drop a block
			if (state == State.Dropping) {
				counter++;
				if (counter >= frameLength) {
					if (IsActiveTetrominoColliding (Vector3.down)) {
						AddActiveTetrominoToGrid ();
						RemoveFullRows ();
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
		}
	}

	void Restart() {
		gameOver = false;
		gameOverPanel.SetActive (false);
		scoreText.gameObject.SetActive (true);
		score = 0;
		for (int i = 0; i < grid.GetLength (0); i++) {
			for (int j = 0; j < grid.GetLength (1); j++) {
				if (grid [i, j] != null) {
					Destroy (grid [i, j].gameObject);
					grid [i, j] = null;
				}
			}
		}
		foreach (GameObject obj in allTetrominoes) {
			Destroy (obj);
		}
		state = State.Generating;
	}

	void GameOver() {
		gameOver = true;
		gameOverPanel.SetActive (true);
		scoreText.gameObject.SetActive (false);
		Text endScore = GameObject.Find ("EndScore").GetComponent<Text>();
		endScore.text = "your score: " + score;
		Text highScoreText = GameObject.Find ("HighScore").GetComponent<Text> ();
		if (score > highscore) {
			highscore = score;
			PlayerPrefs.SetInt ("HighScore", highscore);
		}
		highScoreText.text = "high score: " + highscore;
	}

	void AddActiveTetrominoToGrid() {
		Transform[] transforms = activeTetromino.GetComponentsInChildren<Transform> ();
		foreach (Transform cube in transforms) {
			if (cube.Equals(activeTetromino.transform))
				continue;
			if (cube.position.y >= 19) {
				GameOver ();
				return;
			}
			cube.parent = null;
			int x = Mathf.RoundToInt (cube.position.x);
			int y = Mathf.RoundToInt (cube.position.y);
			grid [y, x] = cube.gameObject;
		}
		Destroy (activeTetromino);
		activeTetromino = null;
	}

	void PrintGrid() {		
		for (int i = 5; i > 0; i--) {
			string line = "";
			for (int j = 0; j < grid.GetLength (1); j++) {
				if (grid [i, j] == null)
					line += "_ ";
				else
					line += "X ";
			}
			Debug.Log (line);
		}
		Debug.Log ("-----------------------");
	}

	void AddToScore(int val) {
		score += val;
		scoreText.text = "score: " + score;
		if (frameLength > 5)
			frameLength -= 1;
	}

	bool ScanAndRemoveOneRow() {
		bool deleted = false;
		for (int i = 0; i < grid.GetLength(0); i++) {
			bool rowContainsNull = false;
			for (int j = 0; j < grid.GetLength (1); j++) {
				if (grid [i,j] == null) {
					rowContainsNull = true;
				}
			}
			if (!rowContainsNull) {
				AddToScore (10);
				for (int j = 0; j < grid.GetLength (1); j++) {
					Destroy (grid [i, j]);
					for (int k = i + 1; k < grid.GetLength (0); k++) {
						GameObject obj  = grid [k, j];

						if (obj != null) {
							Vector3 pos = obj.transform.position;
							pos.y -= 1;
							obj.transform.position = pos;
						}
						grid [k - 1, j] = obj;
					}
				}
				deleted = true;
			}
		}
		return deleted;
	}

	void RemoveFullRows() {
		while (ScanAndRemoveOneRow ())
			;
	}

	bool IsActiveTetrominoColliding(Vector3 direction){
		foreach (Transform cube in activeTetromino.transform) {
			RaycastHit hit;
			if (Physics.Raycast (cube.position, direction, out hit)) {
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
		activeTetromino = Instantiate (tetrominoes[index], transform.position, transform.rotation);
		allTetrominoes.Add (activeTetromino);
	}

	private void Tap_Updated(GestureRecognizer gesture)
	{
		if (gameOver) {
			Restart ();
			return;
		}
		if (gesture.State == GestureRecognizerState.Ended)
		{
			Vector3 pos = activeTetromino.transform.position;
			if (gesture.FocusY > Screen.height * 3 / 4) {
				if (gesture.FocusX < Screen.width * 2 / 3)
					activeTetromino.transform.Rotate (90, 0, 0);
				else
					activeTetromino.transform.Rotate (-90, 0, 0);
			} else if (gesture.FocusY < Screen.height / 6) {
				while (!IsActiveTetrominoColliding(Vector2.down)) {
					pos.y--;
					activeTetromino.transform.position = pos;
				}
			}else if (gesture.FocusX < Screen.width * 2 / 3) {
				Transform[] cubes = activeTetromino.GetComponentsInChildren<Transform> ();
				foreach (Transform cube in cubes) {
					if (cube.transform.position.x == 0)
						return;
				}
				if (!IsActiveTetrominoColliding (Vector3.left))
					pos.x -= 1;
				activeTetromino.transform.position = pos;
			} else {
				Transform[] cubes = activeTetromino.GetComponentsInChildren<Transform> ();
				foreach (Transform cube in cubes) {
					if (cube.transform.position.x >= 9)
						return;
				}
				if (!IsActiveTetrominoColliding (Vector3.right))
					pos.x += 1;
				activeTetromino.transform.position = pos;
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
