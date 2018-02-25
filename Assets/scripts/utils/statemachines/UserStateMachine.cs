//
//  UserStateMachine.cs
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

public class UserStateMachine : StateMachine {
	

#if !UNITY_EDITOR && !UNITY_WEBPLAYER
	
	protected float prevPinchDistance = 0f, initPinchDistance = 0f;
	static protected int maxFramesBeforeDetect = 5;
	protected int numFrames = 0;
	
	static protected float  TouchUpdateInterval  = 1.0f / 60.0f;
	static protected float  TouchLowPassKernelWidthInSeconds  = 1.0f ;
	
	static protected  float  TouchLowPassFilterFactor  = TouchUpdateInterval / TouchLowPassKernelWidthInSeconds; // tweakable
	
	static protected  float touchLowPassValue  = 0f;
	
	static protected float  TouchLowPassFilterDistance ( float curDistance ) {
		
		touchLowPassValue = Mathf.Lerp ( touchLowPassValue, curDistance, TouchLowPassFilterFactor );
		return touchLowPassValue;
	}
	
	static protected Vector3[] screenPoints;
	
#endif
	
	// Use this for initialization
	public UserStateMachine (  ) {
	

		ProcessStateMachine += WaitState;
		
#if !UNITY_EDITOR && !UNITY_WEBPLAYER
		if ( screenPoints == null ) {
			
			screenPoints = new Vector3[10];
		}
#endif
		
	}
	
	
	override protected void Cleanup () {
		
		base.Cleanup ();
	}
	
	
	
	// Update is called once per frame
	virtual protected void WaitState () {
	
		Vector3 screenPoint = Vector3.zero;

		if ( GetScreenPoint ( out screenPoint )) {
			
/*			
#if !UNITY_EDITOR
			
#else
			ProcessStateMachine -= WaitState;
			ProcessStateMachine += DragState;
#endif
*/
			//Debug.Log ( " UserPress " );
			pubInstance.NotifyListeners ( "UserPress" );
			
			
			
		}
		
		
	}
		
	virtual protected void DragState () {
	
		Vector3 screenPoint = Vector3.zero;
		if ( !GetScreenPoint ( out screenPoint )) {
				
			pubInstance.NotifyListeners ( "UserRelease" );
			ProcessStateMachine -= DragState;
			ProcessStateMachine += WaitState;
			
		}
		
		
	}

#if !UNITY_EDITOR && !UNITY_WEBPLAYER
	virtual protected void TouchState () {
	
		
		
		
		switch ( Input.touchCount ) {
			
		case 1:
			
			ProcessStateMachine -= TouchState;
			ProcessStateMachine += DragState;
			break;
			
		case 2:
			
			ProcessStateMachine -= TouchState;
			ProcessStateMachine += PinchStartState;
			prevPinchDistance =  ( Input.GetTouch ( 0 ).position - 
				                   Input.GetTouch ( 0 ).position ).sqrMagnitude;
			initPinchDistance = prevPinchDistance;
			
			break;
			
		default:
			
			ProcessStateMachine -= TouchState;
			ProcessStateMachine += WaitState;
			break;
		}
		
		
	}
	
	virtual protected void PinchStartState () {
		
		if ( Input.touchCount == 2 ) {
			
 
			float curDistance =  ( Input.GetTouch ( 0 ).position - 
				                   Input.GetTouch ( 1 ).position ).sqrMagnitude;
			
			numFrames++;
			if ( numFrames >= maxFramesBeforeDetect ) {
				
					ProcessStateMachine -= PinchStartState;
				
				    if ( touchLowPassValue  <= initPinchDistance ) {
					
						// Either scroll or ZoomIn
						
						if ( Mathf.Abs ( touchLowPassValue - initPinchDistance ) <= 0.0001f ) {
								
							ProcessStateMachine += ZoomInState ;
							
					 	}
						else {
						
							ProcessStateMachine += ZoomInState;
						
						}
					    
					}
					else {
					
						ProcessStateMachine += ZoomOutState;
					
					}
					numFrames = 0; 
						
			}
			else {
				
				TouchLowPassFilterDistance ( curDistance );
				prevPinchDistance = curDistance;
				
			}
		}
		else {
			
			numFrames = 0;
			ProcessStateMachine -= PinchStartState;
			ProcessStateMachine += PinchEndState;
			
		}
		
	}
	
