using UnityEngine;
using System.Collections;

public class Engine : MonoBehaviour {
	
	public int menuLeft, menuTop;
	public Transform gameBoard_prefab, TTT_X, TTT_O;
	private GameObject gameBoard_gameObject;
	private TurnState turn;
	private NetworkScript networkScript;
	private GameBoard gameBoard;
	private string textFieldString = "Game #1";
	private string gameName;

	private bool instantiationFlag = false;
	private bool networked;
	private int player;

	private int spacing = 10;

	public bool debug = true;

	private Vector2 mouseClick;
	private Vector3 mouseClickInRWC; //in real world coordinates
	private Vector2 noClick = new Vector2(-1000,-1000);
	private int winner, p1Wins,p2Wins, draws;
	private int menuPage = 0;

	public string GameName{
		set{gameName = value;}
	}

	public bool Networked{
		get{return networked;}
	}

	// Use this for initialization
	void Start () {
		turn = GameObject.Find("TurnState").GetComponent("TurnState") as TurnState;
		networkScript = GameObject.Find("NetworkManager").GetComponent("NetworkScript") as NetworkScript;
		p1Wins = 0; p2Wins = 0; draws = 0;
		menuLeft = Screen.width/2 +5;
		menuTop = Screen.height/2 - 220;

	}
	
	void OnGUI(){
		//Debug.Log ("Engine.OnGui called");
		if(debug) DebugText();
		if(turn.currentTurnState == (int)TurnState.States.Menu){
			StartMenu();
		}else if(turn.currentTurnState == (int)TurnState.States.Start){
			GameStartAlert();
		}else if(turn.currentTurnState == (int)TurnState.States.End){
			if(networked)
				EndMenu();
			else
				NetworkedEndMenu();
		}

		if(turn.currentTurnState != (int)TurnState.States.Menu){
			ScoreKeeper();
		}
	}

	// Update is called once per frame
	void Update () {
		//Hot Seat Mode
		if(!networked){
			HotSeatGameLogic();
		//Networked Game
		}else{
			NetworkedGameLogic();
		}
	}

	public void HotSeatGameLogic(){
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
			if(winner == (int)GameBoard.SpaceStatus.X){ 
				p1Wins++;
			}else if(winner == (int)GameBoard.SpaceStatus.O){ 
				p2Wins++;
			}

			turn.EndGame();
		}
		
