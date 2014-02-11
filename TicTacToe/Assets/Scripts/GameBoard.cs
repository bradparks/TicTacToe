using UnityEngine;
using System.Collections;

public class GameBoard : MonoBehaviour {

	public enum SpaceStatus{open,X,O}; //0,1,2


	TurnState turn;
	Engine engine;
	private int[,] spaces;

	// Use this for initialization
	void Awake () {
		turn = GameObject.Find("TurnState").GetComponent("TurnState") as TurnState;
		engine = GameObject.Find("Engine").GetComponent("Engine") as Engine;
		InitSpaces();
	}
	

	public void InitSpaces(){
		spaces = new int[3,3];
		for(int i=0;i<3;i++){
			for(int j=0;j<3;j++){
				spaces[i,j] = (int)SpaceStatus.open;
				if(engine.Networked)
					networkView.RPC("UpdateSpace",RPCMode.All,i,j,(int)SpaceStatus.open);
			}
		}
	}
	

	public bool BoardFull(){
		for(int i=0;i<3;i++){
			for(int j=0;j<3;j++){
				if(!SpaceTaken(new Vector2(i,j))) return false;
			}
		}
		return true;
	}

	/*return: 
	0 if no winner, 
	1 if player 1, 
	2 if player 2
	*/
	public int ReturnWinner(){
		for(int i=0;i<3;i++){
			if(CheckRow (i)) return spaces[i,0];
			if(CheckColumn(i)) return spaces[0,i];
		}
		if(CheckDiagonals()) return spaces[1,1];
		return 0;

	}

	public bool CheckRow(int rowToCheck){
		return (spaces[rowToCheck,0]==spaces[rowToCheck,1] && spaces[rowToCheck,1]==spaces[rowToCheck,2] && spaces[rowToCheck,0]!=(int)SpaceStatus.open);
	}

	public bool CheckColumn(int columnToCheck){
		return (spaces[0,columnToCheck]==spaces[1,columnToCheck] && spaces[1,columnToCheck]==spaces[2,columnToCheck] && spaces[0,columnToCheck]!=(int)SpaceStatus.open);
	}

	public bool CheckDiagonals(){
		if(spaces[0,0]==spaces[1,1] && spaces[1,1]==spaces[2,2] && spaces[1,1]!=(int)SpaceStatus.open) return true;
		if(spaces[0,2]==spaces[1,1] && spaces[1,1]==spaces[2,0] && spaces[1,1]!=(int)SpaceStatus.open) return true;
		return false;
	}

	public bool SpaceTaken(Vector2 space){
		return (!(spaces[(int)space.x,(int)space.y] == (int)SpaceStatus.open));
	}

	public void TakeSpace(Vector2 space){
		int status = 0;
		if(turn.currentTurnState == (int)TurnState.States.Player1){
			spaces[(int)space.x,(int)space.y] = (int)SpaceStatus.X;
			status = (int)SpaceStatus.X;
		}else if(turn.currentTurnState == (int)TurnState.States.Player2){
			spaces[(int)space.x,(int)space.y] = (int)SpaceStatus.O;
			status = (int)SpaceStatus.O;
		}else{
			Debug.Log ("Error: GameBoard.TakeSpace");
		}

		if(engine.Networked) networkView.RPC("UpdateSpace",RPCMode.All,(int)space.x,(int)space.y,status);
	}

	[RPC] void UpdateSpace(int x,int y, int spaceStatus){
		spaces[x,y] = spaceStatus;
	}
}
