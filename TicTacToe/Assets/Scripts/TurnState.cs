using UnityEngine;
using System.Collections;

public class TurnState : MonoBehaviour {

	public enum States: int{Menu, Start, Player1, Player2, End};

	public int currentTurnState;

	// Use this for initialization
	void Start () {
		currentTurnState = (int)States.Menu;
		//Debug.Log ("currentTurnState set to States.Menu");
	}
	
	public void StartGame(){
		Debug.Log ("StartGame() called");
		currentTurnState = (int)States.Start;
	}

	public void NextTurn(){
		if(currentTurnState == (int)States.Player1)
			currentTurnState = (int)States.Player2;
		else 
			currentTurnState = (int)States.Player1;
	}

	public void EndGame(){
		Debug.Log ("currentTurnState set to States.End");
		currentTurnState = (int)States.End;
	}

	public void GoToMenu(){
		Debug.Log ("currentTurnState set to States.Menu");
		currentTurnState = (int)States.Menu;
	}


}
