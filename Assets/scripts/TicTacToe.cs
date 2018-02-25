//
//  TicTacToe.cs
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

public class TicTacToe : BoardGame {


	private  static TicTacToe instance;

	protected static int notVal = -1;
	protected static int crossVal = 1;
	protected static int vacantVal = 0;



	public static int NotVal  { get { return notVal; } }
	public  static int CrossVal { get { return crossVal; } }
	public  static int DefVal { get { return vacantVal; } }

	static int randomStart = 0;

	protected TicTacToeStateMachine stateMachine;
	public TicTacToeStateMachine StateMachine {

		get {
			return stateMachine;
		}
	}



	private TicTacToe() {}

	public static TicTacToe Instance
	{
		get 
		{
			if (instance == null)
			{
				instance = new TicTacToe();
			}
			return instance;
		}
	}

	override public void init (bool isLearning, int dim, int defValue, int computerVal) {

		base.init(isLearning, dim, defValue, computerVal);
		this.stateMachine = ScriptableObject.CreateInstance<TicTacToeStateMachine>();
		this.isLearning = isLearning;
		QLearning.Instance.rewardDelegate += reward;
		InitEventTriggers ();


	}

	override public void learn ( int numEpisodes = 10000 ) {

		int numCrossWins = 0, numNotWins = 0, numDraws = 0;

		BoardGame.Print ( " learning ....");
		QLearning brain = QLearning.Instance;
		for ( int i = 0; i < numEpisodes; i++ ) {

			episode ( );
			if ( winner == TicTacToe.CrossVal) {
				numCrossWins++;
			}
			else if ( winner == TicTacToe.NotVal ) {
				numNotWins++;
			}
	 
			else {
				numDraws++;
			}

		}
		brain.writeTrainingData();
		Debug.Log ("Total Episodes " + numEpisodes + " Cum Results: NumCrossWins:" + numCrossWins + " NumNotWins: " + numNotWins + " numDraws: " + numDraws);

	}

	protected void episode () {

		pubInstance.NotifyListeners ("ResetGame");
		base.learn();
		
	}


	public bool defence(int myVal) {


		// You;ve made a valid move but was it a defence?
		int index = GetIndex (lastMoveCol, lastMoveRow);
		int saved = tiles [index];
		tiles [index] = DefVal;

		bool defence = false;
		int sum = sumCol (lastMoveCol, false);
		int sign = (int)Mathf.Sign (sum);

		if (Mathf.Abs (sum) == dim - 1 && sign != myVal && saved == myVal) {
			defence = true; 
		} else {
			sum = sumRow (lastMoveRow, false);
			sign = (int)Mathf.Sign (sum);
			if (Mathf.Abs (sum) == dim - 1 && sign != myVal && saved == myVal) {
				defence = true; 
			} else if (partOfDiagonal (lastMoveCol, lastMoveRow)) {
				 

				if (lastMoveCol == lastMoveRow) {
					sum = sumDiagLR (false);
					sign = (int)Mathf.Sign (sum);
					if (Mathf.Abs (sum) == dim - 1 && sign != myVal && saved == myVal) {
						//Debug.Log (" Sum " + sum + " myVal " + myVal + " lastMoveCol " + lastMoveCol + "lastMoveRow " +  lastMoveRow ); 

						defence = true; 

					}
				} 
				if ( lastMoveCol + lastMoveRow == dim - 1 ) {
					sum = sumDiagRL (false);
					sign = (int)Mathf.Sign (sum);
					if (Mathf.Abs (sum) == dim - 1 && sign != myVal && saved == myVal) {
						defence = true; 
					}

				}
			}
		}
				
		tiles [index] = saved;
		return defence;
	

	}

