using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//
// TO DOs:
//
// 1. Clean up all the "TODO" comments in this script
// 2. Make the beam scale colours/alpha to match the max/min beam distance
// 3. Give the beam a cool shader effect
// 4. Public functions for: 
//			BeamCollided(GameObject collidingBeam)  -- To set special properties on this beam if it collides with another beam
//			BeamSizeChange(float factor)			-- Allow other scripts to change the beam size. Factor can be negative.
//			
//

public class BeamController : MonoBehaviour
{

	#region vars

	//This is the prefab we must drag-in on the Unity Editor
	public GameObject beamPrefab;

	//This is how we'll refer to any beam prefabs that we instantiate
	GameObject beam;

	//This boolean tells our script if we're in a beam or non-beam state
	bool beamEstablished = false;

	//Public floats for input in the Unity Editor
	public float startingBeamDistance;
	public float maxBeamDistance;
	public float maxAllowedBeamDistance;

	//This is our list of node GameObjects invovled with making this beam
	public List<GameObject> ownerNodeList;
	public List<GameObject> nodeBeamList;
	public List<Vector3> nodePositionlist;

	//This is the colour of the beam
	Color beamColour;

	#endregion

	void Start()
	{
		maxAllowedBeamDistance = startingBeamDistance;
	}

	// Update is called once per frame
	void Update()
	{

		//If two nodes are trying to make a beam...
		if (nodeBeamList.Count > 1)
		{

			//If we already have a beam established
			if (beamEstablished == true)
			{
				//Check if the current node positions are different from our last
				if (!nodePositionlist.Contains(nodeBeamList[0].transform.position) || !nodePositionlist.Contains(nodeBeamList[1].transform.position))
				{
					//If they're different, the player has moved a node and the beam must be re-drawn
					StartBeam(nodeBeamList[0].transform.position, nodeBeamList[1].transform.position);
				}
			}
			else
			{
				//We don't have a beam already, see if we're able to make one
				if (CloseEnoughForBeam(nodeBeamList[0].transform.position, nodeBeamList[1].transform.position) > 0)
				{
					//If our distance is good, we can tell our script a beam can be established
					beamEstablished = true;
				}
			}

		}

	}

	void GetNodes()
	{
		//Get our nodes
		foreach (Transform child in transform)
		{
			if (child.gameObject.tag == "Node")
			{
				//Debug.Log("- " + child.gameObject);
				ownerNodeList.Add(child.gameObject);
			}
		}
	}

	public void UpdateBeamDistance(float distanceIncrease)
	{
		if ((maxBeamDistance + distanceIncrease) < maxAllowedBeamDistance)
		{
			maxBeamDistance = maxBeamDistance + distanceIncrease;
		}

	}

	//Used throughout the script for measuring the distance between two nodes
	//NOTE: This function returns 0 if the nodes are not close enough, otherwise it returns the distance value
	public float CloseEnoughForBeam(Vector3 node1Position, Vector3 node2Position)
	{
		//Get the distance between the two node vectors
		float distance = Vector3.Distance(node1Position, node2Position);

		if (distance >= startingBeamDistance && distance <= maxBeamDistance)
		{
			beamEstablished = true;
			return distance;
		}
		else
		{
			beamEstablished = false;
			return 0;
		}
	}

	public void TryBeam(GameObject requestingNode)
	{

		//Lower the requsting node's speed
		NodeController requestingNodeController = requestingNode.GetComponent<NodeController>();
		requestingNodeController.DecreaseNodeSpeed(0);

		//If we already have two nodes...
		if (nodeBeamList.Count > 1)
		{

			//If we're close enough...
			if (CloseEnoughForBeam(nodeBeamList[0].transform.position, nodeBeamList[1].transform.position) > 0)
			{
				//Tell our script the beam is established
				beamEstablished = true;
			}
		}
		//Otherwise, check if the requesting node is already registered for trying a beam...
		else if (!nodeBeamList.Contains(requestingNode))
		{
			//If not, add it to the list
			nodeBeamList.Add(requestingNode);
		}

	}

	public bool IsBeamEstablished()
	{
		//TODO: There's probably a better way to do this
		if (beamEstablished == true)
		{
			return true;
		}
		else
		{
			return false;
		}

	}

	public void StopTryBeam(GameObject requestingNode, bool destroyed)
	{
		//A node is no longer trying to make a beam
		if (nodeBeamList.Contains(requestingNode))
		{

			NodeController requestingNodeController = requestingNode.GetComponent<NodeController>();
			requestingNodeController.IncreaseNodeSpeed(0);

			//Stop the beam, we know we require 2 nodes to have a beam
			StopBeam();

			//Take the node out of the list
			nodeBeamList.Remove(requestingNode);
			nodeBeamList.TrimExcess();

			// //If this is our last node and it was destroyed, destroy our player
			// if (destroyed == true)
			// {
			// 	ownerNodeList.Remove(requestingNode);
			// 	ownerNodeList.TrimExcess();

			// 	if (ownerNodeList.Count == 0)
			// 	{
			// 		Debug.Log("Destroying Player");
			// 		Destroy(gameObject);
			// 	}
			// }
		}
	}

	void StartBeam(Vector3 node1Position, Vector3 node2Position)
	{

		NodeController node1Controller = nodeBeamList[0].gameObject.GetComponent<NodeController>();
		NodeController node2Controller = nodeBeamList[1].gameObject.GetComponent<NodeController>();

		//Check the node distance and store it for use throughout calculations later
		float distance = CloseEnoughForBeam(node1Position, node2Position);

		//If we've gone beyond our allowable distance. Stop the beam.
		if (distance == 0)
		{
			StopBeam();
		}
		//Otherwise, start the beam
		else
		{
			//Do stuff to find the position, direction, scale and rotation of the beam
			Vector3 direction = Vector3.Normalize(node2Position - node1Position);
			Quaternion rotation = Quaternion.FromToRotation(Vector3.up, direction);
			Vector3 position = (node1Position + node2Position) / 2;

			//Generate a beam if one does not exist
			if (beam == null)
			{
				beam = Instantiate(beamPrefab, position, rotation, transform);

				MeshRenderer beamMesh = beam.GetComponent<MeshRenderer>();
				beamMesh.material.color = new Color(beamColour.r, beamColour.g, beamColour.b, 0.25f);

			}

			//Update the beam transform
			Vector3 scale = new Vector3(beam.transform.localScale.x, distance / 2, beam.transform.localScale.z);
			beam.transform.localScale = scale;
			beam.transform.rotation = rotation;
			beam.transform.position = new Vector3(position.x, position.y + 0.25f, position.z);

			//Add the the current node positions to the list to check if they change later
			nodePositionlist.Clear();
			nodePositionlist.Add(node1Position);
			nodePositionlist.Add(node2Position);


		}

	}

	void StopBeam()
	{
		beamEstablished = false;

		Destroy(beam);

		nodePositionlist.Clear();
	}

	public void SetBeamColour(Color color)
	{
		beamColour = color;
	}

	public void BeamCollided(bool collisionBegin)
	{
		foreach (GameObject node in nodeBeamList)
		{
			NodeController thisNode = node.gameObject.GetComponent<NodeController>();

			if (collisionBegin == true)
			{
				thisNode.IncreaseNodeSpeed(0);
			}
			else
			{
				thisNode.DecreaseNodeSpeed(0);
			}

		}
	}
}
