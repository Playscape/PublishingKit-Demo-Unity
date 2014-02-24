using UnityEngine;
using System.Collections;
using Playscape.Analytics;
using System.Collections.Generic;

public class LobbyMenuController : MonoBehaviour {
	private const int PLAYERS_COUNT = 2;
	private static int mMyPlayerId = MPUtils.GeneratePlayerId();

	abstract class IConnectionStateMachine
	{
		abstract public string Message { get; }

		public virtual IConnectionStateMachine Connected() { return this; }
		public virtual IConnectionStateMachine RoomJoined() { return this; }
		public virtual IConnectionStateMachine Failed() { return new FailState(); }
		public virtual IConnectionStateMachine PlayerJoined() { return this; }
	}

	class ConnectToPhoton: IConnectionStateMachine
	{
		private string mRoomName;

		public override string Message
		{
			get { return "Connecting..."; }
		}

		public ConnectToPhoton (string roomName)
		{
			mRoomName = roomName;
			PhotonNetwork.ConnectUsingSettings("1.0");
			Report.Instance.ReportMPServerConnect(PhotonNetwork.ServerAddress);
		}

		public ConnectToPhoton() : this(null)
		{

		}

		public override IConnectionStateMachine Connected()
		{
			if (mRoomName != null) {
				if (GameState.IsHost) {
					return new TryCreatingRoom(mRoomName);
				} else {
					return new TryJoiningExistingRoom(mRoomName);
				}
			} else {
				return new TryJoiningExistingRoom();
			}
		}
	}

	static class MPUtils {
		private static System.Random mRandom = new System.Random();
		public static string GenerateNetSessionId() {
			return (System.Math.Abs(mRandom.Next() | (mRandom.Next() << 31))).ToString();
		}

		public static int GeneratePlayerId() {
			return mRandom.Next();
		}

		public static string CreateRoomName(string roomName, bool isPrivate) {
			return string.Format("room_{0}_{1}", roomName, isPrivate ? "private" : "public");
		}

		/// <summary>
		/// Determines if the given room name is a private room.
		/// </summary>
		/// <returns><c>true</c> if the room is a private room; otherwise, <c>false</c>.</returns>
		/// <param name="roomName">Room name.</param>
		public static bool IsPrivateRoom(string roomName) {
			return roomName != null && roomName.EndsWith("private");
		}

		public static string CurrentRoomName {
			get {
				return PhotonNetwork.room != null ? PhotonNetwork.room.name : "null";
			}
		}
	}

	class TryJoiningExistingRoom: IConnectionStateMachine
	{
		public override string Message
		{
			get { return "Looking for existing rooms..."; }
		}

		public TryJoiningExistingRoom (string roomName)
		{
			Report.Instance.ReportMPJoinedPrivateGame(MPUtils.GenerateNetSessionId(), roomName, mMyPlayerId);
			PhotonNetwork.JoinRoom(roomName);
		}

		public TryJoiningExistingRoom()
		{
			Report.Instance.ReportMPJoinedPublicGame(MPUtils.GenerateNetSessionId(), "randomGame", mMyPlayerId);
			PhotonNetwork.JoinRandomRoom();
		}

		public override IConnectionStateMachine Failed()
		{
			return new TryCreatingRoom();
		}

		public override IConnectionStateMachine RoomJoined()
		{
			// Success
			return new SuccessState();
		}
	}

	class TryCreatingRoom: IConnectionStateMachine
	{
		private const string SESSION_ID = "sessionId";
		public override string Message
		{
			get { return "Creating new room..."; }
		}

		private ExitGames.Client.Photon.Hashtable CreateCustomProperties() {
			ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable();
			customProperties[SESSION_ID] = MPUtils.GenerateNetSessionId();
			return customProperties;
		}

		/// <summary>
		/// Creates a private game room
		/// </summary>
		/// <param name="roomName">Room name.</param>
		public TryCreatingRoom(string roomName) {
			Report.Instance.ReportMPCreatePrivateGame(MPUtils.GenerateNetSessionId(), roomName, PLAYERS_COUNT);

			PhotonNetwork.CreateRoom(roomName, false, true, PLAYERS_COUNT, CreateCustomProperties(), new string[]{ SESSION_ID });
		}

		public TryCreatingRoom()
		{
			string roomName = MPUtils.CreateRoomName((Random.value * 100000).ToString(), true);

			Report.Instance.ReportMPCreatePublicGame(MPUtils.GenerateNetSessionId(), PLAYERS_COUNT, new Dictionary<string, string>());

			PhotonNetwork.CreateRoom(roomName, true, true, PLAYERS_COUNT,  CreateCustomProperties(), new string[]{ SESSION_ID });

		}

		public override IConnectionStateMachine RoomJoined()
		{
#if UNITY_EDITOR
			return new SuccessState();
#else
			return new WaitForAnotherPlayer();
#endif
		}
	}

	class WaitForAnotherPlayer: IConnectionStateMachine
	{
		public override string Message
		{
			get { return "Waiting for another player..."; }
		}

		public override IConnectionStateMachine PlayerJoined()
		{
			// Success
			return new SuccessState();
		}
	}

	class FailState: IConnectionStateMachine
	{
		public override string Message
		{
			get { return "Failed"; }
		}
	}

	class SuccessState: IConnectionStateMachine
	{
		public override string Message
		{
			get { return "Success"; }
		}