	virtual protected void ZoomOutState () {
		
		if ( Input.touchCount == 2 ) {
			
 
			float curDistance =  ( Input.GetTouch ( 0 ).position - 
				                   Input.GetTouch ( 1 ).position ).sqrMagnitude;
			
			if ( touchLowPassValue < curDistance ) {
				
				// Switch to Zoom in
				ProcessStateMachine -= ZoomOutState;
				ProcessStateMachine += ZoomInState;
			}
			TouchLowPassFilterDistance ( curDistance );
			prevPinchDistance = curDistance;
			
			
		}
		else if ( Input.touchCount <= 0 ) {
			
			Debug.Log (  " Zoooom Out Over ");
			
			ProcessStateMachine -= ZoomOutState;
			ProcessStateMachine += PinchEndState;
			touchLowPassValue = 0f;
			numFrames = 0;
			
			
		}
		
		
	}
	
	virtual protected void ZoomInState () {
		
		if ( Input.touchCount == 2 ) {
			
 
			float curDistance =  ( Input.GetTouch ( 0 ).position - 
				                   Input.GetTouch ( 1 ).position ).sqrMagnitude;
			
			if ( touchLowPassValue > curDistance ) {
				
				// Switch to Zoom Out
				ProcessStateMachine -= ZoomInState;
				ProcessStateMachine += ZoomOutState;
			}
			TouchLowPassFilterDistance ( curDistance );
			prevPinchDistance = curDistance;
			
			
		}
		else if ( Input.touchCount <= 0 ) {
			
			Debug.Log (  " Zoooom In Over ");
			
			ProcessStateMachine -= ZoomInState;
			ProcessStateMachine += PinchEndState;
			touchLowPassValue = 0f;
			numFrames = 0;
		}
		
		
	}
	
	
	
	
	virtual protected void PinchEndState () {
		
		if ( Input.touchCount <= 0 ) {
			
			ProcessStateMachine -= PinchEndState;
			ProcessStateMachine += WaitState;
			
			
		}
		
		
	}
	

	
#endif

#if !UNITY_EDITOR && !UNITY_WEBPLAYER
	
protected bool GetScreenPoints ( out int num ) {
		
	bool selected = false;
	num = 0;

		
	// Check for touch Events 
	
	foreach (Touch touch in Input.touches) {
		
		//Debug.Log ( " Touch received  " +  touch.phase + " Position " +  touch.position );
        if ( ! (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended )) {
            
			selected = true;
			screenPoints[num] = touch.position;
			num++;
				
        }
		
	} 
	
	return selected;
		
	}		
		
#endif
	
	
   	protected bool GetScreenPoint ( out Vector3 screenPoint ) {
	
		bool selected = false;
		screenPoint = Vector3.zero;
		
#if UNITY_IPHONE || UNITY_ANDROID
			
		// Check for touch Events 
		
		foreach (Touch touch in Input.touches) {
			
			//Debug.Log ( " Touch received  " +  touch.phase + " Position " +  touch.position );
	        if ( ! (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended )) {
	            
				selected = true;
				screenPoint = touch.position;
				break;
					
	        }
			
    	} 
		
		
#else
		 if ( Input.GetMouseButton( 0 )  || Input.GetMouseButton ( 1 ) || Input.GetMouseButton ( 2  ) ) {
			
			selected = true;
			screenPoint = Input.mousePosition;

		}
		
#endif	
		
		return selected;
		
	}		
		
		
		
}
