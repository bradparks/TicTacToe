using UnityEngine;
using System.Collections;

public class TurnState : MonoBehaviour {

	public enum States: int{Menu, Start, Player1, Player2, End};

	public int currentTurnState;
	Engine engine;

	// Use this for initialization
	void Start () {
		currentTurnState = (int)States.Menu;
		engine = GameObject.Find ("Engine").GetComponent("Engine") as Engine;
		//Debug.Log ("currentTurnState set to States.Menu");
	}
	
	public void StartGame(){
		Debug.Log ("StartGame() called");
		currentTurnState = (int)States.Start;
		if(engine.Networked)
			networkView.RPC ("ChangeTurnStateTo",RPCMode.All,currentTurnState);
	}

	public void NextTurn(){
		if(currentTurnState == (int)States.Player1)
			currentTurnState = (int)States.Player2;
		else 
			currentTurnState = (int)States.Player1;

		if(engine.Networked)
			networkView.RPC ("ChangeTurnStateTo",RPCMode.All,currentTurnState);
	}
	

	[RPC] void ChangeTurnStateTo(int turnState){
		currentTurnState = turnState;
	}

	public void EndGame(){
		Debug.Log ("currentTurnState set to States.End");
		currentTurnState = (int)States.End;
		if(engine.Networked)
			networkView.RPC ("ChangeTurnStateTo",RPCMode.All,currentTurnState);
	}

	public void GoToMenu(){
		Debug.Log ("currentTurnState set to States.Menu");
		currentTurnState = (int)States.Menu;
		if(engine.Networked)
			networkView.RPC ("ChangeTurnStateTo",RPCMode.All,currentTurnState);
	}


}
