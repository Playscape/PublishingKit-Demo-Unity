using UnityEngine;
using System.Collections;

/// <summary>
/// Class which shares game state between all other classes
/// </summary>
public static class GameState {
	public enum GameType {
		SinglePlayer,
		MultiplayerPrivateGame,
		MultiplayerPublicGame
	}

	public static bool IsHost = false;
	public static GameType CurrentGameType = GameType.SinglePlayer;
}
