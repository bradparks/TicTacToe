using UnityEngine;
using System.Collections;

public class GamePiece : MonoBehaviour {

	public float startingHeight = 5;

	// Use this for initialization
	void Start () {
		transform.Translate(Vector3.up*startingHeight);
	}

}
