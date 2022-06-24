using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ExplosionController : MonoBehaviour
{

	public AudioSource explosionAudioSource;

	public float YForce;
	public float maxForce;
	public float destroyAfter;

	public void BeginExplosion(Color colour)
	{

		explosionAudioSource.Play();

		Sequence chunkFadeOut;
		chunkFadeOut = DOTween.Sequence();

		foreach (Transform child in transform)
		{
			//Get the child components
			MeshRenderer meshChild = child.gameObject.GetComponent<MeshRenderer>();
			Rigidbody rbChild = child.gameObject.GetComponent<Rigidbody>();

			//Apply the colours
			meshChild.material.color = colour;

			chunkFadeOut.Join(meshChild.material.DOFade(0, destroyAfter));

			//Add the force
			rbChild.AddForce(RandomVector(false));
			rbChild.AddTorque(RandomVector(true));
		}

		chunkFadeOut.OnComplete(destroyThisObject);
		chunkFadeOut.SetEase(Ease.InQuad);
		chunkFadeOut.Play();

	}

	Vector3 RandomVector(bool isTorque)
	{
		if (isTorque == true)
		{ return new Vector3(RandomNumber() * 10, RandomNumber() * 10, RandomNumber() * 10); }
		else
		{ return new Vector3(RandomNumber(), YForce, RandomNumber()); }
	}

	float RandomNumber()
	{
		return Random.Range(-maxForce, maxForce);
	}

	void destroyThisObject()
	{
		Destroy(gameObject);
	}

}
