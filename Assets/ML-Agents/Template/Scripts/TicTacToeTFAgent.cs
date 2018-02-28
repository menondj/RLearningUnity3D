//
//  TicTacToeTFAgent.cs
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

public class TicTacToeTFAgent : Agent {



	public TextMesh Status;
	public TextMesh Score;


	public int Symbol;
	int myVal;

	int lastStep = -1;

	bool isPaused = true;
	bool toResume = false;

	protected EventPublisherListener pubInstance;

	protected int maxStepInit = 0;

	int scoreCross = 0;
	int scoreNot = 0;
	int scoreDraw = 0;

	protected  int missedOpportunity = 0;
	protected  int missedDefence = 0;


#region override

	public override void InitializeAgent()
	{
		lastStep = -1;
		isPaused = true; 
		toResume = false;
		myVal = Symbol;

		// FirstTime
		InitEventTriggers();
		if (TicTacToe.Instance.IsLearning) {

			maxStepInit = maxStep;
			pubInstance.NotifyListeners ("ResetGame");


		
		} else {
			maxStep = 0;
			Status.gameObject.SetActive (false);

			if (BoardGame.Computer == myVal) {
				pauseStep ();
			} 
		}


	}

	public override List<float> CollectState()
	{
		List<float> state = new List<float>( );


		TicTacToe game = TicTacToe.Instance;
	
		List<int> currentState = game.GetBoard ();

		int curPlayer = game.getCurPlayerVal ();


		bool nearingWin = false;
		int winIndex = -1;
		int sign = 0;

		for (int i = 0; i < currentState.Count; i++) {


			if (currentState [i] == TicTacToe.DefVal) {


				int col = -1; int row = -1;
				BoardGame.GetRowColFromIndex (i, out col, out row);



				int sum = 0;
				if ( col == row ) {
					
					sum = game.sumDiagLR (false);
					if (Mathf.Abs (sum) == game.Dim - 1) {
						sign = (int) Mathf.Sign (sum);
						nearingWin = true;
						winIndex = i;

					} 


				}

				if (col + row == game.Dim - 1) {
					sum = game.sumDiagRL (false);
					if (Mathf.Abs (sum) == game.Dim - 1) {

						if (nearingWin && sign == curPlayer) {
							// Do nothing
						} else {
							sign = (int)Mathf.Sign (sum);
							nearingWin = true;
							winIndex = i;
						}
					} 
				}

				sum = game.sumCol (col, false);
				if (Mathf.Abs (sum) == game.Dim - 1) {

					if (nearingWin && sign == curPlayer) {
						// Do nothing
					} else {
						sign = (int)Mathf.Sign (sum);
						nearingWin = true;
						winIndex = i;
					}

				} 
				sum = game.sumRow (row, false);
				if (Mathf.Abs (sum) == game.Dim - 1) {
					if (nearingWin && sign == curPlayer) {
						// Do nothing
					} else {
						sign = (int)Mathf.Sign (sum);
						nearingWin = true;
						winIndex = i;
					}

				}

			}
				
		}



		int maxVal = currentState.Count;


		for (int i = 0; i < currentState.Count; i++) {


			if (winIndex == i ) {

				int val = maxVal * 100;
				state.Add (val );

			} else {

				if (currentState [i] == TicTacToe.DefVal) {

					int val = maxVal * 10;
					state.Add (val );

				} else {
					state.Add (currentState [i] * 10);
				}
			}

				
		}
		state.Add ((float)(curPlayer));



		if (!game.IsLearning) {
			//drawStates (state);
		}


		return state;
	}

	public override void AgentStep(float[] act)
	{

		TicTacToe game = TicTacToe.Instance;
		 

		if ( !game.isPlayable () || game.getWinner() != TicTacToe.DefVal ) {
			pubInstance.NotifyListeners ("AgentReset");
			return;
		}

		if (!isMyMove ()) { 

			return;
		} 




		bool isValid = false;
		int index = act.Length > 0 ? Mathf.RoundToInt(act [0]) : -1;


		if (index >= 0 && index < game.Dim * game.Dim) {

			if (game.GetVal (index) == TicTacToe.DefVal) {
				int row = -1;
				int col = -1;

				BoardGame.GetRowColFromIndex (index, out col, out row);

				if (game.makeMove (col, row, game.getCurPlayerVal ())) {
					isValid = true;
					lastStep = stepCounter;
					// Each time a valid value occurs, increment maxStep while learning
					if (game.IsLearning) {

						maxStep = stepCounter + maxStepInit;
					}
					postStep ();

					return;

				} 
			}

		} 

		string status = "Action: " + act [0] + " Step " + stepCounter + " " + isValid;
		if (game.IsLearning) {

			Status.text = status;

		} else {

			Debug.Log (status); 
		}



		if (!isValid) {

			reward = -1;
			if (!game.IsLearning) {
				randomMove (); 
				int winner = game.getWinner ();
				if (winner != TicTacToe.DefVal) {
					if (game.IsLearning) {

						Status.text = "Yay! " + myVal + " won!";
						if (myVal == TicTacToe.CrossVal) {
							scoreCross++;
						} else {
							scoreNot++;
						}

					}

				} 
			} else {
				pubInstance.NotifyListeners ("InvalidValue");
			}


		}



	}

