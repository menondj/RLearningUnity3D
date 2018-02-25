//
//  TicTacToeStateMachine.cs
//
//  Author:
//       Deepthy Menon <djmenon.menon@gmail.com>
//
//  Copyright (c) 2017 Deepthy Menon
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_TENSORFLOW
using TensorFlow;
#endif

public class TicTacToeStateMachine : UserStateMachine {

#region init


	Transform[,] theTiles;
	int dim;
	Transform menu;

	bool player = true;
	bool winner = false;
	Transform board;

	float timeStart = 0f;


	TicTacToe game;

	BrainType brainType;

	BasicAgent notAgent;
	BasicAgent crossAgent;

	BasicAgent myAgent;

	int lastChange = 0;

	int scoreCross = 0;
	int scoreNot = 0;
	int scoreDraw = 0;

	SpriteRenderer spriteRenderer = null;
	int toggleCount = 0;



	public TicTacToeStateMachine ( ) {


	}

	public void Enable ( Transform board, Transform[,] tiles,  BasicAgent notAgent, BasicAgent crossAgent, BrainType brainType = BrainType.Player, Transform menu = null, int dim = 3 ) {

		cleanup = false;
	

		theTiles = tiles;
		this.dim = dim;
		this.board = board;
		this.brainType = brainType;
		this.notAgent = notAgent;
		this.crossAgent = crossAgent;

		this.menu = menu;
		//Debug.Log ( "MyAgent val " + myAgent.Val); 

		InitEventTriggers ();
		winner = false;
		game = TicTacToe.Instance;
		this.myAgent = BoardGame.Computer == notAgent.Val ? notAgent : crossAgent;
		if (menu != null) {

			menu.gameObject.SetActive (true);
			spriteRenderer = menu.GetComponent<SpriteRenderer> ();

		}



	}

	public void Disable () {

		Debug.Log ( "Disabling State Machine ...");
		Cleanup();
		InitEventTriggers ( false );
	}



	protected void blink () {
		if (spriteRenderer != null && toggleCount >= 0) {
			toggleCount++;
			if (toggleCount > 10) {
				spriteRenderer.enabled = !spriteRenderer.enabled;
				toggleCount = 0;
			}
		}
	}


#endregion

#region States

	override protected void WaitState () {

		if ( winner ) {
			
			WaitForSometime ();
			return;
		}
		blink ();
		base.WaitState ();



	}

	protected void LimboState () {
	}

	protected void WaitForComputerMove () {

		if ( winner ) {
			//Debug.Log ( "returning");
			WaitForSometime ();
			return;
		}

		timeStart += Time.deltaTime;
		//Debug.Log ( "WaitForComputerMove");
		if ( timeStart > 1f ) {


			timeStart = 0f;
			RemoveDelegates ();
			// Make the computer Move.
			if (brainType == BrainType.Player) {

				myAgent.play ();
				bool valid = (game.LastMoveCol >= 0 && game.LastMoveCol < dim && game.LastMoveRow >= 0 && game.LastMoveRow < dim);
				if (valid) {

					//pubInstance.NotifyListeners (myAgent.Val == TicTacToe.CrossVal ? "AssignedCross" : "AssignedNot", theTiles [game.LastMoveCol, game.LastMoveRow].gameObject);
					ProcessStateMachine += WaitState;
					player = true;

				} else {
					// Somethings wrong
					Debug.Log ("Somethings wrong " + game.LastMoveCol + " " + game.LastMoveRow);
					ProcessStateMachine += LimboState;

				}

			} else if (brainType == BrainType.Internal) {
				// Brain Type == Internal

				player = false;
				ProcessStateMachine += WaitState;

			}

		}

	}

	protected void WaitForReset () {

		//Debug.Log ( "WaitForReset");
	
		timeStart += Time.deltaTime;

		//Debug.Log ( "WaitForREset State" + timeStart);
		if ( timeStart > 2f ) {
//			Debug.Log ( "Notifying ResetGame");
			timeStart = 0f;
			RemoveDelegates ();
//			Debug.Log ( "Notifying ResetGame");
			if (brainType == BrainType.Player) {
				pubInstance.NotifyListeners ("ResetGame");
			}
			else if (brainType == BrainType.Internal) {
				// This case the academy will send a ResetGame notification.
				//pubInstance.NotifyListeners ("GameOverInteractive");
			}


		}


	}






		
#endregion


#region Operation

bool CheckSelection ( Vector3 screenPoint ) {

		Vector3 wp = Camera.main.ScreenToWorldPoint(screenPoint);
		Vector2 touchPos = new Vector2(wp.x, wp.y);
		int layerMask = 1 << board.gameObject.layer;

		Collider2D collider2D = Physics2D.OverlapPoint(touchPos);

		if (collider2D != null ) {

			if (menu != null && collider2D.transform == menu) {
				pubInstance.NotifyListeners ("TogglePlayer", menu.gameObject);
				lastChange = Time.frameCount;
				return false;
			}

			for ( int i = 0; i < dim; i++ ) {

				for ( int j = 0; j < dim; j++ ) {

					if (theTiles[i,j] == collider2D.gameObject.transform ) {

						//Debug.Log ( "Touched " + theTiles[i,j].name + " Col " + i +  " Row " + j );

						game.makeMove(i,j, game.getCurPlayerVal());
						//pubInstance.NotifyListeners (BoardGame.Computer == TicTacToe.CrossVal ? "AssignedCross" : "AssignedNot" );
						theTiles[i,j].gameObject.GetComponent<Collider2D>().enabled = false;
						//Debug.Log ( " New Val " + tileState[i, j] );
						lastChange = Time.frameCount;
						if (spriteRenderer != null) {
							spriteRenderer.enabled = true;
							toggleCount = -1000;
						}

						return true;

					}
				}
			}

			return true;
		}


		return false;

	}

