using UnityEngine;
using System.Collections.Generic;
using MonsterLove.StateMachine;

public class GameStateBehaviour : StateBehaviour {
	
	public GameObject BoardwalkPong;
	public GameObject TableModel;
	public GameObject Ball;

	public GameObject InvalidPlayerPositionText;
	public GameObject YouWonText;
	public GameObject YouLoseText;
	public GameObject ReplayButton;

	public GameObject cupPrefab;
	public GameObject ringPrefab;
	public GameObject obstaclePrefab;
	public GameObject ringBallPrefab;

	private int playerCupCount = 10;
	private Dictionary<int,GameObject> dictCup;
	private List<GameObject> playRoundObject = new List<GameObject> ();

	public float maxVelocity = 7f;
	public float ballReleaseTimeout = 4f;
	public float hitCupLifetime = 2f;

	private PowerUpRing hitRing;

	
	public Vector3 relativeBallStartLocalPosition = new Vector3 (0.13f, 0f, 0.30f);
	public static Vector3 tableLocalScale = new Vector3 (0.69f, 0.6125f, 1.955f);

	public float gameStartTime {
	
		get;
		private set;
	}

	private List<BeerPongCup> hitCups = new List<BeerPongCup> ();
	private BeerPong.PlayerID winnerID = BeerPong.PlayerID.First;
	private Vector3 rocketRingHitTarget;
	
	private Transform gameCameraTransform;
	private Vector3 throwDirection = Vector3.forward;
	private Quaternion beerPongTableDefaultRotation = Quaternion.identity;
	private bool didSetDefaultRotation = false;
	private Bounds defaultCupBounds = new Bounds();
	private float ballThrowStartTime;

	private bool isMyTurn;

