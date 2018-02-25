//
//  Qlearning.cs
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
using System.Linq;
using System.IO;
using UnityEngine;

public class QLearning : RLBrain {

	private  static QLearning instance;

	RLAgent curPlayer;
	protected Dictionary<IntPair, double> allowedStateValues;

	static protected int DECIMAL_PRECISION = 4;


	private QLearning() { this.allowedStateValues = new Dictionary<IntPair, double>(); }

	public static QLearning Instance
	{
		get 
		{
			if (instance == null)
			{
				instance = new QLearning();
			}
			return instance;
		}
	}

	protected  Dictionary<string, double> Q; // Q function for a particular state_action pair
	protected float epsilon = 0.1f, alpha = 0.5f, gamma = 0.9f;

	public delegate void RewardDelegate ( int winner, out double reward);
	public event RewardDelegate rewardDelegate;



	public void init ( BoardGame bGame, int dim = 3, float epsilon = 0.1f, float alpha = 0.5f, float gamma = 0.9f, string fileName = "TicTacToe.csv") {

		this.game = bGame;
		this.fileName = fileName;
		this.epsilon = epsilon;
		this.alpha = alpha;
		this.gamma = gamma;
		this.Q = new Dictionary<string, double> ();
		this.game = bGame;
		this.dim = dim;



		if ( !game.IsLearning) {

			string filePath = getPath(fileName) ;//+ "/" + fileName;
			string[] lines = File.ReadAllText(filePath).Split('\n');
			game.resetBoard();
			Q.Clear();
			foreach ( string stringPair in lines) {

				if ( !stringPair.Contains(",") ) {
					break;
				}

				string[] stateActionQ = stringPair.Split(',');

				int state = int.Parse(stateActionQ[0]);
				int action = int.Parse(stateActionQ[1]);
				double q = double.Parse(stateActionQ[2]);
				double rounded = System.Math.Round(q, DECIMAL_PRECISION);
				string stateAction = state.ToString() + ',' + action.ToString();

				Q.Add(stateAction, rounded);

			}


		}

		//InitEventTriggers ();


	}

	virtual protected double getQ(IntPair stateActionPair) {

		string key = stateActionPair.X.ToString() + ',' + stateActionPair.Y.ToString();
		//BoardGame.Print ( " State " + state);
		if ( Q.ContainsKey(key)) {
			//BoardGame.Print ( "getV State " + V[state]);
			return Q[key];
		}
		if ( !game.IsLearning) {
			Debug.Log ("No key for " + stateActionPair.X + " " + stateActionPair.Y + " Q Count" + Q.Count );
		}
		// Nothing in the container set default reward
		return 0.0;

	}

	virtual protected void setQ ( IntPair stateActionPair, double q ) {

		string key = stateActionPair.X.ToString() + ',' + stateActionPair.Y.ToString();
		//BoardGame.Print ( " State " + state);
		double rounded = System.Math.Round(q, DECIMAL_PRECISION); 
		if ( Q.ContainsKey(key)) {
			Q[key] = q;
		}
		else {
			Q.Add(key, rounded);
		}
	}


	override public void writeTrainingData () {
		writeQ();
	}

	virtual public string printQ() {

		BoardGame.Print ( Q.Count + " Converged Values:" );

		string valString = "";
		foreach (KeyValuePair <string, double> entry in Q ) {

			double rounded = System.Math.Round(entry.Value, DECIMAL_PRECISION);
			//string [] stateAction = entry.Key.Split(',');
			string entryString = entry.Key + "," + rounded + "\n";
			valString = valString + entryString;
			BoardGame.Print ( entryString );

		}
		//BoardGame.Print ( valString);
		return valString;

	}

	virtual protected void writeQ () {

		string filePath = getPath(fileName);//+ "/" + fileName;
		//BoardGame.Print ( "FilePath " + filePath);
		//BoardGame.Print ( printV());


		StreamWriter outStream = System.IO.File.CreateText(filePath);


		outStream.WriteLine(printQ());
		outStream.Flush();
		outStream.Close();


		#if UNITY_EDITOR
		//UnityEditor.AssetDatabase.Refresh ();
		#endif


	}