		public SuccessState()
		{
			PhotonNetwork.room.open = false;
			PhotonNetwork.room.visible = false;
			Application.LoadLevel("level1");
		}
	}

	private IConnectionStateMachine currentState;

	class MultiplayerAnalyticsProvider : MPAnalyticsProvider {
		public long CurrentNetworkTime { get { return -1 /*network time is unused in this game.*/;} }
	}

	void Awake()
	{
		Report.Instance.InitMultiplayer(new MultiplayerAnalyticsProvider());


		if (GameState.CurrentGameType == GameState.GameType.MultiplayerPrivateGame) {
			StartPrivateMultiplayerGame ();
		} else if (GameState.CurrentGameType == GameState.GameType.MultiplayerPublicGame) {
			StartPublicMultiplayerGame ();
		} else {
			throw new System.ArgumentException("Invalid game type: " + GameState.CurrentGameType);
		}
	}

	void StartPrivateMultiplayerGame ()
	{
		if (SocialController.Instance.OpponentFacebookUser == null) {
			Debug.LogError ("Opponent is null!!!!");
		}
		if (SocialController.Instance.MyFacebookUser == null) {
			Debug.LogError ("FacebookId is null!!!!");
		}
		string roomName = "Invalid";
		if (GameState.IsHost) {
			roomName = MPUtils.CreateRoomName (SocialController.Instance.MyFacebookUser.FacebookId, true);
		}
		else {
			roomName = MPUtils.CreateRoomName (SocialController.Instance.OpponentFacebookUser.FacebookId, true);
		}
		Debug.Log ("Initiating private game");
		if (PhotonNetwork.insideLobby) {
			if (GameState.IsHost) {
				Debug.Log ("Private-Game: Starting as host");
				currentState = new TryCreatingRoom (roomName);
			}
			else {
				Debug.Log ("Private-Game: Starting as client");
				currentState = new TryJoiningExistingRoom (roomName);
			}
		}
		else {
			Debug.Log ("Private-Game: Connecting to photon");
			currentState = new ConnectToPhoton (roomName);
		}
	}

	void StartPublicMultiplayerGame ()
	{
		Debug.Log ("Initiating public game");
		if (PhotonNetwork.insideLobby) {
			Debug.Log ("Public-Game: Attempting to join room");
			currentState = new TryJoiningExistingRoom ();
		}
		else {
			Debug.Log ("Public-Game: Connecting to photon");
			currentState = new ConnectToPhoton ();
		}
	}

	// Update is called once per frame
	void Update ()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{ 
			PhotonNetwork.Disconnect();
			Application.LoadLevel("menu");
        }
    }

	void OnGUI()
	{
		GUILayout.BeginArea(new Rect(Screen.width / 2 - 250, 80, 500, 50));
		GUILayout.Box(currentState.Message);
		GUILayout.EndArea();
	}

	// Handle connection events

	public void OnJoinedLobby()
	{
		Debug.Log("OnConnectedToPhoton");
		currentState = currentState.Connected();
		Report.Instance.ReportMPServerConnectSuccess(PhotonNetwork.ServerAddress, System.TimeSpan.FromMilliseconds(100)/*Dummy ping should be replaced with real ping*/);
	}
	
	public void OnCreatedRoom()
	{
		GameState.IsHost = PhotonNetwork.isMasterClient;
		Debug.Log("OnCreatedRoom");
		currentState = currentState.RoomJoined();

	}
	
	public void OnJoinedRoom()
	{
		GameState.IsHost = PhotonNetwork.isMasterClient;
		Debug.Log("OnJoinedRoom");
		currentState = currentState.RoomJoined();
		if (GameState.CurrentGameType == GameState.GameType.MultiplayerPrivateGame) {
			Report.Instance.ReportMPJoinedPrivateGame(MPUtils.GenerateNetSessionId(), MPUtils.CurrentRoomName, mMyPlayerId);
		} else {
			Report.Instance.ReportMPJoinPublicGame(MPUtils.CurrentRoomName, mMyPlayerId, new Dictionary<string, string>());
		}
	}

	public void OnPlayerDisconnected() {
		Debug.Log("OnPhotonPlayerDisconnected");
		GameState.IsHost = PhotonNetwork.isMasterClient;
	}

	public void OnPhotonPlayerConnected()
	{
		Debug.Log("OnPhotonPlayerConnected");
		currentState = currentState.PlayerJoined();
	}
	
	public void OnLeftRoom()
	{
		Debug.Log("OnLeftRoom");
		currentState = currentState.Failed();
		Report.Instance.ReportMPLeaveGame(MPUtils.CurrentRoomName);
	}
	
	public void OnPhotonCreateRoomFailed()
	{
		Debug.Log("OnPhotonCreateRoomFailed");
		currentState = currentState.Failed();

	}
	
	public void OnPhotonRandomJoinFailed()
	{
		Debug.Log("OnPhotonRandomJoinFailed");
		currentState = currentState.Failed();
	}
	
	public void OnDisconnectedFromPhoton()
	{
		Debug.Log("OnDisconnectedFromPhoton");
		currentState = currentState.Failed();
		Report.Instance.ReportMPServerDisconnect();
	}
	
	public void OnFailedToConnectToPhoton()
	{
		Debug.Log("OnFailedToConnectToPhoton");
		currentState = currentState.Failed();
		Report.Instance.ReportMPServerError("OnFailedToConnectToPhoton");
	}
}
