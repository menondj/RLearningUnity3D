//
//  StateMachine.cs
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
using UnityEngine;
using System.Collections;


public class StateMachine : ScriptableObject {

	public delegate void StateTransitionDelegate ( );
	public event StateTransitionDelegate ProcessStateMachine;
	
	static protected EventPublisherListener pubInstance;
	
	protected bool cleanup = false;

	
	public StateMachine () {
		
		InitPubInstance ();
		ProcessStateMachine = null;
		
	}
	
	
	virtual protected void Cleanup ( ) {
		
		// Debug.Log (" Removing delegates" );
		
		RemoveDelegates ();
		ProcessStateMachine = null;
		
		
	}
	
	void OnDestroy () {
		
		Cleanup ();
	}
	
	void OnEnable () {
		
		
	}
	
	
	
	protected void RemoveDelegates () {
		
		// Remove whichever current state
		if ( ProcessStateMachine != null ) {
			
			foreach ( StateTransitionDelegate eventDelegate in ProcessStateMachine.GetInvocationList() ) {
	            
				ProcessStateMachine -= eventDelegate;
				
			}
			
		}
	}
	
	public void Register ( string eventName, EventPublisherListener.EventHandler gameEventHandler ) {
		
		pubInstance.Register ( eventName, gameEventHandler );
	
	}
	
	public void UnRegister ( string eventName, EventPublisherListener.EventHandler gameEventHandler ) {
		
		pubInstance.UnRegister ( eventName, gameEventHandler );
	
	}
	
	public void NotifyListeners ( string eventType, params GameObject[] participants ) {
		
		pubInstance.NotifyListeners ( eventType, participants );
		
	}
	
	
	virtual public void Update () { 
		
		if ( ProcessStateMachine != null ) {
			
			ProcessStateMachine (); 
			
		}
	}
	
	protected void InitPubInstance ( ) {
		
		if ( pubInstance == null ) {
		
			pubInstance = EventPublisherListener.Instance;
			
		}
		
	}
	
}