	public override void AgentReset()
	{
		if (TicTacToe.Instance.IsLearning) {
			maxStep = maxStepInit;
		}
	}

	public override void AgentOnDone()
	{
		AgentReset ();
		pubInstance.NotifyListeners ("GameOver");

	}

#endregion

#region operations

	protected void pauseStep() {
		StopAllCoroutines ();
		isPaused = true; 
		//Debug.Log ("Pausing"); 
	}

	protected void resumeStep() {

		// For interactive mode introduce a delay before the computer responds.
		if (!TicTacToe.Instance.IsLearning) {
			pauseStep ();
			toResume = true;
			StopAllCoroutines ();
			StartCoroutine(WaitToResume(1.5f));
		}
	}

	// every 2 seconds perform the print()
	private IEnumerator WaitToResume(float waitTime)
	{
		float lWaitTime = waitTime;
		while (true)
		{
			yield return new WaitForSeconds(lWaitTime);
			if (lastStep < stepCounter) {
				toResume = false;
				isPaused = false;
				//Debug.Log ("Resumed"); 
				break;
			}
			lWaitTime *= 0.25f;
		}
	}

	private bool isMyMove() {

		if (TicTacToe.Instance.IsLearning) {
			myVal = TicTacToe.Instance.getCurPlayerVal();
			return true;
		}
		return !isPaused; 
	}

	virtual protected void postStep() {

		TicTacToe game = TicTacToe.Instance;
		int winner = game.getWinner ();
		if (winner != TicTacToe.DefVal) {

			reward = 1;

			// Dont give max if u missed an opportunity before
			if (missedOpportunity != 0 ) {
				if (missedOpportunity != winner) {
					reward *= 0.25f;
				}
			}
			else if (missedDefence != 0 ) {
				if (missedDefence != winner) {
					reward *= 0.25f;
				}
			}

			if (game.IsLearning) {
				
				Status.text = "Yay! " + winner + " won!";
				if (myVal == TicTacToe.CrossVal) {
					scoreCross++;
				} else {
					scoreNot++;
				}

			}


		} else {
			// Has the last step missed a winner?
			// Give -1 because, you did not defend nor see the opportunity of a win.
			if (!game.isPlayable ()) {

				scoreDraw++;
			}
			postMortem ();

		}
		if (game.IsLearning) {
			Score.text = "Wins Cross: " + scoreCross + " Wins Not: " + scoreNot + " Draw: " + scoreDraw;

		}
	}

	
	protected void randomMove() {

		TicTacToe game = TicTacToe.Instance;
		int val = game.getCurPlayerVal ();

		// Check and insert the not if there is going to be a winner.
		if (Mathf.Abs (game.sumDiagLR ()) == game.Dim - 1) {
			// Winner possible
			for ( int j = 0; j < game.Dim; j++ ) {

				if (game.makeMove (j, j, val)) {
					return;
				}
			}

		}
		if (Mathf.Abs (game.sumDiagRL ()) == game.Dim - 1) {
			// Winner possible
			for ( int j = 0; j < game.Dim; j++ ) {

				if (game.makeMove (game.Dim-1-j, j, val)) {
					return;
				}
			}

		}
		for ( int j = 0; j < game.Dim; j++ ) {

			if (Mathf.Abs (game.sumRow (j)) == game.Dim - 1) {
				for ( int k = 0; k < game.Dim; k++ ) {

					if (game.makeMove (k, j, val)) {

						return;
					}
				}
			}
			if (Mathf.Abs (game.sumCol (j)) == game.Dim - 1) {
				for ( int k = 0; k < game.Dim; k++ ) {

					if (game.makeMove (j, k, val)) {
						return;
					}
				}
			}
		}


		// Still proceed
		int col = -1, row = -1;

		Dictionary<int, List<int>> vacantTiles = game.getAllowedMoves ();
		int colIndex = UnityEngine.Random.Range ( 0, vacantTiles.Count);
		int i = 0;
		foreach (KeyValuePair < int, List<int >> entry in vacantTiles) {
			if (i == colIndex) {
				col = entry.Key;
				List<int> list = entry.Value;
				int rowIndex = UnityEngine.Random.Range (0, list.Count);
				row = list [rowIndex];
				if (game.makeMove (col, row, val)) {
					lastStep = stepCounter;
				}
				break;
			}
			i++;
		}




	}

	void resetMe() {

		StopAllCoroutines ();
	}


	int getVacantPos () {

		// Still proceed
		int col = -1, row = -1;
		TicTacToe game = TicTacToe.Instance;
		Dictionary<int, List<int>> vacantTiles = game.getAllowedMoves ();
		int colIndex = UnityEngine.Random.Range ( 0, vacantTiles.Count);
		int i = 0;
		foreach (KeyValuePair < int, List<int >> entry in vacantTiles) {
			if (i == colIndex) {
				col = entry.Key;
				List<int> list = entry.Value;
				int rowIndex = UnityEngine.Random.Range (0, list.Count);
				row = list [rowIndex];
				break;
			}
			i++;
		}
		return BoardGame.GetIndex (col, row);
	}