	private static string getPath(string fileName){

		return System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
		/*
		#if UNITY_EDITOR
		return Application.dataPath + "/" + "Training";
		#elif UNITY_ANDROID
		return "jar:file://" + Application.dataPath + "!/assets/";
		#elif UNITY_IPHONE
		return Application.dataPath + "/Raw";
		#else
		return  Application.dataPath + "/" + "StreamingAssets";
		#endif
		*/
	}

	private static string GetiPhoneDocumentsPath() {
		string path = Application.dataPath.Substring(0, Application.dataPath.Length - 5);
		path = path.Substring(0, path.LastIndexOf('/'));
		return path + "/Documents";
	}

	override public void selectMove( RLAgent curPlayer) {

		this.curPlayer = curPlayer;

		KeyValuePair<IntPair, IntPair> selectedMoves = selectNextMoves();

		IntPair bestMove = selectedMoves.Key;
		IntPair selectedMove = selectedMoves.Value;
		if ( !selectedMove.isNone() ) {
		    
		    
			game.makeMove (selectedMove.X, selectedMove.Y, curPlayer.Val);
			lastMove.X = selectedMove.X;
			lastMove.Y = selectedMove.Y;
			learn();
		}
		else {
			// StateAction Pair that got us here in the first place.
			int prevState = game.GetState ( lastMove.X, lastMove.Y, true);// switch to previous state before move
			int curState = game.GetState();
			IntPair curStateAction = new IntPair ( prevState, curState );
			double curQVal = getQ(curStateAction );
		    double r = 0.0f;
		    rewardDelegate( game.getWinner(), out r); // This state winner
			double nextMaxQ = getQS(curQVal, r );//getQ(nextBestStateAction);
			updateQ ( curStateAction, curQVal, nextMaxQ, r, alpha);
		}


	}

	
	

	protected double getQS (double qSA, double r ) {

		// Maybe u could think of something better!
		return r;

	}

	protected void updateQ (  IntPair curStateAction, double curQVal, double nextMaxQS, double r, float learningRate  ) {



		 
		// Updating the current state & action which is maximised for future rewards
		double updatedQ = curQVal + learningRate * ( (r + gamma*nextMaxQS) - curQVal );
		string deb = "UpdatedeQS for lastMove " + curQVal + " nextMaxQ " + nextMaxQS + " r " + r + " Updated: " + updatedQ;
		BoardGame.Print ( deb );
		setQ( curStateAction, updatedQ);



	}




	protected void learn() {

		if ( !lastMove.isNone()) {

			// StateAction Pair that got us here in the first place.
			int prevState = game.GetState ( lastMove.X, lastMove.Y, true);// switch to previous state before move
			int curState = game.GetState();
			IntPair curStateAction = new IntPair ( prevState, curState );
			double curQVal = getQ(curStateAction );
			double r = 0.0f;
			rewardDelegate( game.getWinner(), out r); // This state winner
			if ( !(Mathf.Abs((float)r - (0.0f)) < 0.000001f) ) { // There is a winner, so there is no nextBest Move

				double nextMaxQ = getQS(curQVal, r );//getQ(nextBestStateAction);
				updateQ ( curStateAction, curQVal, nextMaxQ, r, alpha);
			}
			else {

				List<IntPair> bestMoves = getOptimalMove (true );
		        
		        // Learn faster, for e.g if a move has two possible next moves where there is a winner
		        float learningRate = alpha ;
		       
				for ( int i = 0; i < bestMoves.Count ; i++) {

					IntPair  nextBest = bestMoves[i]; // as lookAhead is true, learns the consequences of this move
					BoardGame.Print ( " Next optimal Move for opponent " + nextBest.X + " " + nextBest.Y);
		            if ( nextBest.isNone()|| !allowedStateValues.ContainsKey(nextBest) ) {

					 continue;
			        }


					double futureReward =   allowedStateValues[nextBest];
		            double nextMaxQ = getQS(curQVal, futureReward );//getQ(nextBestStateAction);

					updateQ ( curStateAction, curQVal, nextMaxQ, r, learningRate);
		            learningRate *= 1.1f;

				}
			}
		}
		return;
	}

