//
//  RLearning.cs
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

public class RLearning : MonoBehaviour {



	public bool ForLearning = true;
	public int NumEpisodes = 70000;

    public bool debug = false;

	public int ComputerSymbol = 1;

	public int Dim = 3;
	protected int dim = 3;
	public float epsilon = 0.1f; 
	public float alpha = 0.5f; 
	public float gamma = 0.9f;





	public Transform BackgroundPlane;
	public Transform TilePrefab;

	public Sprite NotTextureDefault;
	public Sprite CrossTextureDefault;
	public Sprite TileTextureDefault;


	public float Gap = 0f;
	protected  Transform[,] theTiles;

	public  Transform You;

	protected float gap = 0f;



	bool learningInProgress = false;

	protected Vector3 origLocalScale = Vector3.one;

	static protected EventPublisherListener pubInstance;

	TicTacToe game;
	QLearning brain;

	bool cleanup = false;

	TicTacToeAgent notAgent, crossAgent;
	// Use this for initialization
	void Start () {

		BoardGame.setDebug(debug);
		game = TicTacToe.Instance;
		crossAgent  = new TicTacToeAgent(1);
		notAgent  = new TicTacToeAgent(-1);
		game.init(ForLearning, dim, 0, ComputerSymbol );
		brain = QLearning.Instance;
		brain.init (game, dim, epsilon, alpha, gamma);



		if ( !ForLearning ) {



			InitEventTriggers ();

			//player.playTicTacToe(dim, NumEpisodes, epsilon, alpha, gamma);

			SetupTiles ();

			game.StateMachine.Enable ( BackgroundPlane, theTiles, notAgent, crossAgent, BrainType.Player, You );
			// FirstTime
			pubInstance.NotifyListeners ( "ResetGame" ) ;

		}



	}
	
	// Update is called once per frame
	void Update () {



		if ( ForLearning ) {
			if ( Time.frameCount < 20) {
				return;
			}
			if ( !learningInProgress  ) {
				learningInProgress = true;
				game.learn(NumEpisodes);
			}
		}

		else if ( game.StateMachine != null ){

			game.StateMachine.Update ();

		}

		if ( cleanup ) {


			Cleanup ();

			cleanup = false;
		}



		
	}

	void Cleanup () {

		//Debug.Log ( " Before Disable " );
		if ( game.StateMachine != null ) {
			game.StateMachine.Disable ();
			Destroy ( game.StateMachine );

		}
		StopAllCoroutines ();
		// DUNNO WHY THERE IS AN EXCEPTION FOR THIS LINE HAPPENS ONLY FOR THIS GAME!!!
		InitEventTriggers ( false );


		for ( int row = 0; row < dim; row++ ) {
			for ( int col = 0; col < dim; col++ ) { 
				theTiles[row, col] = null;
			}
		}


	}

	#region operations

	void SetupTiles () {

		theTiles = new Transform[dim,dim];

		Vector3 pos = BackgroundPlane.position;
		for ( int col = 0; col < dim; col++ ) {  


			for ( int row = 0; row < dim; row++ ) {    

				Transform tile =  ( Transform ) Instantiate( TilePrefab, pos, Quaternion.identity ); 
				tile.name = "Tile" + "_Col_" + col + "_Row_" + row;            
				tile.parent = transform; 
				theTiles[ col, row ]  = tile.transform;
				theTiles[ col, row ].gameObject.SetActive ( false );
				//pubInstance.NotifyListeners("TileCreated", theTiles[ col, row ].gameObject);
				//Debug.Log ( " Created XForm, X " + theTiles[ col, row ].name );

			}      
		}

		int tMiddle = dim/2;
		//Debug.Log ( "Middle " + dim/2 );
		Transform middle = theTiles[ dim/2, dim/2 ];
		middle.gameObject.SetActive ( true );
		//middle.position = Vector3.zero;


		foreach ( Transform child in middle ) {
			origLocalScale = child.localScale;
			break;
		}


		Vector3 center = middle.GetComponent<Renderer>().bounds.center;
		Vector3 extents = middle.GetComponent<Renderer>().bounds.size/2; 
		 


		foreach ( Transform child in middle ) {
			origLocalScale = child.localScale;
			break;
		}

		// Middle Col
		int dir = 1;// up
		for ( int row = 0; row < dim; row++) {


			if ( row == tMiddle) {
				
				dir = -1;
				pos.y = center.y;
				continue;
			}
			pos.y += dir * extents.y * 2 + gap;
			theTiles[ tMiddle, row ].position = pos;
			theTiles[ tMiddle, row ].gameObject.SetActive ( true );


		}


		// Fill col 1
		pos = theTiles[ tMiddle, 0 ].position;

		pos.x -=  (dim-1) * ( extents.x * 2 + gap );

		for ( int row = 0; row < dim; row++ ) {
			theTiles[ 0, row ].position = pos;
			theTiles[ 0, row ].gameObject.SetActive ( true );
			pos.y -=  extents.y * 2 + gap;
		}


		// Now start filling in col from one end to the other;
		for ( int col = 1; col < dim; col++) {

			pos = theTiles[ col-1, 0 ].position;
			pos.x +=   extents.y * 2 + gap ;
			for ( int row = 0; row < dim; row++ ) {
				theTiles[ col, row ].position = pos;
				theTiles[ col, row ].gameObject.SetActive ( true );
				pos.y -=  extents.y * 2 + gap;
			}


		}



	}





