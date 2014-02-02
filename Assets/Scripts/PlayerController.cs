using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
	public float speed;
	
	public GameObject MyHUD;

	private const string PICKUP_TAG = "PickUp";
	private int collected = 0;
	private int total = 0;


	void Start() {
		total = GameObject.FindGameObjectsWithTag(PICKUP_TAG).Length;
	}

	void FixedUpdate() {
		float moveHortizontal = Input.GetAxis ("Horizontal");
		float moveVertical = Input.GetAxis ("Vertical");

		if (Application.platform == RuntimePlatform.Android ||  Application.platform == RuntimePlatform.IPhonePlayer) {
			moveHortizontal = Input.acceleration.x;
			moveVertical = Input.acceleration.y + 0.5f;
		} 

		if (rigidbody != null) {
			Vector3 movement = new Vector3 (moveHortizontal, 0.0f, moveVertical);
			rigidbody.AddForce (movement * speed * Time.deltaTime);
		}
	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.tag == "PickUp") {
			other.gameObject.SetActive(false);
			collected ++;
			MyHUD.SendMessage("OnCollected", collected.ToString());

			if (collected == total) {
				MyHUD.SendMessage("OnVictory");
			}
		}
	}

}
