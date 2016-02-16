using UnityEngine;
using System.Collections.Generic;
using MonsterLove.StateMachine;

public class GameStateBehaviour : StateBehaviour {

	public GameObject TableModel;
	public Transform GameCamera;

	private BeerPongCup hitCup = null;
	private PowerUpRing hitRing = null;
	private BeerPong.PlayerID winnerID = BeerPong.PlayerID.First;
	
	/**
	 * TODO: Dispatch events to opponent via network if current state != CurrentPlayerInactive
	 * Else, listen to opponent events when (currentState == CurrentPlayerInactive)
	 */
	public enum States {

		Init, //To initialize the table with cups, and transit to WaitToThrow if player 1. Else, transit to CurrentPlayerInactive if player 2
		WaitToThrow, //Wait till slider down. If (isTouchDown) ChangeState(RenderTrail)
		InvalidPlayerPosition, //Validate if player moved out
		RenderTrail, //Call Render trail method in MotionController, dispatch trail
		//Trail is defined as a sequence of tuples where each tuple has a position and velocity
		BallReleased, //On update, follow trail with specified velocity
		HitOpponentCup, //(Animation for drinking), Drunkeness Meter update & dispatch, Transit to GameOver / CurrentPlayerInactive
		HitRing, //Expose enter and exit till game logic is implemented
		MissedOpponentCup, //Broadcast to the opponent that this player's turn is over. Transit to CurrentPlayerInactive
		GameOver, //Transit to view 4 by dispatching a OnGameOver (bool didWin) event
		CurrentPlayerInactive, //Listen and apply events over the network : render trail, render ball motion, render ring
		OnHitMyCup	// After hitting the cup, it enters this state to show animation from this player's side. 
					//From here transit to WaitToThrow or GameOver depending the number of cups on current player's side

	}

	void Awake () {
	
		Initialize <States> ();

		//TODO: Once the networking component is completed, uncomment the following 
		//event registration and delete the force invocation of OnPairingComplete();
		//BeerPongNetwork.Instance.OnPairingComplete += OnPairingComplete;
		OnPairingComplete ();
	}

	//This event handler will be registered to BeerPongNetwork component to listen to the pairing event
	private void OnPairingComplete () {

		ChangeState (States.Init);
	}

	private void SetUpCups () {

		//Initialize all cups numbered from 0 to 19, where cups 0 to 9 belong to player 1, and 10 to 19 belong to player 2
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

		BeerPongInput.Instance.OnThrowEnd += HandleOnThrowEnd;

		if (BeerPongNetwork.Instance.thisPlayerID == BeerPong.PlayerID.First) { 

			ChangeState (States.WaitToThrow);

		} else {

			ChangeState (States.CurrentPlayerInactive);
		}

		BeerPongNetwork.Instance.OnOpponentMissedCup += HandleOnOpponentMissedCup;
		BeerPongNetwork.Instance.OnHitMyCup += HandleOnHitMyCup;
	}

	private void HandleOnThrowEnd () {

		if ((States)GetState () == States.RenderTrail) {

			ChangeState (States.BallReleased);

		} else {
			
			Debug.LogError ("Unexpected state. Check if the input is active in an invalid state.");
		}
	}

	private void HandleOnOpponentMissedCup ()
	{
		if ((States)GetState () == States.CurrentPlayerInactive) {

			ChangeState (States.WaitToThrow);

		} else {

			Debug.LogError ("Unexpected opponent state");
		}
	}
	
	private void HandleOnHitMyCup (int cupNumber) {

		if ((States)GetState () == States.CurrentPlayerInactive) {

			hitCup.cupNumber = cupNumber;
			ChangeState (States.OnHitMyCup);

		} else {
			
			Debug.LogError ("Unexpected opponent state");
		}
	}
	
	private void WaitToThrow_Enter () {
		
		BeerPongInput.Instance.SetVisible (true);

		//TODO: Render ball with hand on right side

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

	private bool DidBallGoBelowCupLevel () {

		//TODO: Check the current Y position of the ball and check if it is
		//less than (tableHeight + cupHeight)
		return false;
	}

	private bool DidBallHitOpponentCup () {
		
		//TODO: Return true if the ball crossed the rim of an opponent cup. 
		//TODO: Set the hitCup value if we return true
		return false;
	}
	
	private bool DidBallHitRing () {
		
		//TODO: Return true if the ball crossed any of the active rings 
		//TODO: Set the hitRing value if we return true
		return false;
	}
	
	private void BallReleased_Update () {

		//Assumption: All rings are above cup height
		if (DidBallGoBelowCupLevel ()) {

			ChangeState (States.MissedOpponentCup);

		} else if (DidBallHitOpponentCup ()) {

			ChangeState (States.HitOpponentCup);

		} else if (DidBallHitRing ()) {
			
			ChangeState (States.HitRing);
		}

		//TODO: Call motion controller and follow the motion controller path 
	}

	private bool DidClearCups (BeerPong.PlayerID playerID) {

		//TODO: Check if all of the playerID's cups are cleared
		return false;
	}

	private bool DidAnimateClearingCup () {

		//TODO: Clear the cup with number:hitCupNumber from the this player's cup list
		//TODO: Animate clearing cup in every frame
		//TODO: Update the DifficultyMeter (new name for DrunkennessMeter) 
		//TODO: Return true on completion of animation and difficulty meter updation
		return false;
	}

	private void HitOpponentCup_Enter () {

		BeerPongNetwork.Instance.OnHitOpponentCup (hitCup.cupNumber);

		//TODO: Clear the cup with number:hitCupNumber from the opponent cup list

		if (DidClearCups (BeerPongNetwork.Instance.opponentPlayerID)) {
			
			winnerID = BeerPongNetwork.Instance.thisPlayerID;
			ChangeState (States.GameOver);
			
		} else {
			
			ChangeState (States.CurrentPlayerInactive);
		}
	}
	
	private void HitRing_Enter () {

		//TODO: Trigger the powerup based on the ring type
	}

	private void MissedOpponentCup_Enter () {

		BeerPongNetwork.Instance.OnIMissedCup ();

		ChangeState (States.CurrentPlayerInactive);
	}

	private void OnHitMyCup_Enter() {

		if (DidAnimateClearingCup ()) {

			if (DidClearCups (BeerPongNetwork.Instance.thisPlayerID)) {
				
				winnerID = BeerPongNetwork.Instance.opponentPlayerID;
				ChangeState (States.GameOver);
				
			} else {
				
				ChangeState (States.WaitToThrow);
			}
		}
	}
	
	private void GameOver_Enter () {

		BeerPongInput.Instance.OnThrowEnd -= HandleOnThrowEnd;
		BeerPongNetwork.Instance.OnOpponentMissedCup -= HandleOnOpponentMissedCup;
		BeerPongNetwork.Instance.OnHitMyCup -= HandleOnHitMyCup;
		//TODO: Display the Game Over message on screen that winnerID won

	}

	void OnDestroy () {
		//BeerPongNetwork.Instance.OnPairingComplete -= OnPairingComplete;
	}
}
