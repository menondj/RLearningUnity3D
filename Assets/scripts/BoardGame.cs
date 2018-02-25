//
//  BoardGame.cs
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

public class Players {

	private  static Players instance;

	protected Players() {  thePlayers = new List<BasicAgent> (); curPlayer = -1; }

	protected  List<BasicAgent> thePlayers;

	protected int curPlayer = -1; // Index into list




	public void addPlayer (BasicAgent player) {
		
		thePlayers.Add (player);

	}

	public void removePlayer (int id) {

		for (int i = 0; i < thePlayers.Count; i++) {
			if (thePlayers[i].Val == id) {
				thePlayers.Remove (thePlayers [i]);
				break;
			}

		}

	}

	public void removePlayer ( BasicAgent player) {

		thePlayers.Remove (player);

	}

	public void setFirstPlayer() {

		curPlayer = UnityEngine.Random.Range (0, thePlayers.Count);

	
	}

	public int getCurPlayerVal() {

		if (curPlayer >= 0 && curPlayer < thePlayers.Count) {
			return thePlayers [curPlayer].Val;
		}
		return -1000;

	}

    public void advancePlayer() {
		
		curPlayer++;
		if (curPlayer >= thePlayers.Count) {
			curPlayer = 0;
		}
	}

    virtual public void play() {

		if (curPlayer >= 0 && curPlayer < thePlayers.Count) {
			thePlayers [curPlayer].play ();
		}
	}




	public static Players Instance
	{
		get 
		{
			if (instance == null)
			{
				instance = new Players();

			}
			return instance;
		}
	}


}

public abstract class BoardGame   {

	static protected bool debug = false;

	static public void Print ( string str, bool printanyway = false ) {

		if (  debug || printanyway ) {
			Debug.Log ( str );
		}

	}

	static protected int computerVal = 1;
	static public int Computer { get { return computerVal; } }

	static public void setDebug ( bool isDebug ) {
		BoardGame.debug = isDebug;
	}

	static protected int dim = 0;
	static public int GetIndex ( int col, int row ){
		return row + col * dim;
	}

	static public void GetRowColFromIndex ( int index, out int col, out int row ){

		row = index % dim;
		col = index / dim;
	}

	static protected EventPublisherListener pubInstance;


	
	protected List<int>tiles;
	protected Dictionary<int, List<int>> vacantSpaces;

	protected int defValue = 0;
	protected bool isLearning = true;
	protected int lastMoveCol = -1, lastMoveRow = -1;
	protected int prevMoveCol = -1, prevMoveRow = -1;

	protected int winner = 0;// No winner

	public BoardGame() { 

		InitEventTriggers();
		winner = 0;
		tiles = new List<int> ();
		vacantSpaces = new Dictionary<int, List<int>> ();

	}

	public int Dim {

		get { return dim; }
	}

	public bool IsLearning {

		get { return isLearning; }
	}

	public int LastMoveCol {

		get { return lastMoveCol; }
	}

	public int LastMoveRow {

		get { return lastMoveRow; }
	}

	public int GetVal ( int col, int row ) {

		if (col < dim && row < dim && col >= 0 && row >= 0) {
			return tiles [BoardGame.GetIndex(col, row)];
		}
		return defValue;
	}

	public List<int> GetBoard ()  {
		return tiles;
	}

	public int GetVal ( int index ) {

		if (index >= 0 && index < tiles.Count ) {
			return tiles [index];
		}
		return defValue;
	}

	protected  int Winner {

		get { return winner; }

		set { winner = value; }
	}

	public bool partOfDiagonal ( int col, int row) {
		
		if ((col + row) == dim - 1 || col == row) {
			return true;
		}
		return false;
	}


	#region Operations

	virtual public void init ( bool isLearning, int dim, int defValue, int computerVal  ) {
		

		BoardGame.dim = dim;
		this.defValue = defValue;
		this.isLearning = isLearning;
		BoardGame.computerVal = computerVal;
		resetBoard ();

		/*
		for (int i = 0; i < tiles.Count; i++) {

			int col = -1; int row = -1;

			BoardGame.GetRowColFromIndex (i, out col, out row);
			Debug.Log ( " Index " + i + " col " + col + " row " + row);

		}
		*/


	}

	virtual public void learn() {

		BoardGame.Print (" Starting ... " + getCurPlayerVal ());
		while ( isPlayable ()) {
			
			Players.Instance.play();

		}
	}



	public void drawBoard ( bool printAnyway = false) {

		//return;
		BoardGame.Print ( "Board:", printAnyway);

		for ( int row = 0; row < dim; row++) {
			
			string rowStr = "";
			for ( int col = 0; col < dim; col++) {
				int index = BoardGame.GetIndex (col, row);
				rowStr = rowStr + tiles[index] +  " ";

			}
			BoardGame.Print ( rowStr, printAnyway );

		}
	}


	virtual protected void setFirstPlayer() {

		Players.Instance.setFirstPlayer ();

	}

	public int getCurPlayerVal() {

		return Players.Instance.getCurPlayerVal ();

	}
		
	#region Abstract
	abstract public int GetState ( int potentialCol = -1, int potentialRow = -1, bool backTrack = false, int val = 0 );
	abstract public bool makeMove ( int i, int j, int val );
	abstract public bool isPlayable();
	abstract public bool isDraw();
	abstract public int checkWinner();
	abstract public int predictWinner ( int potentialCol, int potentialRow, int val);
	abstract public void learn(int numEpisodes = 10000);
	#endregion

	public int getWinner() { return winner; }

	public void resetBoard () {
		

		if (vacantSpaces == null ) {

			vacantSpaces = new Dictionary<int, List<int>> ();
		}
		vacantSpaces.Clear();
		tiles.Clear ();
		for ( int i = 0; i < dim; i++) {

			List<int> list = new List<int>();
			for ( int j = 0; j < dim; j++) {

				tiles.Add(defValue);
				list.Add (j);

			}
			vacantSpaces.Add(i, list);
		}
		winner = 0;
		lastMoveCol = -1; lastMoveRow = -1;
		prevMoveCol = -1; prevMoveRow = -1;

	}

	virtual public Dictionary<int, List<int>> getAllowedMoves () {

		return vacantSpaces;
	}

	virtual public int getNumVacantSpaces () {

		int count = 0;
		foreach (KeyValuePair < int, List<int >> entry in vacantSpaces) {

		
			List<int> rows = entry.Value;
			for (int row = 0; row < rows.Count; row++) {
				count++;
			}
		}
		return count;
	}



#endregion

#region Events


	void InitEventTriggers ( bool register = true ) {

		if ( register ) {
			if ( pubInstance == null ) { 

				pubInstance = EventPublisherListener.Instance;
				if ( pubInstance == null ) {

					Debug.Log ( " Unable to load EventPublisherListener" );

				}

			}

			//Write  register functions here, if applicable




		}
		else if ( pubInstance != null ) {

			//Write  Unregister functions here, if applicable
		}


	}

#endregion


}
