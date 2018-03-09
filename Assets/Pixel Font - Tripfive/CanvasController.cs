using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DigitalRubyShared;

public class CanvasController : MonoBehaviour {

	private int counter = 0;
	private Text start;

	// Use this for initialization
	void Start () {
		start = GameObject.Find ("Start").GetComponent<Text> ();

		TapGestureRecognizer tap = new TapGestureRecognizer();
		tap.StateUpdated += Tap_Updated;
		FingersScript.Instance.AddGesture(tap);
	}
	
	// Update is called once per frame
	void Update () {
		start.gameObject.SetActive (counter < 50);
		counter++;
		if (counter == 100)
			counter = 0;
	}

	private void Tap_Updated(GestureRecognizer gesture) {
		SceneManager.LoadScene ("PlayingField");
	}
}