	private bool isSliderSet = false;
	
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
		CurrentPlayerInactive //Listen and apply events over the network : render trail, render ball motion, render ring, hit cup
		//From here transit to WaitToThrow or GameOver depending the number of cups on current player's side
	}
	
	void Awake () {
		
		Initialize <States> ();

		//TODO: Once the networking component is completed, uncomment the following 
		//event registration and delete the force invocation of OnPairingComplete();
		//BeerPongNetwork.Instance.OnPairingComplete += OnPairingComplete;

		//OnPairingComplete ();

		InvalidPlayerPositionText.SetActive (false);
		YouWonText.SetActive (false);
		YouLoseText.SetActive (false);
		ReplayButton.SetActive (false);

		gameCameraTransform = GameObject.Find ("Tango AR Camera").transform;

	}

	//This event handler will be registered to BeerPongNetwork component to listen to the pairing event
	public void OnPairingComplete () {

		ChangeState (States.Init);
	}

	private void SetUpCups () {

		//Initialize all cups numbered from 0 to 19, where cups 0 to playerCupCount - 1 
		//belong to player 1, and playerCupCount to 2*playerCupCount-1 belong to player 2
		GameObject cup=null;
		//float y_table = TableModel.transform.position.y;
		float tableHeight = tableLocalScale.y + 0.005f;//TableModel.GetComponent<Renderer> ().bounds.size.y;
		float tableWidth = tableLocalScale.x ; //TableModel.GetComponent<Renderer> ().bounds.size.x;
		float tableLength =tableLocalScale.z ; //TableModel.GetComponent<Renderer> ().bounds.size.z;
		float zSpacing = Mathf.Sqrt (3) / 2;
		float xOffset = 0.12f ;
		dictCup = new Dictionary<int,GameObject> ();
		for (int i = 0,l=6; i < playerCupCount; i++,l+=4) {
			
			if (i == 0)
				l = 0;
			if (i == 4)
				l = 3;
			if (i == 7)
				l = 5;
			if (i == 9)
				l = 7;
			
			cup = GameObject.Instantiate<GameObject>(cupPrefab);
			playRoundObject.Add (cup);
			cup.transform.parent = BoardwalkPong.transform;
			
			if (i == 0) {
				defaultCupBounds = cup.GetComponentInChildren<Renderer> ().bounds;
				defaultCupBounds.center = Vector3.zero;
			}
			
			float cupOffset =  cup.transform.position.y - cup.GetComponentInChildren<Renderer> ().bounds.min.y;
			float cupRadius  = defaultCupBounds.extents.x;
			
			if(i<4)
				cup.transform.localPosition = new Vector3(tableWidth/2-cupRadius/2-l*cupRadius/2-xOffset,tableHeight+cupOffset,tableLength/2-zSpacing*cupRadius);
			
			else if(i>=4 && i<7)
				cup.transform.localPosition = new Vector3(tableWidth/2-l*cupRadius/2-xOffset,tableHeight+cupOffset,tableLength/2-3*zSpacing*cupRadius);
			else if(i>=7 && i<9)
				cup.transform.localPosition = new Vector3(tableWidth/2-l*cupRadius/2-xOffset,tableHeight+cupOffset,tableLength/2-5*zSpacing*cupRadius);
			else if(i==9)
				cup.transform.localPosition = new Vector3(tableWidth/2-l*cupRadius/2-xOffset,tableHeight+cupOffset,tableLength/2-7*zSpacing*cupRadius);
			
			cup.GetComponent<BeerPongCup> ().cupNumber = i;
			cup.GetComponent<BeerPongCup> ().ball = Ball;
			cup.GetComponent<BeerPongCup> ().OnHit += OnHitOpponentCup;
			dictCup.Add (i,cup);
			
			//	Debug.Log ("position is  " + magnitude);
		}



		for (int i = playerCupCount,l=6; i < 2*playerCupCount; i++,l+=4) {

			if (i == 10)
				l = 0;
			if (i == 14)
				l = 3;
			if (i == 17)
				l = 5;
			if (i == 19)
				l = 7;

			cup = GameObject.Instantiate<GameObject>(cupPrefab);
			playRoundObject.Add (cup);
			cup.transform.parent = BoardwalkPong.transform;
			
			float cupOffset =  cup.transform.position.y - cup.GetComponentInChildren<Renderer> ().bounds.min.y;
			float cupRadius  = defaultCupBounds.extents.x;

			if(i<14)
				cup.transform.localPosition = new Vector3(tableWidth/2-cupRadius/2-l*cupRadius/2-xOffset,tableHeight+cupOffset,-tableLength/2+zSpacing*cupRadius);
			
			else if(i>=14 && i<17)
				cup.transform.localPosition = new Vector3(tableWidth/2-l*cupRadius/2-xOffset,tableHeight+cupOffset,-tableLength/2+3*zSpacing*cupRadius);
			else if(i>=17 && i<19)
				cup.transform.localPosition = new Vector3(tableWidth/2-l*cupRadius/2-xOffset,tableHeight+cupOffset,-tableLength/2+5*zSpacing*cupRadius);
			else if(i==19)
				cup.transform.localPosition = new Vector3(tableWidth/2-l*cupRadius/2-xOffset,tableHeight+cupOffset,-tableLength/2+7*zSpacing*cupRadius);
			
			cup.GetComponent<BeerPongCup> ().cupNumber = i;
			cup.GetComponent<BeerPongCup> ().ball = Ball;
			cup.GetComponent<BeerPongCup> ().OnHit += OnHitOpponentCup;
			dictCup.Add (i,cup);


			//	Debug.Log ("position is  " + magnitude);
		}


	}

	private void SetUpRings () {
		
		GameObject rocketRing = GameObject.Instantiate<GameObject>(ringPrefab);
		playRoundObject.Add (rocketRing);
		rocketRing.transform.parent = BoardwalkPong.transform;
		
		rocketRing.transform.localPosition = new Vector3(0,tableLocalScale.y*3/2,0);

		rocketRing.GetComponent<PowerUpRing> ().ringType = PowerUpRing.Type.ROCKET;
		rocketRing.GetComponent<PowerUpRing> ().OnHitRing += OnHitRing;

		GameObject ring = GameObject.Instantiate<GameObject>(ringPrefab);
		playRoundObject.Add (ring);
		ring.transform.parent = BoardwalkPong.transform;
		
		ring.transform.localPosition = new Vector3(0,tableLocalScale.y*3/2,0);
		
		ring.GetComponent<PowerUpRing> ().ringType = PowerUpRing.Type.SHOTGUN;
		ring.GetComponent<PowerUpRing> ().OnHitRing += OnHitRing;
	}
	
	private void SetUpObstacles () {

		for (int i = 0; i < 2; i ++) {

			GameObject obstacle = GameObject.Instantiate<GameObject> (obstaclePrefab);
			playRoundObject.Add (obstacle);
			obstacle.transform.parent = BoardwalkPong.transform;
			obstacle.transform.localPosition = new Vector3 (0, tableLocalScale.y * 3 / 2, 0);
		}
	}
	
	private void OnHitRing(PowerUpRing ring)
	{
		if ((States)GetState () == States.BallReleased) {

			hitRing = ring;
			ChangeState (States.HitRing);
		}
	}

	private void OnHitOpponentCup (int cupID) {

		if ((States)GetState () != States.BallReleased &&
			(States)GetState () != States.HitRing) {

			return;
		
		}

		if ((BeerPongNetwork.Instance.thisPlayerID == BeerPong.PlayerID.First && cupID >= 0 && cupID < playerCupCount) ||
		    (BeerPongNetwork.Instance.thisPlayerID == BeerPong.PlayerID.Second && cupID >= playerCupCount && cupID < 2*playerCupCount)) {

			if (!dictCup.ContainsKey (cupID)) return;

			hitCups.Add (dictCup [cupID].GetComponent <BeerPongCup> ());
			BeerPongNetwork.Instance.OnHitOpponentCup (cupID);

			if ((States) GetState () != States.HitOpponentCup) {

				ChangeState (States.HitOpponentCup);
			} 
		} 
		
		else {
			
			ChangeState (States.MissedOpponentCup);
		}
	}
	
	private void SetUpCamera (BeerPong.PlayerID playerID) {

		if (playerID == BeerPong.PlayerID.Second) {

			//Set the default roation, to reset roation on replay
			if (!didSetDefaultRotation) {

				beerPongTableDefaultRotation = BoardwalkPong.transform.rotation;
				didSetDefaultRotation = true;
			}

			//Change rotation as needed
			BoardwalkPong.transform.rotation = beerPongTableDefaultRotation;
			BoardwalkPong.transform.localRotation = Quaternion.Euler(BoardwalkPong.transform.localRotation.eulerAngles + Vector3.up * 180f);
		}
	}

	private void DestroyPreviousRoundObjects () {

		foreach (GameObject obj in playRoundObject) {
		
			Destroy (obj);
		}
	}
	
	private void Init_Enter () {

		DestroyPreviousRoundObjects ();

		gameStartTime = Time.time;

		SetUpCups ();
		SetUpCamera (BeerPongNetwork.Instance.thisPlayerID);
		SetUpRings ();
		SetUpObstacles ();




		InvalidPlayerPositionText.SetActive (false);
		YouWonText.SetActive (false);
		YouLoseText.SetActive (false);
		ReplayButton.SetActive (false);

		DifficultyMeter.Instance.Clear ();
		BeerPongInput.Instance.Reset ();

		BeerPongInput.Instance.OnThrowEnd += HandleOnThrowEnd;

		if (BeerPongNetwork.Instance.thisPlayerID == BeerPong.PlayerID.First) { 

			isMyTurn = true;
			ChangeState (States.WaitToThrow);

		} else {

			ChangeState (States.CurrentPlayerInactive);
		}

		BeerPongNetwork.Instance.OnOpponentMissedCup += HandleOnOpponentMissedCup;
		BeerPongNetwork.Instance.OnHitMyCup += HandleOnHitMyCup;
		BeerPongNetwork.Instance.OnTurnChange += HandleOnTurnChange;
	}

	void HandleOnTurnChange ()
	{
		if ((States)GetState () == States.CurrentPlayerInactive) {
		
			isMyTurn = true;
		}
		//TODO: Complete this!!!
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

		hitCups.Add (dictCup [cupNumber].GetComponent <BeerPongCup> ());
	}

	public void RenderBallPosition () {

		//TODO: We might want to slerp on absolute position change
		Ball.transform.position = gameCameraTransform.TransformPoint (relativeBallStartLocalPosition);

		if (!isSliderSet) {
			BeerPongInput.Instance.setSliderPosition ();
			isSliderSet = true;
			Debug.Log("isSliderSet");
		}

	}

	private void RenderBallBeforeThrow () {

		RenderBallPosition ();

		//TODO: Sync the ball across the network
	}

	private bool isUserAtValidPosition {

		get { 
			Vector3 ballLocalPosition = BoardwalkPong.transform.InverseTransformPoint (Ball.transform.position);
			return Mathf.Abs (ballLocalPosition.z) > tableLocalScale.z/2;
		}
	}
	
	private void WaitToThrow_Enter () {
		
		BeerPongInput.Instance.SetVisible (true);
		BeerPongInput.Instance.setSliderInitialState (0);

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
			BeerPongInput.Instance.setSliderInitialState (0.1f);

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

		//Throw at small angle upwards
		throwDirection = (gameCameraTransform.position + 
		                  (gameCameraTransform.forward + gameCameraTransform.up*0.4f).normalized * 5 
		                  - Ball.transform.position).normalized;

		//TODO: Wobble the throw direction based on Difficulty Meter level
	}

	private void RenderTrail_Update () {
		
		RenderBallBeforeThrow ();

		if (!isUserAtValidPosition) {
			
			ChangeState (States.InvalidPlayerPosition);
		}

		if (BeerPongInput.Instance.isTouchDown) {

			SetThrowDirection ();
			Vector3 initialVelocity = BeerPongInput.Instance.currentPower * throwDirection.normalized * maxVelocity;
			float targetY = Ball.transform.position.y - BoardwalkPong.transform.position.y - tableLocalScale.y;
			BallMotionController.Instance.RenderTrail (initialVelocity, Ball.transform.position, targetY);
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
		BallMotionController.Instance.ClearTrail ();
		
		if (isUserAtValidPosition) {

			ChangeState (States.WaitToThrow);
		}
	}
	
	private void InvalidPlayerPosition_Exit () {
		
		BeerPongInput.Instance.SetVisible (true);

		//Clear Invalid Position text
		InvalidPlayerPositionText.SetActive (false);
	}
	
	private bool isBallBelowTableLevel {
		
		get {
			
			Vector3 ballLocalPosition = BoardwalkPong.transform.InverseTransformPoint (Ball.transform.position);

			return ballLocalPosition.y < tableLocalScale.y / 2 - Ball.GetComponent <Renderer> ().bounds.size.y;
		}
	}
	
	private void BallReleased_Enter () {
		
		BallMotionController.Instance.ClearTrail ();
		BeerPongInput.Instance.SetVisible (false);

		ballThrowStartTime = Time.time;

		/****** TODO: Clear this DUMMY CODE till MotionController is completed ******/
		Ball.GetComponent<Rigidbody>().velocity = throwDirection.normalized * maxVelocity * BeerPongInput.Instance.currentPower;
		/****** TODO: Clear this DUMMY CODE till MotionController is completed ******/
	}
	
	private void BallReleased_Update () {
		
		if (isBallBelowTableLevel || Time.time - ballThrowStartTime > ballReleaseTimeout) {
			
			ChangeState (States.MissedOpponentCup);
		}
	}
	
	private bool DidClearCups (BeerPong.PlayerID playerID) {
		
		bool didClear = true;

		if (playerID == BeerPong.PlayerID.Second) {

			for (int i = 0; i < playerCupCount; i ++) {

				if (dictCup.ContainsKey (i)) {

					didClear = false;
					break;
				}
			}

		} else if (playerID == BeerPong.PlayerID.First) {
			
			for (int i = playerCupCount; i < 2*playerCupCount; i ++) {
				
				if (dictCup.ContainsKey (i)) {
					
					didClear = false;
					break;
				}
			}
		}

		return didClear;
	}
	
	private bool threwCup = false;
	private void AnimateClearingCup (BeerPongCup hitCup) {
		
		dictCup.Remove (hitCup.cupNumber);

		MeshCollider collider = hitCup.GetComponentInChildren<MeshCollider> ();
		collider.convex = true;
		collider.gameObject.transform.position += Vector3.up * 0.5f * hitCup.GetComponentInChildren<Renderer> ().bounds.size.y;
		collider.gameObject.AddComponent<Rigidbody> ().velocity = -Physics.gravity * 0.5f;
		collider.GetComponent<Rigidbody> ().mass = 0.2f;

		threwCup = false;
	}

	private bool DidAnimateClearingCups () {

		bool animatedAll = true;

		foreach (BeerPongCup hitCup in hitCups) {

			if (dictCup.ContainsKey (hitCup.cupNumber)) {
				AnimateClearingCup (hitCup);
			}

			Bounds tableBounds = TableModel.GetComponentInChildren<Renderer> ().bounds;

			if (tableBounds.max.y + hitCup.GetComponentInChildren<Renderer> ().bounds.size.y * 1.5f < 
				hitCup.GetComponentInChildren<Renderer> ().bounds.min.y && !threwCup) {

				Rigidbody cupRigidBody = hitCup.gameObject.GetComponentInChildren<Rigidbody> ();
				Vector3 xzPosition = Vector3.Scale (cupRigidBody.transform.position - tableBounds.center, new Vector3 (1, 0, 1));
				cupRigidBody.velocity += Physics.gravity.magnitude * 0.15f * xzPosition.normalized;
				cupRigidBody.angularVelocity = new Vector3 (Random.value, Random.value, Random.value) * 5f;
				threwCup = true;
			}

			if (tableBounds.max.y - tableLocalScale.y + hitCup.GetComponentInChildren<Renderer> ().bounds.size.y * 1.5f <
				hitCup.GetComponentInChildren<Renderer> ().bounds.min.y) {
			    
				if (Time.time - hitCup.hitTime < hitCupLifetime) {
			
					animatedAll = false;
				
				} else {
				
					hitCups.Remove (hitCup);
					Destroy (hitCup.gameObject);
				}
			}
		}

		return animatedAll;
	}

	private void HitOpponentCup_Update () {

		//TODO: Update ball position across network
		bool didAnimate = DidAnimateClearingCups ();

		if (didAnimate) {

			hitCups.Clear ();

			if (DidClearCups (BeerPongNetwork.Instance.opponentPlayerID)) {
		
				winnerID = BeerPongNetwork.Instance.thisPlayerID;
				ChangeState (States.GameOver);
		
			} else {
				BeerPongNetwork.Instance.NotifyTurnChange ();
				ChangeState (States.CurrentPlayerInactive);
			}
		}
	}

	private void OnHitRocketRing () {

		int cupIDOffset = 0;
		if (BeerPongNetwork.Instance.thisPlayerID == BeerPong.PlayerID.Second) {
			
			cupIDOffset = playerCupCount;
		}
		
		int targetCupID = cupIDOffset;
		foreach (int key in dictCup.Keys) {
			
			if (key >= cupIDOffset && key < cupIDOffset + playerCupCount) {
				
				targetCupID = key;
				break;
			}
		}
		
		GameObject targetCup = dictCup [targetCupID];
		float ballRadius = Ball.GetComponent<Renderer> ().bounds.extents.y;
		rocketRingHitTarget = targetCup.GetComponent<BeerPongCup> ().top + ballRadius * Vector3.up;
		
		Ball.GetComponent <Rigidbody> ().isKinematic = true;
		Ball.GetComponent <Rigidbody> ().velocity = Vector3.zero;
	}

	private void OnHitShotGunRing () {
	
		Vector3 ballPosition = Ball.transform.position;
		for (int i = 0; i < 4; i ++) {
		
			GameObject ringBall = Instantiate<GameObject> (ringBallPrefab);
			ringBall.transform.parent = BoardwalkPong.transform;
			Vector3 velocity = Ball.GetComponent <Rigidbody> ().velocity;
			Vector3 angleDiff = new Vector3 (Random.Range (-15f, 15f), Random.Range (-15f, 15f), 0);
			Quaternion rotation = Quaternion.Euler (angleDiff);

			ringBall.transform.position = Ball.transform.position;
			Vector3 rotatedVelocity = rotation * velocity;
			ringBall.transform.position += rotatedVelocity.normalized * ringBall.GetComponent<Renderer> ().bounds.size.x;
			Ball.GetComponent <Rigidbody> ().velocity = rotatedVelocity.normalized * velocity.magnitude;
		}
	}
	
	private void HitRing_Enter () {

		//TODO: Notify this event over network

		switch (hitRing.ringType) {

		case PowerUpRing.Type.ROCKET:
			OnHitRocketRing ();
			break;

		case PowerUpRing.Type.SHOTGUN:
			OnHitShotGunRing ();
			break;
		}
	}

	private void UpdateOnHitRocketRing () {
	
		float speedFactor = 1f;
		if (Time.time - hitRing.lastHitTime < PowerUpRing.HIT_WAIT_TIME / 2) {
			
			speedFactor = 0.1f;
			return;
		}
		
		float ballDiameter = Ball.GetComponent<Renderer> ().bounds.size.y;
		
		float xzDistance = Vector3.Scale (rocketRingHitTarget - Ball.transform.position, new Vector3(1, 0, 1)).magnitude;
		if (xzDistance < ballDiameter * 0.5f) {
			
			if (Ball.GetComponent<Rigidbody> ().isKinematic) {
				
				Ball.GetComponent<Rigidbody> ().isKinematic = false;
			}
			
		} else {
			
			Ball.transform.position += maxVelocity * 0.5f * Time.deltaTime * (rocketRingHitTarget - Ball.transform.position) * speedFactor;
		}
	}

	private void UpdateOnHitShotGunRing () {
		
	}

	private void HitRing_FixedUpdate () {

		switch (hitRing.ringType) {
			
		case PowerUpRing.Type.ROCKET:
			UpdateOnHitRocketRing ();
			break;
			
		case PowerUpRing.Type.SHOTGUN:
			UpdateOnHitShotGunRing ();
			break;
		}
		
		if (Time.time - ballThrowStartTime > ballReleaseTimeout) {
			
			ChangeState (States.MissedOpponentCup);
		}
	}

	private void HitRing_Exit () {

		Ball.GetComponent <Rigidbody> ().isKinematic = false;
	}

	private void MissedOpponentCup_Enter () {

		BeerPongNetwork.Instance.OnIMissedCup ();

		ChangeState (States.CurrentPlayerInactive);
	}

	private void CurrentPlayerInactive_Enter () {

		isMyTurn = false;

		//Clear all ring balls
		Ball[] balls = GameObject.FindObjectsOfType<Ball> ();
		foreach (Ball ball in balls) {
		
			if (ball.gameObject != Ball) {
				Destroy (ball.gameObject);
			}
		}

		//TODO: IMPORTANT!! Uncomment below line on completion of play testing, and remove the above line!
		isMyTurn = true;
	}
	
	private void CurrentPlayerInactive_Update() {
		
		if (DidAnimateClearingCups ()) {

			hitCups.Clear ();

			if (DidClearCups (BeerPongNetwork.Instance.thisPlayerID)) {
				
				winnerID = BeerPongNetwork.Instance.opponentPlayerID;
				ChangeState (States.GameOver);
				
			} else if (isMyTurn) {

				ChangeState (States.WaitToThrow);
			}
		}
	}

	private void GameOver_Enter () {

		BeerPongInput.Instance.OnThrowEnd -= HandleOnThrowEnd;
		BeerPongNetwork.Instance.OnOpponentMissedCup -= HandleOnOpponentMissedCup;
		BeerPongNetwork.Instance.OnTurnChange -= HandleOnTurnChange;

		if (winnerID == BeerPongNetwork.Instance.thisPlayerID) {
		
			YouWonText.SetActive (true);
		
		} else {
		
			YouLoseText.SetActive (true);
		}

		ReplayButton.SetActive (true);

		//TODO: Display the button for quit
	}

	public void OnClickPlayAgain () {
	
		if ((States)GetState () == States.GameOver) {
		
			ChangeState (States.Init);
		}
	}

	void OnDestroy () {
		//BeerPongNetwork.Instance.OnPairingComplete -= OnPairingComplete;
	}
}
