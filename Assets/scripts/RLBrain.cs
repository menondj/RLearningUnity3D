//
//  RLBrain.cs
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

abstract public class RLBrain  {


	public class IntPair {

		protected int x = -1, y = 1;

		public IntPair ( int x=-1, int y=-1) {

			this.x = x;
			this.y = y;

		}
		public int X
		{
			get
			{
				return x;
			}
			set
			{
				x = value;
			}
		}

		public int Y
		{
			get
			{
				return y;
			}
			set
			{
				y = value;
			}
		}



		public bool isNone () { return x == -1 || y == -1;}
	}
	protected  string fileName;
	protected BoardGame game;
	protected int dim = 3;

	protected IntPair lastMove ;

	public RLBrain ( ) {

		lastMove = new IntPair(); 

	}

	abstract public void writeTrainingData () ;
	abstract public void selectMove(RLAgent curPlayer);




}