	public bool missedOpportunity ( out int val, int myVal ) {

		val = 0;
		int index = GetIndex (lastMoveCol, lastMoveRow);
		int saved = tiles [index];
		tiles [index] = DefVal;
		bool missed = false;
		int sum = 0;
		List<int> missedList = new List<int> ();
		for (int i = 0; i < dim; i++) {
			sum = sumCol (i, false);
			if (Mathf.Abs(sum) == dim - 1) {
				missed = true;
				val = (int)Mathf.Sign (sum);
				//Debug.Log ("Potential Win at col " + i + " val " + val ); 
				missedList.Add(val);


			}
			sum = sumRow (i, false);
			if (Mathf.Abs(sum) == dim - 1) {
				missed = true;
				val = (int)Mathf.Sign (sum) ;
				//Debug.Log ("Potential Win at row " + i + " val " + val );
				missedList.Add(val);
			}
		}
		sum = sumDiagLR ( false ); 
		if (Mathf.Abs(sum) == dim - 1) {
			val = (int)Mathf.Sign (sum) ;
			missed = true;
			//Debug.Log ("Potential Win at LR " + val); 
			missedList.Add(val);

		} 
		sum = sumDiagRL (false); 
		if (Mathf.Abs(sum) == dim - 1) {
			val = (int)Mathf.Sign (sum);
			//Debug.Log ("Potential Win at RL " + val); 
			missed = true;
			missedList.Add(val);
		}
		// Give opportunity for myVal.

		for (int i = 0; i < missedList.Count; i++) {
			if (missedList[i] == myVal) {
				val = myVal;
				break;
			}
		}
		if (missed && val != myVal) {
			val = -1 * myVal;
		}
		tiles [index] = saved;
		return missed;
	}



	override public int GetState ( int potentialCol = -1, int potentialRow = -1, bool backTrack = false, int val = 0 ) {

		int k = 0;
		float h = 0f;

		bool potentialMove = potentialCol >= 0 && potentialCol < dim && potentialRow >= 0 && potentialRow < dim ;



		for ( int col = 0; col < dim; col++) {

			int v = 0;

			for ( int row = 0; row < dim; row++) {

				int valueOfIntrest = tiles[BoardGame.GetIndex(col, row)];

				if ( potentialMove  ) {

					if (col == potentialCol && row == potentialRow ) {

						if ( !backTrack ) {
							// Note u need to switch the players as you are calculating the next potential move
							valueOfIntrest = val ;
						}
						else {
							valueOfIntrest = 0;
						}

					}
				}



				switch (valueOfIntrest) {

				case 1:
					v = 1;
					break;

				case -1:
					v = 2;
					break;

				default:
					v = 0;
					break;
				}

				h += Mathf.Pow(3, k) *v;
				k++;
			}
		}

		/*
			if ( potentialMove ) {
				BoardGame.Print ( "Potential Move "+ potentialCol + " " + potentialRow + " h " + (int)Mathf.Ceil(h) );
			}
			*/


		return (int)Mathf.Ceil(h);

	}


	override public Dictionary<int, List<int>> getAllowedMoves () {

		return vacantSpaces;
	}



	override public int checkWinner () {


		int sum = 0;

		// Go thru all cols, see if they add up
		for ( int col = 0; col < dim; col++) {

			sum = 0;
			for ( int row = 0; row < dim; row++) {
				sum += tiles[BoardGame.GetIndex(col, row)];
			}

			if ( sum == dim) {

				return 1;
			}
			if ( sum == -1 * dim) {


				return -1;
			}
		}
		sum = 0;

		// Go thru all cols, see if they add up
		for ( int row = 0; row < dim; row++) {

			sum = 0;
			for ( int col = 0; col < dim; col++) {
				sum += tiles[BoardGame.GetIndex(col, row)];
			}

			if ( sum == dim) {
				return 1;
			}
			if ( sum == -1 * dim) {
				return -1;
			}
		}
		// l_R
		sum = 0;
		for ( int col = dim-1; col >= 0; col--) {

			for ( int row = 0; row < dim ; row++) {
				if ( row == col ) {
					sum += tiles[BoardGame.GetIndex(col, row)];
				}
			}
		}
		if ( sum == dim) {

			return 1;
		}
		if ( sum == -1 * dim) {

			return -1;
		}
		sum = 0;
		//R-L
		for ( int col = dim-1; col >= 0; col--) {

			for ( int row = 0; row < dim ; row++) {
				if ( row == dim - 1 - col ) {
					sum += tiles[BoardGame.GetIndex(col, row)];
				}
			}
		}
		if ( sum == dim) {
			return 1;
		}
		if ( sum == -1 * dim) {
			return -1;
		}
		return 0;
	}


	override public bool isPlayable () {

		if ( winner != 0 || isDraw() ) {

			return false;
		}

		return true;
	}

	override public bool isDraw () {

		if ( winner == 0 ) {

			if ( vacantSpaces.Count == 0 ) {
				return true;
			}

		}
		return false;



	}

	public void postChecks () {

		winner = checkWinner( );
		if ( winner != TicTacToe.DefVal) {
			if ( !isLearning ) {
				BoardGame.Print ( " Winner " + winner );
				pubInstance.NotifyListeners (winner == TicTacToe.CrossVal ? "GameOverCross" : "GameOverNot");
			}
		}
		else if ( !isLearning ){
			if ( !isPlayable() ) {
				BoardGame.Print ( " Draw "  );
				pubInstance.NotifyListeners ("GameOverDraw");
			}
		}


	}