		if(gameBoard!=null && gameBoard.BoardFull() && turn.currentTurnState!=(int)TurnState.States.End && turn.currentTurnState!=(int)TurnState.States.Menu){
			Debug.Log ("Board is full, game set to end");
			draws++;
			turn.EndGame();
		}
	}

	//0 for draws, 1 for p1, 2 for p2
	[RPC] void SetWinCount(int player,int amount){ 
		Debug.Log ("SetWinCount RPC call");
		switch(player){
		case 0:
			draws = amount;
			break;
		case 1:
			p1Wins = amount;
			break;
		case 2:
			p2Wins = amount;
			break;
		}
	}

	public void NetworkedGameLogic(){
		if(instantiationFlag){
			gameBoard = GameObject.FindWithTag("Board").GetComponent("GameBoard") as GameBoard;
			instantiationFlag = false;
		}else if(turn.currentTurnState == (int)TurnState.States.Player1 && player==(int)TurnState.States.Player1){
			mouseClick = MouseClickToTileCoords();
			mouseClickInRWC = new Vector3(mouseClick.x*spacing,1.0f,mouseClick.y*spacing);
			if(mouseClick!=noClick && !gameBoard.SpaceTaken(mouseClick)){
				gameBoard.TakeSpace(mouseClick);
				Network.Instantiate(TTT_X,mouseClickInRWC,Quaternion.identity,0);
				turn.NextTurn();
			}
		}else if(turn.currentTurnState == (int)TurnState.States.Player2 && player==(int)TurnState.States.Player2){
			mouseClick = MouseClickToTileCoords();
			mouseClickInRWC = new Vector3(mouseClick.x*spacing,1.0f,mouseClick.y*spacing);
			if(mouseClick!=noClick && !gameBoard.SpaceTaken(mouseClick)){
				gameBoard.TakeSpace(mouseClick);
				Network.Instantiate(TTT_O,mouseClickInRWC,Quaternion.identity,0);
				turn.NextTurn();
			}
		}
		
		if(gameBoard!=null && gameBoard.ReturnWinner()!=0 && turn.currentTurnState!=(int)TurnState.States.Menu 
		   && turn.currentTurnState!=(int)TurnState.States.Start && turn.currentTurnState!=(int)TurnState.States.End){
			Debug.Log ("Winner found, game set to end");
			winner = gameBoard.ReturnWinner();
			if(winner == (int)GameBoard.SpaceStatus.X){ 
				p1Wins++;
				networkView.RPC ("SetWinCount",RPCMode.All,1,p1Wins);
			}else if(winner == (int)GameBoard.SpaceStatus.O){ 
				p2Wins++;
				networkView.RPC ("SetWinCount",RPCMode.All,2,p2Wins);
			}
			turn.EndGame();
		}
		
		if(gameBoard!=null && gameBoard.BoardFull() && turn.currentTurnState!=(int)TurnState.States.End && turn.currentTurnState!=(int)TurnState.States.Menu){
			Debug.Log ("Board is full, game set to end");
			draws++;
			networkView.RPC ("SetWinCount",RPCMode.All,0,draws);
			turn.EndGame();
		}
	}

	public void DisconnectProcedure(){
		DestroyBoardAndPieces();
		turn.GoToMenu();
		Network.Disconnect();
		menuPage = 0;
	}

	public void ScoreReset(){
		p1Wins = 0; p2Wins = 0; draws = 0;
	}

	public void ScoreKeeper(){
		if(networked){ 
			GUI.Label(new Rect(Screen.width-100,Screen.height/2-250,100,100),gameName);
			GUI.Label(new Rect(Screen.width-100,Screen.height/2-200,100,100),"Player " + (player-1));
		}
		GUI.Label(new Rect(Screen.width-100,Screen.height/2-150,100,100),System.Convert.ToChar(9632)+" wins: "+p1Wins);
		GUI.Label(new Rect(Screen.width-100,Screen.height/2-100,100,100),System.Convert.ToChar(9679)+" wins: "+p2Wins);
		GUI.Label(new Rect(Screen.width-100,Screen.height/2-50,100,100),"Draws: "+draws);
	}

	public void DebugText(){
		string currentState = "";
		if(turn.currentTurnState == (int)TurnState.States.Player1)
			currentState = "Player 1's Turn";
		else if(turn.currentTurnState == (int)TurnState.States.Player2)
			currentState = "Player 2's Turn";
		else if(turn.currentTurnState == (int)TurnState.States.Menu)
			currentState = "Menu";
		else if(turn.currentTurnState == (int)TurnState.States.End)
			currentState = "End";
		else if(turn.currentTurnState == (int)TurnState.States.Start)
			currentState = "Start";

		GUI.Label(new Rect(Screen.width-100,Screen.height-50,100,50),currentState);
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
		GUI.Box (new Rect(Screen.width/2+5,Screen.height/2-220,345,345),"Tic-Tac-Toe");
		switch(menuPage){
		case 0:
			if(GUI.Button(new Rect(Screen.width/2+22,Screen.height/2-200,300,100), "Hot Seat Mode")){
				networked = false;
				turn.StartGame();
				Instantiate(gameBoard_prefab,new Vector3(spacing,0,spacing),Quaternion.identity);
				instantiationFlag = true;
				ScoreReset();
			}

			if(!Network.isClient && !Network.isServer){
				if(GUI.Button(new Rect(Screen.width/2+22,Screen.height/2-100,300,100), "Host Game")){
					menuPage = 2;
					//networkScript.HostServer();
				}
			}

			if(GUI.Button(new Rect(Screen.width/2+22,Screen.height/2,300,100), "Join Game")){
				menuPage=1;
			}
			break;
		case 1:
			Vector2 scrollViewVector = Vector2.zero;
			GUI.Box (new Rect(menuLeft+10,menuTop+20,325,275),"Server List");
			if(networkScript.HostList!=null){
				//there is a host list instantiated
				scrollViewVector = GUI.BeginScrollView(new Rect(menuLeft+15,menuTop+30,315,255),scrollViewVector,new Rect(menuLeft+15,menuTop+30,315,255));
				int count = 0;
				for(int i=0;i<networkScript.HostList.Length;i++){
					if(networkScript.HostList[i].connectedPlayers<2){
						if(GUI.Button(new Rect(menuLeft+20,menuTop+40+(count*110),305,100), networkScript.HostList[i].gameName)){
							networkScript.JoinHost(networkScript.HostList[i]);
						}
						count++;
					}
				}
				GUI.EndScrollView();
			}

			if(GUI.Button(new Rect(menuLeft+150,menuTop+300,100,40), "Refresh")){
				networkScript.RefreshHostList();

			}
			if(GUI.Button(new Rect(menuLeft+10,menuTop+300,100,40), "Back")){
				menuPage = 0;
			}
			break;
		case 2:
			GUI.Box (new Rect(menuLeft+10,menuTop+20,325,275),"Create Game");
			textFieldString = GUI.TextField(new Rect(menuLeft+20,menuTop+100,300,100),textFieldString);
			if(GUI.Button (new Rect(menuLeft+120,menuTop+220,100,40),"Create")){
				networkScript.HostServer(textFieldString);
			}
			if(GUI.Button(new Rect(menuLeft+20,menuTop+300,100,40), "Back")){
				menuPage = 0;
			}
			break;
		}
	}

	public void BeginNetworkedGame(){
		networked = true;
		turn.StartGame();
		Network.Instantiate(gameBoard_prefab,new Vector3(spacing,0,spacing),Quaternion.identity,0);
		instantiationFlag = true;
		ScoreReset();
		player = (int)TurnState.States.Player1;
	}

	public void JoinNetworkedGame(){
		networked = true;
		turn.StartGame();
		instantiationFlag = true;
		ScoreReset();
		player = (int)TurnState.States.Player2;
	}
	

	public void GameStartAlert(){
		if(networked){	
			if(Network.connections.Length==1){
				if(Network.isServer){	
					if(GUI.Button(new Rect(menuLeft,menuTop,300,300), "Start!")){
						turn.NextTurn();
					}
				}else{
					GUI.Label(new Rect(Screen.width/2+122,Screen.height/2-200,300,300), "Waiting for host to start the game...");
				}
			}else{
				GUI.Label(new Rect(Screen.width/2+172,Screen.height/2-200,300,300), "Waiting for an opponent...");
				//GUI.Label(new Rect(Screen.width/2+172,Screen.height/2-100,300,300), "Connections: "+Network.connections.Length);
			}
		}else{
			if(GUI.Button(new Rect(menuLeft,menuTop,300,300), "Start!")){
				turn.NextTurn();
			}
		}
	}

	public void EndMenu(){
		GUI.Box (new Rect(menuLeft,menuTop,345,345),"Game Over");
		if(GUI.Button(new Rect(menuLeft+22,menuTop+50,300,100), "Rematch")){
			DestroyPieces();
			gameBoard.InitSpaces();
			turn.StartGame();
		}
		if(GUI.Button(new Rect(menuLeft+22,menuTop+170,300,100), "Back to Menu")){
			DestroyBoardAndPieces();
			turn.GoToMenu();
			menuPage = 0;
		}
	}

	public void NetworkedEndMenu(){
		GUI.Box (new Rect(menuLeft,menuTop,345,345),"Game Over");
		if(GUI.Button(new Rect(menuLeft+22,menuTop+50,300,100), "Rematch")){
			DestroyPieces();
			gameBoard.InitSpaces();
			turn.StartGame();
		}
		if(GUI.Button(new Rect(menuLeft+22,menuTop+170,300,100), "Back to Menu")){
			DestroyBoardAndPieces();
			turn.GoToMenu();
			menuPage = 0;
		}
	}


	public void DestroyBoardAndPieces(){
		if(networked){
			foreach(GameObject g in GameObject.FindGameObjectsWithTag("Board")) Network.Destroy(g);
			foreach(GameObject g in GameObject.FindGameObjectsWithTag("Piece")) Network.Destroy(g);
		}else{
			foreach(GameObject g in GameObject.FindGameObjectsWithTag("Board")) Destroy(g);
			foreach(GameObject g in GameObject.FindGameObjectsWithTag("Piece")) Destroy(g);
		}
	}

	public void DestroyPieces(){
		if(networked){
			foreach(GameObject g in GameObject.FindGameObjectsWithTag("Piece")) Network.Destroy(g);
		}else{
			foreach(GameObject g in GameObject.FindGameObjectsWithTag("Piece")) Destroy(g);
		}
	}

}
