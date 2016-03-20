using UnityEngine;
using System.Collections;

//This component will let the Game know about :
//	- When the pairing was completed
//	- This player's & opponent's player ID
//	- If the opponent managed to hit this player's cup
//	- If the current player's turn is over

public class BeerPongNetwork : Singleton<BeerPongNetwork> {

	protected BeerPongNetwork () {}

	public BeerPong.PlayerID thisPlayerID = BeerPong.PlayerID.First;

	public BeerPong.PlayerID opponentPlayerID {

		get {

			return thisPlayerID == BeerPong.PlayerID.First ? BeerPong.PlayerID.Second : BeerPong.PlayerID.First;
		}
	}

	public delegate void PairingCompleteEvent();

	//This event is called when pairing is completed
	public event PairingCompleteEvent OnPairingComplete;

	public delegate void HitCupEvent (int cupID);

	//This event is called if this player's cup is hit
	public event HitCupEvent OnHitMyCup;

	//Call this function if the opponent's cup is hit
	public void OnHitOpponentCup (int cupID) {
		//TODO: Forward it through the network
	}

	public delegate void TurnChangeEvent ();
	
	//This event is called if this player's turn is complete
	public event TurnChangeEvent OnTurnChange;
	
	//Notify opponent on turn change
	public void NotifyTurnChange () {
	
		//TODO: Forward it through the network
	}
	
	public delegate void MissedCup ();

	//This event is called if the opponent said he missed the cup
	public event MissedCup OnOpponentMissedCup;

	//Call this function if this player missed a cup
	public void OnIMissedCup () {
		//TODO: Forward it through the network
	}
}