	protected KeyValuePair<IntPair, IntPair> selectNextMoves () {

		List<IntPair> bestMoves = getOptimalMove ();
		if (bestMoves.Count == 0) {
	         return new KeyValuePair<IntPair, IntPair>(new IntPair(), new IntPair());
		}
		IntPair bestMove = bestMoves[Random.Range(0, bestMoves.Count)];


		IntPair selectedMove = bestMove;

		if ( game.IsLearning ) {

			float random = Random.Range(0.0f, 1.0f);
			//BoardGame.Print ( " random " + random);

			if ( random < epsilon) {

				// Pick a random move
				int selIndex = Random.Range(0, allowedStateValues.Count);

				int index = 0;

				foreach ( KeyValuePair <IntPair, double> entry in allowedStateValues ) {

					if ( index == selIndex ) {
						selectedMove = entry.Key;
						break;

					}
					index++;

			    }

			}
		}
		return new KeyValuePair<IntPair, IntPair> ( bestMove, selectedMove );
	}

	protected List<IntPair>  getOptimalMove ( bool lookAhead = false ) {

		Dictionary<int, List<int>> allowedMoves = game.getAllowedMoves();

		allowedStateValues.Clear();
		IntPair stateAction = new IntPair();
		int curState = game.GetState();


		//BoardGame.Print ( "Processing states for Allowed Moves:" );
		foreach ( KeyValuePair < int, List<int >> entry in allowedMoves ) {

			int col = entry.Key;
			List<int> list = entry.Value;
			for ( int j = 0; j<list.Count; j++) {


				int row = list[j];

				IntPair move = new IntPair(col, row);


				if ( lookAhead  ) {
					// Learn here.
					//IntPair stateActionPotentialMove = new IntPair(col, row);
		            double r = 0; 
		            rewardDelegate(game.predictWinner ( col, row, curPlayer.futureVal ()), out r);
					allowedStateValues.Add (move, r); // Note lookAhead maximses for r
		            if ( Mathf.Abs((float)r) > 0.0f ) {
					   BoardGame.Print ( " Allowed " + col + " " + row + " reward " + r);
		            }

				}
				else {

					int statePotentialMove = game.GetState ( col, row, false, curPlayer.Val );
					stateAction.X = curState;
					stateAction.Y = statePotentialMove;
					double stateVal = getQ(stateAction);
					allowedStateValues.Add (move, stateVal);
				}

	        }

		}

	    return getSelectedState ( lookAhead );


	}

	protected List<IntPair> getSelectedState ( bool lookAhead = false ) {

		int index = 0;

		List<IntPair> selectedKeys = new List<IntPair>();



		// Now if u have two statesVals, if we just take the max/min, u will again create a bias as the keys
		// are being ordered as col/row, its always in an ordered loop.

		int maxCount = allowedStateValues.Count - 1;


		int lookingFor = 0;
		// When there is lookahead, you are optimising for yourself, which means u need to check the max value for opponent
		lookingFor = curPlayer.toMaximiseValues(lookAhead) ? 0 : maxCount;


		foreach (KeyValuePair<IntPair, double> entry in allowedStateValues.OrderByDescending(key => key.Value))
		{

			if ( index == lookingFor ) {
			  return getMatchingNext( entry.Key, entry.Value);
			}
			index++;

		}
		return selectedKeys;
	}

	protected List<IntPair> getMatchingNext ( IntPair startKey, double val ) {


		List<IntPair> matchList = new List<IntPair>();


		foreach (KeyValuePair<IntPair, double> entry in allowedStateValues.OrderByDescending(key => key.Value)) {

			if ( Mathf.Abs((float)val - (float)entry.Value) < 0.000001f ) {
		       matchList.Add ( entry.Key );
			} 

		}
		return matchList;
	}



}
