using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
	public float speed;

	void FixedUpdate() {
		float moveHortizontal = Input.GetAxis ("Horizontal");
		float moveVertical = Input.GetAxis ("Vertical");

		Vector3 movement = new Vector3 (moveHortizontal, 0.0f, moveVertical);
		rigidbody.AddForce (movement * speed * Time.deltaTime);
	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.tag == "PickUp") {
			other.gameObject.SetActive(false);
		}
	}
}
