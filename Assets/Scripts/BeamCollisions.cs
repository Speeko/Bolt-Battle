using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BeamCollisions : MonoBehaviour
{

	#region vars

	List<GameObject> ownerNodes;

	PlayerController playerController;

	bool collidingWithBeam;
	bool wasCollidingWithBeamLastFrame;

	GameObject collidingBeam;

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

	void Update()
	{
		//TODO: Make this more robust and don't call in update
		//Check if there's a beam we're colliding with
		if (collidingBeam != null && collidingBeam.activeInHierarchy == true)
		{
			foreach (GameObject node in ownerNodes)
			{
				//If there's a beam we're colliding with - increase the nodes speed
				NodeController nodeController = node.gameObject.GetComponent<NodeController>();
				nodeController.IncreaseNodeSpeed(0);
			}
		}
		else
		{
			foreach (GameObject node in ownerNodes)
			{
				//If we're not colliding with a beam, set our speed to slow
				NodeController nodeController = node.gameObject.GetComponent<NodeController>();
				nodeController.DecreaseNodeSpeed(0);
			}
		}

	}

	void OnDestroy()
	{
		//Our beam is being destroyed, we need to slow the nodes if there was a collision
		if (collidingBeam != null)
		{
			foreach (GameObject node in ownerNodes)
			{
				NodeController nodeController = node.gameObject.GetComponent<NodeController>();
				nodeController.DecreaseNodeSpeed(0);
			}
		}
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
			//If we're colliding with another beam - remember what the beam is.
			collidingBeam = collider.gameObject;
		}
	}

}
