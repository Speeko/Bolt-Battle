using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BeamCollisions : MonoBehaviour
{

	#region vars

	List<GameObject> ownerNodes;

	PlayerController playerController;

	#endregion

	void Awake()
	{
		//Our beam is alive, lets create a list with the nodes that generated this beam
		ownerNodes = new List<GameObject>();

		foreach (Transform child in transform.parent)
		{
			ownerNodes.Add(child.gameObject);
		}

		//Remove ourselves from the list (we're not a node)
		ownerNodes.Remove(gameObject);

		playerController = transform.parent.GetComponent<PlayerController>();

	}

	void OnTriggerEnter(Collider collider)
	{
		if (collider.gameObject.tag == "Node" || collider.gameObject.tag == "EnemyAI")
		{
			if (!ownerNodes.Contains(collider.gameObject))
			{
				NodeController enemyNodeController = collider.gameObject.GetComponent<NodeController>();
				enemyNodeController.DestroyNode();

				//We killed a node, tell our playerController about it
				playerController.GotKill();
			}
		}

		if (collider.gameObject.tag == "Beam")
		{
			//PlayerController playerController = transform.parent.gameObject.GetComponent<PlayerController>();
			foreach (GameObject node in ownerNodes)
			{
				NodeController nodeController = node.gameObject.GetComponent<NodeController>();
				nodeController.IncreaseNodeSpeed(0);
			}
		}
	}

	//TODO: This is not being called because the beam is destroyed before ever exiting the trigger
	void OnTriggerExit(Collider collider)
	{
		if (collider.gameObject.tag == "Beam")
		{
			foreach (GameObject node in ownerNodes)
			{
				Debug.Log("Exiting beam collision");
				NodeController nodeController = node.gameObject.GetComponent<NodeController>();
				nodeController.DecreaseNodeSpeed(0);
			}
		}
	}
}
