using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ReviveIconController : MonoBehaviour
{

	public GameObject nodeToRevive;
	public GameObject revivingNode;

	public float fadeInTime;
	public float reviveReady;

	bool canRevive;

	public void SetNodeToRevive(GameObject destroyedNode, GameObject twinNode)
	{

		canRevive = false;

		//Debug.Log("ReviveIconPrefab now exists");
		//Debug.Log("Setting nodeToRevive and revivingNode");

		nodeToRevive = destroyedNode;
		revivingNode = twinNode;

		//Debug.Log("ReviveIcon now has the following vars:");

		//Debug.Log("- Node to revive is " + nodeToRevive);
		//Debug.Log("- Reviving node is " + revivingNode);

		//Debug.Log("Revive icon has been instantiated, setting " + destroyedNode + " to inactive");
		destroyedNode.SetActive(false);

		StartCoroutine(ShowReviveIcon(reviveReady));

	}

	private IEnumerator ShowReviveIcon(float delayLength)
	{
		yield return new WaitForSeconds(delayLength);

		canRevive = true;

		GetComponent<ParticleSystem>().Play();

		Sequence ShowReviveIcon;
		ShowReviveIcon = DOTween.Sequence();

		ShowReviveIcon.Join(GetComponent<MeshRenderer>().material.DOFade(1, fadeInTime));

		foreach (Transform child in transform)
		{
			ShowReviveIcon.Join(child.GetComponent<SpriteRenderer>().DOFade(1, fadeInTime));
		}

		ShowReviveIcon.Play();

	}

	void ReviveIconReady()
	{
		canRevive = true;
	}

	void OnTriggerEnter(Collider collider)
	{

		//Debug.Log("Collision has been detected. The reviving node var is: " + revivingNode);

		// if (collider.gameObject.tag == "Node" && canRevive == true)
		// {
		// 	//	Debug.Log("Revive icon has detected a collsion with " + collider.gameObject);
		// 	//Debug.Log("Node can only be revived by " + revivingNode);

		// 	if (collider.gameObject == revivingNode)
		// 	{
		// 		ReviveNode();
		// 	}
		// }
	}

	void OnTriggerStay(Collider collider)
	{
		if (collider.gameObject.tag == "Node" && canRevive == true)
		{
			//	Debug.Log("Revive icon has detected a collsion with " + collider.gameObject);
			//Debug.Log("Node can only be revived by " + revivingNode);

			if (collider.gameObject == revivingNode)
			{
				ReviveNode();
			}
		}
	}

	void ReviveNode()
	{
		//Debug.Log("Reviving node!");
		nodeToRevive.SetActive(true);
		NodeController nodeController = nodeToRevive.GetComponent<NodeController>();
		nodeController.Respawn();

		//Destroy this revive icon as revive has occurred
		Destroy(gameObject);
	}

	public void SetColours(Color colour)
	{
		MeshRenderer myMesh = GetComponent<MeshRenderer>();
		myMesh.material.color = new Color(colour.r, colour.g, colour.b, 0);

		SpriteRenderer mySprite = GetComponentInChildren<SpriteRenderer>();
		mySprite.color = new Color(colour.r, colour.g, colour.b, 0);

		ParticleSystem myParticles = GetComponent<ParticleSystem>();
		var myParticlesVar = myParticles.main;
		myParticlesVar.startColor = colour;
		myParticles.Stop();
	}

}