	void ChangeTexture ( GameObject tile, bool not ) {

		foreach ( Transform child in tile.transform ) {

			child.gameObject.SetActive ( true );
			child.GetComponent<SpriteRenderer>().sprite = not ? NotTextureDefault : CrossTextureDefault;
		}

	}

	void ChangeTexture ( Transform xForm, Sprite to, bool overwrite = false ) {

		foreach ( Transform child in xForm ) {

			if ( !overwrite ) {
				child.gameObject.SetActive ( true );
				child.GetComponent<SpriteRenderer>().sprite = to;

			}
			else {
				child.gameObject.SetActive ( false );
			}
		}
		if ( overwrite ) {

			xForm.gameObject.GetComponent<SpriteRenderer>().sprite = to;

		}
	}




	#endregion


	#region Events

	void AssignedCross ( params GameObject[] dummy) {

		GameObject tile = theTiles [game.LastMoveCol, game.LastMoveRow].gameObject;
		// Cross is assigned by the computer, so dont forget to disable the collider!
		tile.GetComponent<Collider2D>().enabled = false;
		ChangeTexture ( tile, false );

	}

	void AssignedNot ( params GameObject[] dummy) {

		GameObject tile = theTiles [game.LastMoveCol, game.LastMoveRow].gameObject;
		// Cross is assigned by the computer, so dont forget to disable the collider!
		tile.GetComponent<Collider2D>().enabled = false;
		ChangeTexture ( tile, true );

	}

	void TogglePlayer ( params GameObject[] menu) {

		Sprite curSprite = menu[0].GetComponent<SpriteRenderer> ().sprite;
		menu[0].GetComponent<SpriteRenderer> ().sprite = curSprite == NotTextureDefault ? CrossTextureDefault : NotTextureDefault;

	}

	void GameOverInteractive ( params GameObject[] dummy) {


		pubInstance.NotifyListeners ("ResetGame");
	}



	void ResetGame ( params GameObject[] dummy) {

//		Debug.Log ( "resetting tiles");

		for ( int i = 0; i < dim; i++ ) {

			for ( int j = 0; j < dim; j++ ) {

				ChangeTexture ( theTiles[i,j], TileTextureDefault, true );
				theTiles[i,j].gameObject.GetComponent<Collider2D>().enabled = true;
			}
		}

	}

	protected void GameOverNot ( params GameObject[] dummy ) {

	


	}

	void InitEventTriggers ( bool register = true ) {

		if ( register ) {
			if ( pubInstance == null ) { 

				pubInstance = EventPublisherListener.Instance;
				if ( pubInstance == null ) {

					Debug.Log ( " Unable to load EventPublisherListener" );

				}

			}

			pubInstance.Register ( "AssignedCross", this.AssignedCross );
			pubInstance.Register ( "AssignedNot", this.AssignedNot );

			pubInstance.Register ( "ResetGame", this.ResetGame );
			pubInstance.Register ( "TogglePlayer", this.TogglePlayer );
			pubInstance.Register ( "GameOverInteractive", this.GameOverInteractive );



		}
		else if ( pubInstance != null ) {


			pubInstance.UnRegister ( "AssignedCross", this.AssignedCross );
			pubInstance.UnRegister ( "AssignedNot", this.AssignedNot );
			pubInstance.UnRegister ( "ResetGame", this.ResetGame );
			pubInstance.UnRegister ( "TogglePlayer", this.TogglePlayer );
			pubInstance.UnRegister ( "GameOverInteractive", this.GameOverInteractive );



		}


	}

	#endregion
}
