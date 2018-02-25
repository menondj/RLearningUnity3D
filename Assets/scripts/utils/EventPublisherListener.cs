// EventPublisher.cs : Base Class for all publishers and Listeners
//   Copyright (C) 2011 Deepthy Menon

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


// Repository of all publishers & listners. To be kept in the root hierarchy
// Any script in the project can make use of this

public class EventPublisherListener : ScriptableObject {

	public  delegate void EventHandler ( params GameObject[] participants );


	private  static EventPublisherListener instance;

	private EventPublisherListener() {}

	public static EventPublisherListener Instance
	{
		get 
		{
			if (instance == null)
			{
				instance = ScriptableObject.CreateInstance<EventPublisherListener>();
			}
			return instance;
		}
	}

	void OnDestroy ( ) {



	}

	public interface Publisher {

		void RegisterListener ( string eventType, EventHandler gameEventHandler );
		void UnRegisterListener( string eventType, EventHandler gameEventHandler );
		void NotifyListeners ( string eventType, params GameObject[] participants );
		void UnRegisterListeners(  );

	} ;


	public class GamePublisher : Publisher {


		public void RegisterListener ( string eventType, EventHandler gameEventHandler ) {

			if ( eventTable.ContainsKey ( eventType ) == false ) {
				//Debug.Log ( "Added Key " + eventType );



				//eventTable.Add ( eventType, new EventHandler ( gameEventHandler ) );
				eventTable.Add ( eventType, gameEventHandler );

			}
			else {



				//eventTable[eventType] += new EventHandler ( gameEventHandler );
				eventTable[eventType] += gameEventHandler ;

			}



		}
		public void UnRegisterListener ( string eventType, EventHandler gameEventHandler ) {


			// TOP BE MODIFIED <IT NOW KILLS EVERY ENTRY MATCHING THE KEY

			if ( eventTable.ContainsKey ( eventType ) ) {

				//	            Debug.Log ( " Unregistering " + eventType );
				//eventTable.Remove ( eventType );
				//eventTable[eventType] -= new EventHandler ( gameEventHandler );
				eventTable[eventType] -= gameEventHandler ;



			}


			//

		}

		public void UnRegisterListeners ( ) {

			//			Debug.Log ( "Unregistering ... " );
			GamePublisher.RemoveAllEntries () ;

		}

		public void NotifyListeners ( string eventType, params GameObject[] participants ) {



			if ( eventTable.ContainsKey ( eventType )  ) {

				eventTable [ eventType ] ( participants );
			}


		}

		public bool isEmpty ( ) {


			return ( eventTable.Count > 0 ? false : true );
		}


		// Only 1 copy throughout.
		static protected Dictionary< string, EventHandler > eventTable = new Dictionary<string, EventHandler> ();

		static public void RemoveAllEntries () {



			while ( eventTable.Count > 0 ) {

				foreach ( KeyValuePair < string, EventHandler > entry in eventTable ) {

					//Debug.Log ( "Removing " + entry.Key );
					//eventTable[entry.Key] -= new EventHandler ( entry.Value );
					eventTable.Remove ( entry.Key );
					break;
				}

			}
			//Debug.Log ( " pubInstance empty " );
			eventTable.Clear ();
		}


	}
	static protected GamePublisher thePublisher = new GamePublisher ();



	public void Register ( string eventType, EventHandler gameEventHandler ) {

		thePublisher.RegisterListener ( eventType,  gameEventHandler );
	}

	public void UnRegister ( string eventType, EventHandler gameEventHandler ) {

		thePublisher.UnRegisterListener ( eventType,  gameEventHandler );
	}

	public void UnRegisterListeners ( ) {

		thePublisher.UnRegisterListeners ( );
	}

	public void NotifyListeners ( string eventType, params GameObject[] participants ) {


		thePublisher.NotifyListeners ( eventType, participants );
	}



	public bool isEmpty ( ) {

		return thePublisher.isEmpty ();

	}

}
