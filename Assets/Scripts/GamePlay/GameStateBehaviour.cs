using UnityEngine;
using System.Collections;
using MonsterLove.StateMachine;

public class GameStateBehaviour : StateBehaviour {

	public GameObject TableModel;
	public Transform GameCamera;

	/**
	 * Dispatch events to opponent if current state != CurrentPlayerInactive
	 * Else, listen to opponent events when (currentState == CurrentPlayerInactive)
	 */
	public enum States {

		Init, //To initialize the table with cups, and transit to WaitToThrow if player 1. Else, transit to CurrentPlayerInactive if player 2
		WaitToThrow, //Wait till slider down
					 //If (isTouchDown) ChangeState(RenderTrail)
		InvalidPlayerPosition, //Validate if player moved out
		RenderTrail, //Call Render trail method in MotionController, dispatch trail
		//Trail is defined as a sequence of tuples where each tuple has a position and velocity
		BallReleased, //On update, follow trail with specified velocity
		HitOpponentCup, //(Animation for drinking), Drunkeness Meter update & dispatch, Transit to GameOver / CurrentPlayerInactive
		HitRing, //Expose enter and exit till game logic is implemented
		MissedOpponentCup, //Direct transition to CurrentPlayerInactive
		GameOver, //Transit to view 4
		CurrentPlayerInactive, //Listen and apply events over the network : render trail, render ball motion, render ring
		OnHitMyCup

	}

	void Awake () {
	
		Initialize <States> ();

		ChangeState (States.Init);
	}

	private void SetUpCups () {

		//TODO: Complete this
	}
	
	private void SetUpCamera (BeerPong.PlayerID playerID) {

		//If player 2, rotate the world 180 degree around Y axis, but keep the camera where it already is
		//TODO: Complete this
	}
	
	private void Init_Enter () {

		SetUpCups ();
		SetUpCamera (BeerPongNetwork.Instance.thisPlayerID);
		DrunkennessMeter.Instance.Clear ();
		BeerPongInput.Instance.Reset ();

		BeerPongInput.Instance.OnThrowEnd += OnThrowEnd;

		if (BeerPongNetwork.Instance.thisPlayerID == BeerPong.PlayerID.First) { 

			ChangeState (States.WaitToThrow);

		} else {

			ChangeState (States.CurrentPlayerInactive);
		}
	}

	private void OnThrowEnd () {

		if ((States)GetState () == States.RenderTrail) {

			ChangeState (States.BallReleased);
		}
	}

	private void WaitToThrow_Enter () {
		
		BeerPongInput.Instance.SetVisible (true);

		//Reset only if user is not pressing the button
		if (!BeerPongInput.Instance.isTouchDown) {

			BeerPongInput.Instance.Reset ();
		}
	}

	private bool isUserAtValidPosition {

		get { 
			return GameCamera.position.z < TableModel.GetComponent<Renderer> ().bounds.min.z;
		}
	}

	private void WaitToThrow_Update () {

		if (!isUserAtValidPosition) {

			ChangeState (States.InvalidPlayerPosition);
		}
		
		if (BeerPongInput.Instance.isTouchDown) {
			
			ChangeState (States.RenderTrail);
		}
	}

	private void RenderTrail_Update () {

		if (!isUserAtValidPosition) {
			
			ChangeState (States.InvalidPlayerPosition);
		}
		
		if (BeerPongInput.Instance.isTouchDown) {

			BallMotionController.Instance.RenderTrail (BeerPongInput.Instance.currentPower);
		}
	}

	private void InvalidPlayerPosition_Enter () {

		BeerPongInput.Instance.SetVisible (false);
		//TODO: Show invalid position message, clear trail and ball
	}

	private void InvalidPlayerPosition_Update () {
		
		//TODO: Update invalid position message (if necessary)
	}
	
	private void InvalidPlayerPosition_Exit () {
		
		BeerPongInput.Instance.SetVisible (true);
		//TODO: Clear invalid position message
	}
	
	private void BallReleased_Enter () {
		
		BeerPongInput.Instance.SetVisible (false);
	}

	private void GameOver_Enter () {

		BeerPongInput.Instance.OnThrowEnd -= OnThrowEnd;
	}
}
