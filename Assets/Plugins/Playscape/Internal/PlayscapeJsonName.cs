using System;
using Pathfinding.Serialization.JsonFx;

namespace Playscape.Internal
{
	/// <summary>
	/// Attribute for set name of field in json.
	/// </summary>

	public class PlayscapeJsonName : JsonNameAttribute
	{
		//Indicates availability of field for traversing in Configuration class
		public bool Traversable;

		public PlayscapeJsonName (String jsonName) : base (jsonName)
		{
			this.Traversable = true;
		}

		public PlayscapeJsonName (String jsonName, bool traversable) : base (jsonName)
		{
			this.Traversable = traversable;
		}
	}
}

