using UnityEngine;
using System.Collections;

public class LobbyMenuController : MonoBehaviour {

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
		public override string Message
		{
			get { return "Connecting..."; }
		}

		public ConnectToPhoton()
		{
			PhotonNetwork.ConnectUsingSettings("1.0");
		}

		public override IConnectionStateMachine Connected()
		{
			return new TryJoiningExistingRoom();
		}
	}

	class TryJoiningExistingRoom: IConnectionStateMachine
	{
		public override string Message
		{
			get { return "Looking for existing rooms..."; }
		}

		public TryJoiningExistingRoom()
		{
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
		public override string Message
		{
			get { return "Creating new room..."; }
		}
		
		public TryCreatingRoom()
		{
			string roomName = string.Format("GameRoom_{0}", (int) (Random.value * 100000));

			PhotonNetwork.CreateRoom(roomName, true, true, 2);
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

	void Awake()
	{
		if (PhotonNetwork.insideLobby)
		{
			currentState = new TryJoiningExistingRoom();
		}
		else
		{
			currentState = new ConnectToPhoton();
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
	}
	
	public void OnCreatedRoom()
	{
		Debug.Log("OnCreatedRoom");
		currentState = currentState.RoomJoined();
	}
	
	public void OnJoinedRoom()
	{
		Debug.Log("OnJoinedRoom");
		currentState = currentState.RoomJoined();
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
	}
	
	public void OnFailedToConnectToPhoton()
	{
		Debug.Log("OnFailedToConnectToPhoton");
		currentState = currentState.Failed();
	}
}