	protected void WaitForSometime () {


		player = false;
		RemoveDelegates ();
		timeStart = 0f;
//		Debug.Log ( "Waiting for reset");
		ProcessStateMachine += WaitForReset;

		if (menu != null) {

			TextMesh[] textMeshs = menu.GetComponentsInChildren<TextMesh>();

			foreach (TextMesh textMesh in textMeshs) {

				if (textMesh.name.Contains ("Score")) {

					string scoreText = "Cross Wins: " + scoreCross + " Not Wins: " + scoreNot + " Draws: " + scoreDraw;
					textMesh.text = scoreText;
					break;
				}
			}
		}

	}


#endregion


#region Events

	void TogglePlayer ( params GameObject[] dummy ) {

		scoreNot = scoreCross = scoreDraw = 0;
		WaitForSometime ();

	}

	void StartGame ( params GameObject[] dummy ) { 

		 
		int curPlayer = game.getCurPlayerVal ();
		Debug.Log ("Starting Game " + curPlayer );
		RemoveDelegates ();
		timeStart = 0f;
		this.myAgent = BoardGame.Computer == notAgent.Val ? notAgent : crossAgent;
		if (curPlayer == BoardGame.Computer) {

			ProcessStateMachine += WaitForComputerMove;
			player = false;
			toggleCount = -1000;


			//Debug.Log ("WaitForComputerMove");
		} else {
			//Debug.Log ("WaitState"); 
			ProcessStateMachine += WaitState;
			player = true;
			toggleCount = 0;

		}

		winner = false;
	}

	void UserPress ( params GameObject[] dummy ) { 

		//Debug.Log ( " UserPress " );
		if ( !player || winner ) {
			//Debug.Log ( "user press returning ");
			return;
		}
		if (Time.frameCount - lastChange < 10) {
			return;
		}
		Vector3 screenPoint = Vector3.zero;
		if ( GetScreenPoint ( out screenPoint )) {

			//Debug.Log ( " UserPress " + screenPoint );
			if ( CheckSelection ( screenPoint ) ) {
				player = false;


			}
		}

	}

	void AssignedCross ( params GameObject[] dummy ) {

		//Debug.Log ( "recvd assigned cross " + winner);
		if ( winner) {
			return;
		}
		RemoveDelegates ();
		timeStart = 0f;

		if (game.getCurPlayerVal() != BoardGame.Computer) {
			ProcessStateMachine += WaitState;
			player = true;
		} else {
			player = false;
			ProcessStateMachine += WaitForComputerMove;
		}




	}

	void AssignedNot ( params GameObject[] dummy ) {


		if ( winner) {
			return;
		}
		RemoveDelegates ();
		timeStart = 0f;
		if (game.getCurPlayerVal() != BoardGame.Computer) {
			ProcessStateMachine += WaitState;
			player = true;
		} else {
			player = false;
			ProcessStateMachine += WaitForComputerMove;
		}

	}
		

	void ResetGame ( params GameObject[] dummy) {

	}

	void GameOverCross ( params GameObject[] dummy) {

		Debug.Log ( "Game Over Cross");

		winner = true;
		scoreCross++;
		WaitForSometime ();



	}

	void GameOverNot ( params GameObject[] dummy) {


		Debug.Log ( "Game Over Not");
		winner = true;
		scoreNot++;
		WaitForSometime ();


	}

	void GameOverDraw ( params GameObject[] dummy) {

		Debug.Log ( "Game Over Draw");
		winner = true;
		scoreDraw++;

		WaitForSometime ();
	}



	void InitEventTriggers ( bool register = true ) {

		if ( register ) {
			if ( pubInstance == null ) { 

				pubInstance = EventPublisherListener.Instance;
				if ( pubInstance == null ) {

					Debug.LogError ( " Unable to load EventPublisherListener" );

				}

			}

			pubInstance.Register ( "StartGame", this.StartGame );
			pubInstance.Register ( "UserPress", this.UserPress );
			pubInstance.Register ( "AssignedCross", this.AssignedCross );
			pubInstance.Register ( "AssignedNot", this.AssignedNot );
			pubInstance.Register ( "GameOverNot", this.GameOverNot );
			pubInstance.Register ( "GameOverCross", this.GameOverCross );
			pubInstance.Register ( "GameOverDraw", this.GameOverDraw );
			pubInstance.Register ( "TogglePlayer", this.TogglePlayer );



		}
		else if ( pubInstance != null ) {

			pubInstance.UnRegister ( "StartGame", this.StartGame );
			pubInstance.UnRegister ( "UserPress", this.UserPress );
			pubInstance.UnRegister ( "AssignedCross", this.AssignedCross );
			pubInstance.UnRegister ( "AssignedNot", this.AssignedNot );
			pubInstance.UnRegister ( "GameOverNot", this.GameOverNot );
			pubInstance.UnRegister ( "GameOverCross", this.GameOverCross );
			pubInstance.UnRegister ( "GameOverDraw", this.GameOverDraw );
			pubInstance.UnRegister ( "TogglePlayer", this.TogglePlayer );


		}


	}

#endregion
}
