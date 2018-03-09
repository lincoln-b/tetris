using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomTetrominoGenerator : MonoBehaviour {

	public GameObject straight;
	public GameObject square;
	public GameObject tee;
	public GameObject rightDog;
	public GameObject leftDog;
	public GameObject rightElbow;
	public GameObject leftElbow;

	private int state = 0;
	private int counter = 0;

	void Start () {
//		for (int i = 0; i < 7; i++) {
//			Generate (i);
//		}
	}

	void Generate(int i) {
		System.Random rnd = new System.Random ();
		GameObject[] tetrominoes = { straight, square, tee, rightDog, leftDog, rightElbow, leftElbow };
		Vector3 pos = new Vector3 ((float) rnd.NextDouble () * 6f - 3f, (float) rnd.NextDouble () * 10f - 5f, 0);
		GameObject tetromino = Instantiate (tetrominoes[i], pos, Random.rotation);
		foreach (Rigidbody rb in tetromino.GetComponentsInChildren<Rigidbody>()) {
			rb.isKinematic = false;
		}
	}

	void FixedUpdate () {
		if (counter == 0) {
			Generate (state);
			state++;
			if (state >= 7)
				state = 0;
		}
		counter++;
		if (counter >= 30) {
			counter = 0;
		}
	}
}
