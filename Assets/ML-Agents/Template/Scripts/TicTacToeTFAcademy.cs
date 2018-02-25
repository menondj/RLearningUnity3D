using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TicTacToeTFAcademy : Academy {



	public Transform BackgroundPlane;
	public Transform TilePrefab;

	public Sprite NotTextureDefault;
	public Sprite CrossTextureDefault;
	public Sprite TileTextureDefault;

	public int ComputerSymbol = 1; // Cross, change to -1 if u want the computer to be Not.
	public Transform You;

	public int dim = 3;

	public float gap = 0f;
	protected  Transform[,] theTiles;
	protected Vector3 origLocalScale = Vector3.one;

	protected EventPublisherListener pubInstance;



	static protected float defencePenalty = -0.25f;
	public static float DefencePenalty { get { return defencePenalty; } }

	static protected float defenceReward = 0.25f;
	public static float DefenceReward { get { return defenceReward; } }

	static protected bool missedOpportunity = false;
	static protected bool invalidValue = false;
	public static bool MissedWin { get { return missedOpportunity; } }
	public static bool InvalidError { get { return invalidValue; } }

	int stepDecr = 0;




#region
	public override void InitializeAcademy()
	{
		InitEventTriggers ();

		TicTacToe game = TicTacToe.Instance;
		Brain[] brains = GetComponentsInChildren<Brain>();
		bool training = true;
		BasicAgent cross = new BasicAgent (1);
		BasicAgent not = new BasicAgent (-1);

		if (brains [0].brainType == BrainType.Internal) {
			training = false;

		}
		game.init (training, dim, 0, ComputerSymbol);

		if (!training) {
			
			SetupTiles ();
			You.gameObject.SetActive (true);
			game.StateMachine.Enable (BackgroundPlane, theTiles, not, cross, BrainType.Internal, You);

		} else {
			You.gameObject.SetActive (false);
		}

	}

	public override void AcademyReset()
	{
		
		defencePenalty = (float)resetParameters ["defence_penalty"];
		defenceReward = (float)resetParameters ["defence_reward"];
		StopAllCoroutines ();
		//Debug.Log (" Current Step " + currentStep); 
		pubInstance.NotifyListeners ("ResetGame");

	}

	public override void AcademyStep()
	{
		
		TicTacToe game = TicTacToe.Instance;
		if (game.getWinner () != TicTacToe.DefVal || !game.isPlayable()) {
			done = true;
		}

	}




#endregion

#region Operations

	void SetupTiles () {

		theTiles = new Transform[dim,dim];

		Vector3 pos = BackgroundPlane.position;
		for ( int col = 0; col < dim; col++ ) {  


			for ( int row = 0; row < dim; row++ ) {    

				Transform tile =  ( Transform ) Instantiate( TilePrefab, pos, Quaternion.identity ); 
				tile.name = "Tile" + "_Col_" + col + "_Row_" + row;            
				tile.parent = BackgroundPlane; 
				theTiles[ col, row ]  = tile.transform;
				theTiles[ col, row ].gameObject.SetActive ( true );
				theTiles[ col, row ].GetComponent<Collider2D>().enabled = true;
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

	private IEnumerator WaitToResume(float waitTime)
	{
		yield return new WaitForSeconds(waitTime);
		done = true;
	}


#endregion

#region Events


	void AssignedCross ( params GameObject[] dummy) {

		TicTacToe game = TicTacToe.Instance;
		if (game.IsLearning) {
			return;
		} 
		 
		GameObject tile = theTiles [game.LastMoveCol, game.LastMoveRow].gameObject;
		// Cross is assigned by the computer, so dont forget to disable the collider!
		tile.GetComponent<Collider2D>().enabled = false;
		ChangeTexture ( tile, false );


	}

	void AssignedNot ( params GameObject[] dummy ) {

		TicTacToe game = TicTacToe.Instance;
		if (game.IsLearning) {
			return;
		}
		GameObject tile = theTiles [game.LastMoveCol, game.LastMoveRow].gameObject;
		tile.GetComponent<Collider2D>().enabled = false;
		ChangeTexture ( tile, true );

	}



	void ResetGame ( params GameObject[] dummy) {


		//Debug.Log ("reset game"); 
		TicTacToe game = TicTacToe.Instance;
		if (game.IsLearning) {
			return;
		}

		for ( int i = 0; i < dim; i++ ) {

			for ( int j = 0; j < dim; j++ ) {

				ChangeTexture ( theTiles[i,j], TileTextureDefault, true );
				theTiles[i,j].gameObject.GetComponent<Collider2D>().enabled = true;
			}
		}




	}



	void GameOverInteractive ( params GameObject[] dummy) {

		done = true;

	}



	void PlayersSet ( params GameObject[] dummy) {

		missedOpportunity = false;
		invalidValue = false;

	}

	void InvalidValue ( params GameObject[] dummy) {
		invalidValue = true; 
		done = true;
	}

	void MissedOpportunity ( params GameObject[] dummy) {
		missedOpportunity = true;
		done = true;
	}


	void TogglePlayer ( params GameObject[] menu) {

		//Debug.Log ("Toggle Player"); 

		Sprite curSprite = menu[0].GetComponent<SpriteRenderer> ().sprite;
		menu[0].GetComponent<SpriteRenderer> ().sprite = curSprite == NotTextureDefault ? CrossTextureDefault : NotTextureDefault;

	}


	void GameOver ( params GameObject[] dummy) {
		done = true;
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
			pubInstance.Register ( "GameOverInteractive", this.GameOverInteractive );
			pubInstance.Register ( "ResetGame", this.ResetGame );
			pubInstance.Register ( "PlayersSet", this.PlayersSet );
			pubInstance.Register ( "TogglePlayer", this.TogglePlayer );
			pubInstance.Register ( "InvalidValue", this.InvalidValue);
			pubInstance.Register ( "MissedOpportunity", this.MissedOpportunity );
			pubInstance.Register ( "AgentReset", this.GameOver );
			pubInstance.Register ( "GameOver", this.GameOver );


		}
		else if ( pubInstance != null ) {

			pubInstance.UnRegister ( "AssignedCross", this.AssignedCross );
			pubInstance.UnRegister ( "AssignedNot", this.AssignedNot );
			pubInstance.UnRegister ( "GameOverInteractive", this.GameOverInteractive );
			pubInstance.UnRegister ( "ResetGame", this.ResetGame );
			pubInstance.UnRegister ( "PlayersSet", this.PlayersSet );
			pubInstance.UnRegister ( "TogglePlayer", this.TogglePlayer );
			pubInstance.UnRegister ( "InvalidValue", this.InvalidValue);
			pubInstance.UnRegister ( "MissedOpportunity", this.MissedOpportunity );
			pubInstance.UnRegister ( "AgentReset", this.GameOver );
			pubInstance.UnRegister ( "GameOver", this.GameOver );



		}


	}
#endregion

}
