using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Chartboost;
using Playscape.Analytics;

public class GameController : MonoBehaviour {

	private static GameController mInstance;
	private const string PICKUP_TAG = "PickUp";

	private int mTotalCollectibles;
	private IList<PlayerDescriptor> mPlayers = new List<PlayerDescriptor>();

	public static GameController Instnace {
		get {
			if (mInstance == null) {
				var go = GameObject.Find("GameController");
				if (go != null) {
					mInstance = go.GetComponent<GameController>();
				}
			}
			
			return mInstance;
		}
	}

	void OnDestroy() {
		mInstance = null;
	}

	const string GAME_AB_TEST_SEGMENT_PREF = "Game.ABTestSegment";

	// Use this for initialization
	void Start()
	{

		int abTestVal = PlayerPrefs.GetInt(GAME_AB_TEST_SEGMENT_PREF, 0);

		if (abTestVal == 0) {
			System.Random rand = new System.Random((int)(Playscape.Utils.CurrentTimeMillis/1000));
			abTestVal = rand.Next();

			PlayerPrefs.SetInt (GAME_AB_TEST_SEGMENT_PREF, abTestVal);
			PlayerPrefs.Save();
		}

		Report.Instance.CustomVariables["ABTestSegment"] = abTestVal.ToString();

		Report.Instance.ReportLevelStarted(Application.loadedLevelName, new Dictionary<string, double>());


		mTotalCollectibles = GameObject.FindGameObjectsWithTag(PICKUP_TAG).Length;

		GameObject player = null;
		float xPosition = 6.5f;

		if (GameState.CurrentGameType != GameState.GameType.SinglePlayer) {
			// Multiplayer
			if (PhotonNetwork.player.isMasterClient)
			{
				xPosition *= -1f;
			}

			player =
				PhotonNetwork.Instantiate("Player",
				                          new Vector3(xPosition, 0.5f, 0f),
				                          Quaternion.identity,
				                          0);
		} else {
			// Single Player
			player = (GameObject)Resources.Load("Player", typeof(GameObject));
			player = (GameObject)Instantiate(player);
		}

		var mainCamera = GameObject.Find("Main Camera");
		mainCamera.GetComponent<CameraController>().player = player;
	}

	void Update() {
		CheckVictoryConditions();
		UpdateHud();

		if (Input.GetKeyDown(KeyCode.Escape)) { 
			PhotonNetwork.Disconnect();
		}
	}

	public class PlayerDescriptor {
		public PlayerDescriptor(string name, bool isMe) {
			Name = name;
			IsMe = isMe;
			Score = 0;
		}
		public int Score {get;set;}
		public string Name {get; private set;}
		public bool IsMe {get; private set;}
	}

	public PlayerDescriptor RegisterPlayer(string playerName = null, bool isMe = true) {
		var desc = new PlayerDescriptor(playerName, isMe);
		mPlayers.Add(desc);

		Debug.Log(string.Format("Registered player, name: {0}, isMe: {1}", playerName, isMe));
		return desc;
	}

	
	void CheckVictoryConditions ()
	{
		int totalCollected = 0;

		foreach (var desc in mPlayers) {
			totalCollected += desc.Score; 
		}

		if (totalCollected < mTotalCollectibles) {
			return; // No need to go further, haven't collected everything
		}

		if (GameState.CurrentGameType == GameState.GameType.SinglePlayer) {

			if (mTotalCollectibles == totalCollected) {
				HUD.Instance.Victory();
			}
		} else {
			if (mPlayers.Count == 2) {
				PlayerDescriptor me = mPlayers[0].IsMe ? mPlayers[0] : mPlayers[1];;
				PlayerDescriptor opponent = !mPlayers[0].IsMe ? mPlayers[0] : mPlayers[1];

				if (opponent.Score > me.Score) {
					HUD.Instance.Victory(opponent.Name);
				} else {
					HUD.Instance.Victory();
				}
			}

		}
	}

	public void GoToNextLevel ()
	{
		if (Application.loadedLevelName == "level1") {
			Application.LoadLevel("level2");
		} else {
			SwitchLevel("menu");
		}

		Report.Instance.ReportLevelCompleted(Application.loadedLevelName, null);

		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		CBBinding.showInterstitial(null);
		#endif
	}

	public void SwitchLevel (string level)
	{
		if (GameState.CurrentGameType != GameState.GameType.SinglePlayer) {
			StartCoroutine (DoSwitchLevel(level));
		} else {
			Application.LoadLevel(level);
		}
	}
	
	IEnumerator DoSwitchLevel (string level)
	{
		PhotonNetwork.Disconnect ();
		while (PhotonNetwork.connected)
			yield return null;
		Application.LoadLevel(level);
	}

	void UpdateHud ()
	{
		if (GameState.CurrentGameType == GameState.GameType.SinglePlayer) {
			if (mPlayers.Count == 1) {
				HUD.Instance.OnCollected(mPlayers[0].Score);
			}
		} else {
			
			if (mPlayers.Count == 2) {
				PlayerDescriptor me = mPlayers[0].IsMe ? mPlayers[0] : mPlayers[1];;
				PlayerDescriptor opponent = !mPlayers[0].IsMe ? mPlayers[0] : mPlayers[1];

				HUD.Instance.OnCollected(me.Score, opponent.Score);
			}
		}
	}
}
