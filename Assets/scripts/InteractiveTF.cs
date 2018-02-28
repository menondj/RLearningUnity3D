//
//  InteractiveTF.cs
//
//  Author:
//       Deepthy Menon <djmenon.menon@gmail.com>
//
//  Copyright (c) 2018 Deepthy Menon
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