	override public int predictWinner ( int potentialCol, int potentialRow, int val ) {

		int saved = tiles[BoardGame.GetIndex(potentialCol, potentialRow)];

		int index = BoardGame.GetIndex(potentialCol, potentialRow);
		tiles[index] = val;
		int localWinner = checkWinner();
		tiles[index] = saved;

		return localWinner;


	}



	override public bool makeMove ( int i, int j, int val ) {


		BoardGame.Print ( " Current Player "  + val);
		int index = BoardGame.GetIndex(i, j);
		bool moveable = (index >= 0 && index < dim * dim && tiles [index] == vacantVal);
		if (!moveable) {
			return false;
		}
		tiles[index] = val;
		// Remove from vacant space;

		if (vacantSpaces.ContainsKey (i)) {

			vacantSpaces[i].Remove(j);
			if (vacantSpaces[i].Count == 0) {
				vacantSpaces.Remove(i);
			}
		}
		prevMoveCol = lastMoveCol; prevMoveRow = lastMoveRow;
		lastMoveCol = i; lastMoveRow = j;
		//BoardGame.Print  (" LastMoveCol " + lastMoveCol + " LastMoveRow " + lastMoveRow);
		postChecks();
		Players.Instance.advancePlayer ();

		drawBoard();
		pubInstance.NotifyListeners (val == TicTacToe.CrossVal ? "AssignedCross" : "AssignedNot");
		return true;

	}

	public int sumCol ( int colIndex, bool abs = true ) {

		int sum = 0;
		for (int row = 0; row < dim; row++) {
			int index = BoardGame.GetIndex (colIndex, row);
			sum += tiles [index];
		}
		return abs ? Mathf.Abs(sum) : sum;
	}

	public int sumRow ( int rowIndex, bool abs = true ) {

		int sum = 0;
		for (int col = 0; col < dim; col++) {
			int index = BoardGame.GetIndex (col, rowIndex);
			sum += tiles [index];
		}
		return  abs ? Mathf.Abs(sum) : sum;
	}

	public int sumDiagLR ( bool abs = true ) {

		int sum = 0;
		for (int i = 0; i < dim; i++) {
			int index = BoardGame.GetIndex (i, i);
			sum +=  tiles [index];
		}
		return  abs ? Mathf.Abs(sum) : sum;
	}

	public int sumDiagRL ( bool abs = true) {

		int sum = 0;
		for (int i = 0; i < dim; i++) {
			int index = BoardGame.GetIndex (dim-1-i, i);
			sum +=  tiles [index];
		}
		return abs ? Mathf.Abs(sum) : sum ;
	}




	#region Delegate Functions

	// These set of functions keep the policy independant of game .
	static  protected void reward( int winner, out double  r ) {

		r = 0.0;
		// We are dealing with probabilities here, so make it 0 - 1.f
		if ( winner == TicTacToe.CrossVal) {
			r = 1.0;
		}
		else if ( winner ==  TicTacToe.NotVal ) {
			r =  -1.0;
		}

	}




	#endregion

	#region Events

	void TogglePlayer ( params GameObject[] dummy) {

		computerVal *= -1;
		pubInstance.NotifyListeners ("GameOverInteractive");
	}

	void ResetGame ( params GameObject[] dummy) {

		TicTacToe game = TicTacToe.Instance;

		resetBoard ();
		setFirstPlayer ();
		if (game.IsLearning) {

			int col = -1;
			int row = -1;
			GetRowColFromIndex (randomStart++, out col, out row);
			game.makeMove (col, row, game.getCurPlayerVal ());


		} 
		pubInstance.NotifyListeners ("PlayersSet");
		pubInstance.NotifyListeners ("StartGame");

	}

	void InitEventTriggers ( bool register = true ) {

		if ( register ) {
			if ( pubInstance == null ) { 

				pubInstance = EventPublisherListener.Instance;
				if ( pubInstance == null ) {

					Debug.Log ( " Unable to load EventPublisherListener" );

				}

			}

			pubInstance.Register ( "ResetGame", this.ResetGame );
			pubInstance.Register ( "TogglePlayer", this.TogglePlayer );




		}
		else if ( pubInstance != null ) {


			pubInstance.UnRegister ( "ResetGame", this.ResetGame );
			pubInstance.UnRegister ( "TogglePlayer", this.TogglePlayer );


		}


	}

	#endregion




}
