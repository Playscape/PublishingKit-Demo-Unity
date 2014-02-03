using UnityEngine;
using System.Collections;

public class InitializerMultiplayer : MonoBehaviour {

	public GameObject camera;
	
	// Use this for initialization
	void Start()
	{
		float xPosition = 6.5f;

		if (PhotonNetwork.player.isMasterClient)
		{
			xPosition *= -1f;
		}

		GameObject player =
			PhotonNetwork.Instantiate("Player",
			                          new Vector3(xPosition, 0.5f, 0f),
			                          Quaternion.identity,
			                          0);

		player.GetComponent<PlayerController>().isLocal = true;
		camera.GetComponent<CameraController>().player = player;
	}
}
