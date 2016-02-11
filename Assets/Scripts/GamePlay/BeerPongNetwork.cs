using UnityEngine;
using System.Collections;

public class BeerPongNetwork : Singleton<BeerPongNetwork> {

	protected BeerPongNetwork () {}

	public BeerPong.PlayerID thisPlayerID = BeerPong.PlayerID.First;
	public BeerPong.PlayerID currentPlayerID = BeerPong.PlayerID.First;

}
