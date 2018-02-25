//
//  TicTacToeAgent.cs
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

public class TicTacToeAgent : RLAgent {

	static protected EventPublisherListener pubInstance;



	public TicTacToeAgent( int val ) : base(val) {
		 
		this.val = val;
		BoardGame.Print ( "Assigned Agent " + this.val); 
		this.brain = QLearning.Instance;
		this.futureValDelegate += opponentVal;



	}

	public void opponentVal( out int oppVal ) { oppVal = -1 * val ;}

	override public void play () {

		brain.selectMove(this);
		BoardGame.Print ("Agent " + val + " Col " + TicTacToe.Instance.LastMoveCol + " Row " + TicTacToe.Instance.LastMoveRow);
		//KeyValuePair<IntPair, IntPair> selectedMoves = selectNextMoves( );//Key: best, value:Selected
		//IntPair move = selectedMoves.Value;

	}

	public bool isNotAgent()  { return val == TicTacToe.CrossVal ? false : true; }
	public bool isCrossAgent() { return val == TicTacToe.CrossVal ? true : false; }

	override public bool toMaximiseValues ( bool lookAhead ) {

		if ( !lookAhead ) {
			return isCrossAgent() ? true : false;
		}
		else {

			// When there is lookahead, you are optimising for opponent
			return isCrossAgent() ? false : true;

		}
	}



}
