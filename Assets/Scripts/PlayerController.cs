using UnityEngine;
using System.Collections;
using Playscape.Analytics;

public class PlayerController : MonoBehaviour {
	public float speed;
	private string mPlayerName = null;
	private bool mIsMe = true;

	GameController.PlayerDescriptor mPlayerDescriptor;

	void Start() {

		if (GameState.CurrentGameType != GameState.GameType.SinglePlayer) {
			MultiplayerStart();
		}

		mPlayerDescriptor = GameController.Instnace.RegisterPlayer(mPlayerName, mIsMe);
	}

	void MultiplayerStart ()
	{
		if (!PhotonView.Get (this).owner.isMasterClient) {
			var material = Resources.Load<Material>("PlayerTwoMaterial");
			gameObject.renderer.material = material;


		}

		if (!PhotonView.Get (this).owner.isLocal) {
			mIsMe = false;

			if (GameState.CurrentGameType == GameState.GameType.MultiplayerPrivateGame) {
				mPlayerName = SocialController.Instance.OpponentFacebookUser.Name;
			} else {
				mPlayerName = "Nemesis";
			}
		}

	}


	void FixedUpdate() {

		if (mIsMe) {
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
	}

	void OnTriggerEnter(Collider other) {

		if (other.gameObject.tag == "PickUp") {
			other.gameObject.SetActive(false);
			mPlayerDescriptor.Score ++;

			Report.Instance.ReportEvent(string.Format("Collected/PlayerName:{0}/IsMe:{1}/Score:{2}", mPlayerDescriptor.Name ?? "Me", mPlayerDescriptor.IsMe, mPlayerDescriptor.Score));
		}
	}


}
