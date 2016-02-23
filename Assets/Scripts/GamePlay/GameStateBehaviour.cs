using UnityEngine;
using System.Collections.Generic;
using MonsterLove.StateMachine;

public class GameStateBehaviour : StateBehaviour {

	public GameObject BeerPongTable;
	public GameObject TableModel;
	public GameObject Ball;
	public GameObject InvalidPlayerPositionText;

	public Vector3 relativeBallStartLocalPosition = new Vector3 (0.13f, 0f, 0.27f);
	public Vector3 tableLocalScale = new Vector3 (0.6096f, 0.7366f, 2.4384f);

	private BeerPongCup hitCup = null;
	private PowerUpRing hitRing = null;
	private BeerPong.PlayerID winnerID = BeerPong.PlayerID.First;

	private Transform gameCameraTransform;
	private Vector3 throwDirection = Vector3.forward;
	private Quaternion beerPongTableDefaultRotation = Quaternion.identity;
	private bool didSetDefaultRotation = false;
	
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
		HitOpponentCup, //(Animation for drinking), Difficulty Meter update & dispatch, Transit to GameOver / CurrentPlayerInactive
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

		InvalidPlayerPositionText.SetActive (false);

		gameCameraTransform = GameObject.Find ("Tango AR Camera").transform;

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

		if (playerID == BeerPong.PlayerID.Second) {

			//Set the default roation, to reset roation on replay
			if (!didSetDefaultRotation) {

				beerPongTableDefaultRotation = BeerPongTable.transform.rotation;
				didSetDefaultRotation = true;
			}

			//Change rotation as needed
			BeerPongTable.transform.rotation = beerPongTableDefaultRotation;
			BeerPongTable.transform.localRotation = Quaternion.Euler(BeerPongTable.transform.localRotation.eulerAngles + Vector3.up * 180f);
		}
	}
	
	private void Init_Enter () {

		SetUpCups ();
		SetUpCamera (BeerPongNetwork.Instance.thisPlayerID);
		DifficultyMeter.Instance.Clear ();
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

	private void RenderBallPosition () {

		//TODO: We might want to slerp on absolute position change
		Ball.transform.position = gameCameraTransform.TransformPoint (relativeBallStartLocalPosition);
	}

	private void RenderBallBeforeThrow () {

		RenderBallPosition ();

		//TODO: Sync the ball across the network
	}

	private bool isUserAtValidPosition {

		get { 
			Vector3 ballLocalPosition = BeerPongTable.transform.InverseTransformPoint (Ball.transform.position);
			return Mathf.Abs (ballLocalPosition.z) > tableLocalScale.z/2;
		}
	}
	
	private void WaitToThrow_Enter () {
		
		BeerPongInput.Instance.SetVisible (true);
		
		RenderBallBeforeThrow ();
		
		//Reset only if user is not pressing the button
		if (!BeerPongInput.Instance.isTouchDown) {
			
			BeerPongInput.Instance.Reset ();
		}
	}

	private void WaitToThrow_Update () {
		
		RenderBallBeforeThrow ();

		if (!isUserAtValidPosition) {

			ChangeState (States.InvalidPlayerPosition);
		}

		if (BeerPongInput.Instance.isTouchDown) {
			
			ChangeState (States.RenderTrail);
		}
	}
	
	private void WaitToThrow_Exit () {
		
		RenderBallBeforeThrow ();
	}

	private void RenderTrail_Enter () {
		
		RenderBallBeforeThrow ();
	}

	private void SetThrowDirection () {

		throwDirection = (gameCameraTransform.position + gameCameraTransform.forward * 5 - Ball.transform.position).normalized;

		//TODO: Wobble the throw direction based on Difficulty Meter level
	}

	private void RenderTrail_Update () {
		
		RenderBallBeforeThrow ();

		if (!isUserAtValidPosition) {
			
			ChangeState (States.InvalidPlayerPosition);
		}

		if (BeerPongInput.Instance.isTouchDown) {

			SetThrowDirection ();
			BallMotionController.Instance.RenderTrail (BeerPongInput.Instance.currentPower, throwDirection);
		}
	}

	private void RenderTrail_Exit () {
		
		RenderBallBeforeThrow ();
	}

	private void InvalidPlayerPosition_Enter () {

		RenderBallBeforeThrow ();

		BallMotionController.Instance.ClearTrail ();

		//Show Invalid Position text
		InvalidPlayerPositionText.SetActive (true);

		BeerPongInput.Instance.SetVisible (false);
	}

	private void InvalidPlayerPosition_Update () {

		RenderBallBeforeThrow ();

		if (isUserAtValidPosition) {

			ChangeState (States.WaitToThrow);
		}
	}
	
	private void InvalidPlayerPosition_Exit () {
		
		BeerPongInput.Instance.SetVisible (true);

		//Clear Invalid Position text
		InvalidPlayerPositionText.SetActive (false);
	}
	
	private bool isBallBelowCupLevel {

		//TODO: Check if the ball slowed down on the table
		get {

			Vector3 ballLocalPosition = BeerPongTable.transform.InverseTransformPoint (Ball.transform.position);
			return ballLocalPosition.y < tableLocalScale.y;
		}
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

	private void BallReleased_Enter () {

		BallMotionController.Instance.ClearTrail ();
		BeerPongInput.Instance.SetVisible (false);

		//TODO: Trace path as suggested by motion controller

		/****** TODO: Clear this DUMMY CODE till MotionController is completed ******/
		const float maxVelocity = 10f;
		Ball.GetComponent<Rigidbody>().velocity = throwDirection.normalized * maxVelocity * BeerPongInput.Instance.currentPower;

		/****** TODO: Clear this DUMMY CODE till MotionController is completed ******/
	}

	private void BallReleased_Update () {

		//Assumption: All rings are above cup height
		if (isBallBelowCupLevel) {

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
		//TODO: Update the DifficultyMeter
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
