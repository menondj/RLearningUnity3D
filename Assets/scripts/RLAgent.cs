//
//  RLAgent.cs
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


public class BasicAgent {

	protected int val = 1;
	public int Val {
		get
		{
			return val;
		}

	}


	public BasicAgent( int val ) {
		this.val = val;
		Players.Instance.addPlayer (this);
	}

	virtual public void play () { }
}

abstract public class RLAgent  : BasicAgent {


	protected RLBrain brain;



	public RLAgent( int val ) : base(val){ }



	protected delegate void FutureVal ( out int val );
	protected event FutureVal futureValDelegate;

	public int futureVal ( ) {

		int futureVal = 0;
		futureValDelegate ( out futureVal);
		return futureVal;

	}

	abstract public bool toMaximiseValues ( bool lookahead  ) ;

}