	// For debug
	void drawStates ( List<float> tiles ) {

		//return;

		// printing has to be done in row
		string rowStr = "Board States: " + TicTacToe.Instance.getCurPlayerVal();
		BoardGame.Print ( rowStr, true);
		rowStr = "";
		int dim = TicTacToe.Instance.Dim;

		for ( int col = 0; col < dim; col++) {


			for ( int row = 0; row < dim; row++) {
				int index = BoardGame.GetIndex (col, row);
				rowStr = rowStr + tiles[index] +  " ";

			}


		}
		BoardGame.Print ( rowStr, true );
	}

	void postMortem () {

		TicTacToe game = TicTacToe.Instance;
		if (!game.IsLearning) {
			// Only interested in the users move
			if (myVal != BoardGame.Computer) {
				return;
			}
		}

		
		int val = 0;
		int winner = game.getWinner ();
		if (winner == TicTacToe.DefVal && game.missedOpportunity (out val, myVal)) {




			bool defence = game.defence (myVal);

			if ((val == myVal ) || (val != myVal && !defence)){
				reward = (val == myVal) ? -0.75f : TicTacToeTFAcademy.DefencePenalty;
				if (!game.isPlayable ()) { // No other choice
					reward = 0;

				}
				if (game.IsLearning ) {
					
					if (val == myVal ) {
						// Catch only the first time u missed an opportunity
						if (missedOpportunity == 0) {
							missedOpportunity = val;
						}
						Status.text = "Missed opportunity by " + myVal + " defence " + defence;
					} else {
						// Catch only the first time u missed a defence
						if (missedDefence == 0) {
							missedDefence = myVal;
						}
						Status.text = "Missed defence by " + myVal ;
					}

				} else {
					if (val == myVal) {
						Debug.Log("Missed opportunity by " + myVal + " defence " + defence);
					} else {
						Debug.Log("Missed defence by " + myVal) ;
					}
				}


			} 
			else if (val != myVal && defence ) {
				
				reward = TicTacToeTFAcademy.DefenceReward;
				if (game.IsLearning) {
					
					Status.text = "Defence by " + myVal;
				} else {
					Debug.Log ("Defence by " + myVal);
				}

			}  
		} 

	}

#endregion


	#region Events

	void AssignedCross ( params GameObject[] dummy) {

		if (!TicTacToe.Instance.IsLearning) {

			if (myVal == TicTacToe.Instance.getCurPlayerVal ()) {
				if (myVal == BoardGame.Computer) {
					resumeStep ();
				} else {
					pauseStep ();
				}
			} else {

				pauseStep ();
			}


		}


	}

	void AssignedNot ( params GameObject[] dummy) {

		if (!TicTacToe.Instance.IsLearning) {

			if (myVal == TicTacToe.Instance.getCurPlayerVal ()) {
				if (myVal == BoardGame.Computer) {
					resumeStep ();
				} else {
					pauseStep ();
				}
			} else {
				
				pauseStep ();
			}

		}
	}


	protected void StartGame ( params GameObject[] dummy ) {


		TicTacToe game = TicTacToe.Instance;
		isPaused = false;
		lastStep = -1;
		toResume = false;
		reward = 0;
		missedOpportunity = 0;
		missedDefence = 0;
		myVal = game.IsLearning ? game.getCurPlayerVal() : BoardGame.Computer;
		if (!game.IsLearning) {

			if (BoardGame.Computer == myVal) {
				if (game.getCurPlayerVal () == myVal) {
					resumeStep ();
				} else {
					pauseStep ();
				}
			} else {
				pauseStep ();

			}
		} 



	}

	protected void ResetAgent ( params GameObject[] dummy ) {

		done = true;
		StopAllCoroutines ();
	

	}



	void InitEventTriggers ( bool register = true ) {

		if ( register ) {
			if ( pubInstance == null ) { 

				pubInstance = EventPublisherListener.Instance;
				if ( pubInstance == null ) {

					Debug.Log ( " Unable to load EventPublisherListener" );

				}

			}
			pubInstance.Register ( "StartGame", this.StartGame );
			pubInstance.Register ( "AssignedCross", this.AssignedCross );
			pubInstance.Register ( "AssignedNot", this.AssignedNot );
			pubInstance.Register ( "AgentReset", this.ResetAgent );





		}
		else if ( pubInstance != null ) {

			pubInstance.UnRegister ( "StartGame", this.StartGame );
			pubInstance.UnRegister ( "AssignedCross", this.AssignedCross );
			pubInstance.UnRegister ( "AssignedNot", this.AssignedNot );
			pubInstance.UnRegister ( "AgentReset", this.ResetAgent );





		}


	}

#endregion
}
