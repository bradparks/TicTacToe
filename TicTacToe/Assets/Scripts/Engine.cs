using UnityEngine;
using System.Collections;

public class Engine : MonoBehaviour {

	public Transform gameBoard_prefab, TTT_X, TTT_O;
	private GameObject gameBoard_gameObject;
	private TurnState turn;
	private GameBoard gameBoard;

	private bool instantiationFlag = false;

	private int spacing = 10;

	public bool debug = true;

	private Vector2 mouseClick;
	private Vector3 mouseClickInRWC; //in real world coordinates
	private Vector2 noClick = new Vector2(-1000,-1000);
	private int winner, p1Wins,p2Wins, draws;
	

	// Use this for initialization
	void Start () {
		turn = GameObject.Find("TurnState").GetComponent("TurnState") as TurnState;
		p1Wins = 0; p2Wins = 0; draws = 0;

	}
	
	void OnGUI(){
		//Debug.Log ("Engine.OnGui called");
		if(debug) DebugText();
		if(turn.currentTurnState == (int)TurnState.States.Menu){
			StartMenu();
		}else if(turn.currentTurnState == (int)TurnState.States.Start){
			GameStartAlert();
		}else if(turn.currentTurnState == (int)TurnState.States.End){
			EndMenu();
		}

		if(turn.currentTurnState != (int)TurnState.States.Menu){
			ScoreKeeper();
		}
	}

	// Update is called once per frame
	void Update () {

		if(instantiationFlag){
			gameBoard = GameObject.FindWithTag("Board").GetComponent("GameBoard") as GameBoard;
			instantiationFlag = false;
		}else if(turn.currentTurnState == (int)TurnState.States.Player1){
			mouseClick = MouseClickToTileCoords();
			mouseClickInRWC = new Vector3(mouseClick.x*spacing,1.0f,mouseClick.y*spacing);
			if(mouseClick!=noClick && !gameBoard.SpaceTaken(mouseClick)){
				gameBoard.TakeSpace(mouseClick);
				Instantiate(TTT_X,mouseClickInRWC,Quaternion.identity);
				turn.NextTurn();
			}
		}else if(turn.currentTurnState == (int)TurnState.States.Player2){
			mouseClick = MouseClickToTileCoords();
			mouseClickInRWC = new Vector3(mouseClick.x*spacing,1.0f,mouseClick.y*spacing);
			if(mouseClick!=noClick && !gameBoard.SpaceTaken(mouseClick)){
				gameBoard.TakeSpace(mouseClick);
				Instantiate(TTT_O,mouseClickInRWC,Quaternion.identity);
				turn.NextTurn();
			}
		}

		if(gameBoard!=null && gameBoard.ReturnWinner()!=0 && turn.currentTurnState!=(int)TurnState.States.Menu 
		   && turn.currentTurnState!=(int)TurnState.States.Start && turn.currentTurnState!=(int)TurnState.States.End){
			Debug.Log ("Winner found, game set to end");
			winner = gameBoard.ReturnWinner();
			if(winner == (int)GameBoard.SpaceStatus.X) p1Wins++;
			if(winner == (int)GameBoard.SpaceStatus.O) p2Wins++;
			turn.EndGame();
		}

		if(gameBoard!=null && gameBoard.BoardFull() && turn.currentTurnState!=(int)TurnState.States.End && turn.currentTurnState!=(int)TurnState.States.Menu){
			Debug.Log ("Board is full, game set to end");
			draws++;
			turn.EndGame();
		}
	}

	public void ScoreReset(){
		p1Wins = 0; p2Wins = 0; draws = 0;
	}

	public void ScoreKeeper(){
		GUI.Label(new Rect(Screen.width-100,Screen.height/2-200,100,100),"X wins: "+p1Wins);
		GUI.Label(new Rect(Screen.width-100,Screen.height/2-150,100,100),"O wins: "+p2Wins);
		GUI.Label(new Rect(Screen.width-100,Screen.height/2-100,100,100),"Draws: "+draws);
	}

	public void DebugText(){
		string currentState = "";
		if(turn.currentTurnState == (int)TurnState.States.Player1)
			currentState = "Player 1";
		else if(turn.currentTurnState == (int)TurnState.States.Player2)
			currentState = "Player 2";
		else if(turn.currentTurnState == (int)TurnState.States.Menu)
			currentState = "Menu";
		else if(turn.currentTurnState == (int)TurnState.States.End)
			currentState = "End";
		else if(turn.currentTurnState == (int)TurnState.States.Start)
			currentState = "Start";

		GUI.Label(new Rect(Screen.width-100,Screen.height-100,100,100),currentState);
	}

	public Vector2 MouseClickToTileCoords()
	{
		if(Input.GetMouseButtonDown(0)){
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if(Physics.Raycast(ray,out hit,1000)){
				float x = hit.point.x;
				float z = hit.point.z;
				if(Mathf.Floor(x)%spacing<=5)
					x = Mathf.Floor(x) - Mathf.Floor(x)%spacing;
				else
					x = Mathf.Floor(x) + (spacing-Mathf.Floor(x)%spacing);
				if(Mathf.Floor(z)%spacing<=5)
					z = Mathf.Floor(z)-Mathf.Floor(z)%spacing;
				else
					z = Mathf.Floor(z) + (spacing-Mathf.Floor(z)%spacing);
				x=x/spacing; z=z/spacing;
				//Debug.Log("Clicked on ("+x+","+z+")");
				//Debug.Log(x+","+z+" in room "+scene.ReturnRoomContainingTile((int)x,(int)z));
				return new Vector2(x,z);
			}
		}
		//no click 
		return noClick;
	}

	public void StartMenu(){
		GUI.Box (new Rect(Screen.width/2+150,Screen.height/2-220,345,345),"Tic-Tac-Toe");
		if(GUI.Button(new Rect(Screen.width/2+172,Screen.height/2-200,300,100), "Hot Seat Mode")){
			turn.StartGame();
			Instantiate(gameBoard_prefab,new Vector3(spacing,0,spacing),Quaternion.identity);
			instantiationFlag = true;
			ScoreReset();
		}
	}
	

	public void GameStartAlert(){
		if(GUI.Button(new Rect(Screen.width/2+172,Screen.height/2-200,300,300), "Start!")){
			turn.NextTurn();
		}
	}

	public void EndMenu(){
		GUI.Box (new Rect(Screen.width/2,Screen.height/2-175,345,345),"Game Over");
		if(GUI.Button(new Rect(Screen.width/2+22,Screen.height/2-150,300,100), "Rematch")){
			DestroyPieces();
			gameBoard.InitSpaces();
			turn.StartGame();
		}
		if(GUI.Button(new Rect(Screen.width/2+22,Screen.height/2,300,100), "Back to Menu")){
			DestroyBoardAndPieces();
			turn.GoToMenu();
		}
	}

	public void DestroyBoardAndPieces(){
		foreach(GameObject g in GameObject.FindGameObjectsWithTag("Board")) Destroy(g);
		foreach(GameObject g in GameObject.FindGameObjectsWithTag("Piece")) Destroy(g);
	}

	public void DestroyPieces(){
		foreach(GameObject g in GameObject.FindGameObjectsWithTag("Piece")) Destroy(g);
	}

}
