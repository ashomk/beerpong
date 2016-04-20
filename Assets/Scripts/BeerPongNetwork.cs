using UnityEngine;
using System.Collections;

//This component will let the Game know about :
//	- When the pairing was completed
//	- This player's & opponent's player ID
//	- If the opponent managed to hit this player's cup
//	- If the current player's turn is over

public class BeerPongNetwork : Singleton<BeerPongNetwork> {

	protected BeerPongNetwork () {}

	public BeerPong.PlayerID thisPlayerID {

		get;
		private set;
	}

	public BeerPong.PlayerID opponentPlayerID {

		get {

			return thisPlayerID == BeerPong.PlayerID.First ? BeerPong.PlayerID.Second : BeerPong.PlayerID.First;
		}
	}

	public delegate void PairingCompleteEvent();

	//This event is called when pairing is completed
	public event PairingCompleteEvent OnPairingComplete;

	public delegate void OpponentQuitEvent();

	//This event is called when opponent has quit 
	public event OpponentQuitEvent OnOpponentQuit;

	private enum GameEvent {

		TURN_CHANGE = 0,
		HIT_CUP
	}

	public delegate void TurnChangeEvent ();
	
	//This event is called if this player's turn is complete
	public event TurnChangeEvent OnTurnChange;
	
	//Notify opponent on turn change
	public void NotifyTurnChange () {
		
		byte evCode = (int)GameEvent.TURN_CHANGE;
		int content = 0;
		bool reliable = true;
		PhotonNetwork.RaiseEvent(evCode, content, reliable, null);
	}
	
	public delegate void HitCupEvent (int cupID);

	//This event is called if this player's cup is hit
	public event HitCupEvent OnHitMyCup;

	//Call this function if the opponent's cup is hit
	public void OnHitOpponentCup (int cupID) {
		
		byte evCode = (int)GameEvent.HIT_CUP;
		int content = cupID;
		bool reliable = true;
		PhotonNetwork.RaiseEvent(evCode, content, reliable, null);
	}

	private bool isPlayerOne;

	public bool pairing {

		get;
		private set;
	}

	public bool unpairing {

		get;
		private set;
	}

	public bool paired {
	
		get;
		private set;
	}

	private bool didRoomListUpdate;

	void Awake()
	{
		PhotonNetwork.OnEventCall += OnPhotonEvent;

		//Connect to the main photon server. This is the only IP and port we ever need to set(!)
		if (!PhotonNetwork.connected)
			PhotonNetwork.ConnectUsingSettings("v1.0"); // version of the game/demo. used to separate older clients from newer ones (e.g. if incompatible)

		//Load name from PlayerPrefs
		PhotonNetwork.playerName = ((int)Random.Range(1, 9999)).ToString ();

		thisPlayerID = BeerPong.PlayerID.First;
		pairing = paired = false;
		unpairing = true;
	}

	//Handle Photon events
	private void OnPhotonEvent(byte eventcode, object content, int senderid)
	{
		PhotonPlayer sender = PhotonPlayer.Find(senderid);
		if (PhotonNetwork.player.ID == sender.ID) {

			return;
		}

		switch ((GameEvent)eventcode)
		{
		case GameEvent.TURN_CHANGE:
			if (OnTurnChange != null) {

				OnTurnChange ();
			}
			break;

		case GameEvent.HIT_CUP:
			if (OnHitMyCup != null) {

				int cupID = (int) content;
				OnHitMyCup (cupID);
			}
			break;
		}
	}

	void OnGUI()
	{
		if (!PhotonNetwork.connected)
		{
			ShowConnectingGUI();
			return;   //Wait for a connection
		}
	}

	void ShowConnectingGUI()
	{
		GUILayout.BeginArea(new Rect((Screen.width - 100) / 2, (Screen.height - 200) / 2, 400, 300));

		GUILayout.Label("Connecting...");

		GUILayout.EndArea();
	}

	public void OnConnectedToMaster()
	{
		// this method gets called by PUN, if "Auto Join Lobby" is off.
		// this demo needs to join the lobby, to show available rooms!

		PhotonNetwork.JoinLobby();  // this joins the "default" lobby
	}

	void CreateRoomOrJoin () {

		int roomCount = PhotonNetwork.GetRoomList ().Length;
		Debug.Log ("Room count : " + roomCount);

		if (roomCount == 0) {

			isPlayerOne = true;
			CreateTwoPlayerRoom ();

		} else {

			isPlayerOne = false;
			PhotonNetwork.JoinRandomRoom ();
		}
	}

	public void Pair () {

		if (didRoomListUpdate) {

			if (!pairing) {
				pairing = true;

				CreateRoomOrJoin ();
			
			} else {

				Debug.LogError (paired ? "Error : Paired in lobby!!!" : "Pairing in progress");
			}
		
		} else {

			if (pairing) {

				Debug.LogError ("Pairing in progress");
			
			} else if (unpairing) {

				Debug.LogError ("Unpairing in progress. Pair after unpairing");
			
			} else if (paired) {

				Debug.LogError ("Already paired");
			}
		}
	}

	void CreateTwoPlayerRoom () {

		int number = Random.Range (0, 100000);
		string roomName = "room"+number;
		Debug.Log ("Creating room : " + roomName);
		PhotonNetwork.CreateRoom (roomName, new RoomOptions () { maxPlayers = 2 }, TypedLobby.Default);
	}

	void OnJoinedRoom()
	{
		Debug.Log("Connected to Room");

		if (!isPlayerOne) {
		
			OnPaired ();
		}
	}

	void OnPhotonRandomJoinFailed (object[] codeAndMsg) {

		Debug.Log ("Failed joining room");
		foreach (object str in codeAndMsg) {

			Debug.Log (str.ToString ());
		}

		isPlayerOne = true;
		CreateTwoPlayerRoom ();
	}


	void OnPaired () {

		unpairing = false;
		pairing = false;
		paired = true;

		thisPlayerID = isPlayerOne ? BeerPong.PlayerID.First : BeerPong.PlayerID.Second;

		if (OnPairingComplete != null) {

			OnPairingComplete ();
		}
	}

	void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
	{
		Debug.Log ("Opponent connected");

		OnPaired ();
	}

	void OnPhotonPlayerDisconnected(PhotonPlayer player)
	{
		Debug.Log ("Player disconnected");

		if (OnOpponentQuit != null) {

			OnOpponentQuit ();
		}

		if (paired && !unpairing) {
			Unpair ();
		}
	}

	void OnReceivedRoomListUpdate() {

		didRoomListUpdate = true;
		pairing = false;
		unpairing = false;
		paired = false;
	}

	void OnJoinedLobby() {

		didRoomListUpdate = false;
		unpairing = true;
	}

	public void Unpair () {

		if (pairing) {

			Debug.LogError ("Cannot unpair untill pairing completed");

		} else if (!paired) {

			Debug.LogError ("Already unpaired");

		} else if (PhotonNetwork.inRoom) {

			if (!unpairing) {
				
				unpairing = true;
				PhotonNetwork.LeaveRoom ();
			
			} else {

				Debug.LogError ("Already unpairing");
			}
		
		}
	}
}
