using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

	#region vars

	public Vector3 nodeCentrePoint;

	List<Color> availableColours = new List<Color>();

	public List<GameObject> nodeList = new List<GameObject>();

	public float playerNormalSpeed;
	public float playerSlowedSpeed;

	GameController gameController;

	Color playerColour;

	bool playerInitialised; //Controls whether or not this players startup vars have been set

	float numKills = 0;

	#endregion

	void InitalisePlayer()
	{
		if (playerInitialised == false)
		{
			//Get components
			gameController = GameObject.Find("GameController").GetComponent<GameController>();
			GetNodes();
			playerInitialised = true;
		}
	}

	// Update is called once per frame
	void Update()
	{
		//Maintain the centre point of our two nodes... do we need this?
		// if (nodeList.Count > 0)
		// 	nodeCentrePoint = (nodeList[0].gameObject.transform.position + nodeList[1].gameObject.transform.position) / 2;
	}

	void GetNodes()
	{
		//Get our nodes
		foreach (Transform child in transform)
		{
			if (child.gameObject.tag == "Node")
			{
				//Debug.Log("- " + child.gameObject);
				nodeList.Add(child.gameObject);
			}
		}
	}

	public void SetNodeColours()
	{

		if (playerInitialised == false)
			InitalisePlayer();

		//Get an available colour from the gamecontroller
		Color myColour = gameController.ProvideColour();

		//Create a "darker" colour variant
		Color variantColour = new Color(myColour.r / 2, myColour.g / 2, myColour.b / 2, 1.0f);
		playerColour = myColour;

		//Set the colour for each node
		nodeList[0].GetComponent<NodeController>().SetNodeColour(myColour, variantColour);
		nodeList[1].GetComponent<NodeController>().SetNodeColour(variantColour, myColour);

	}

	public float GetPlayerNormalSpeed()
	{
		return playerNormalSpeed;
	}

	public float GetPlayerSlowedSpeed()
	{
		return playerSlowedSpeed;
	}

	public void NodesDestroyed()
	{
		gameController.PlayerDestroyed(gameObject, playerColour);
	}

	public void GotKill()
	{
		numKills++;
	}

	//TODO: Keep track of my nodes and if they are alive/dead.
	//		- Handle respawning
	//		- Handle removing player

}
