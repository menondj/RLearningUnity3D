using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveTF : MonoBehaviour {

	TicTacToe game;
	// Use this for initialization
	void Start () {

		game = TicTacToe.Instance;
	}
	
	// Update is called once per frame
	void Update () {

		if (game.IsLearning) {
			return;
		}
		else if ( Time.frameCount > 10 && game.StateMachine != null) {
			game.StateMachine.Update ();
		}
		
	}
}
