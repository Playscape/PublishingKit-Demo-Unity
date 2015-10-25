using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Playscape.Internal;

namespace Playscape.ABTesting {

	/// <summary>
	/// ExperimentData - experiment data which includes the experiment name, experiment group the user joined, and the experiment actual values
	/// </summary>
	public class ExperimentData {

		/// <summary>
		/// Native constant sperator
		/// </summary>
		private const string ABTESTING_PAYLOAD_SEPARATOR = "___:";
		/// <summary>
		/// Native minmum payload size (experiment name and experiment group)
		/// </summary>
		private const int ABTESTING_PAYLOAD_MIN_PARTS_SIZE = 2;

		/// <summary>
		/// Dictionary from experiment variable name to experiment variable value
		/// <summary>
		private IDictionary<string,string> mExperimentValuesMap;
		/// <summary>
		/// The experiment name
		/// <summary>
		private string mExperimentName;
		/// <summary>
		/// The group which the user joined in the experiment
		/// <summary>
		private string mExperimentGroup;

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="experimentName">The experiment name </param>
		/// <param name="experimentGroup">The group which the user joined in the experiment </param>
		/// <param name="experimentValuesMap">Dictionary from experiment variable name to experiment variable value</param>
		private ExperimentData(string experimentName, string experimentGroup, IDictionary<string, string> experimentValuesMap)
		{
			this.mExperimentName = experimentName;
			this.mExperimentGroup = experimentGroup;
			this.mExperimentValuesMap = experimentValuesMap;
		}

		/// <summary>
		/// Returns the experiment name
		/// <summary>
		/// <value>Experiment Name</value>
		public string getExperimentName()
		{
			return this.mExperimentName;
		}

		/// <summary>
		/// Returns the group which the user joined in the experiment
		/// <summary>
		/// <value>Experiment Group</value>
		public string getExperimentGroup()
		{
			return this.mExperimentGroup;
		}

		/// <summary>
		/// Returns dictionary from experiment variable name to experiment variable value
		/// <summary>
		/// <value>Experiment variables</value>
		public IDictionary<string,string>  getExperimentValues()
		{
			return this.mExperimentValuesMap;
		}

		/// <summary>
		/// translate internal payload from native code to unity ExperimentData object
		/// </summary>
		/// <param name="payload">Payload is a string that looks like this: 
		/// 					  ExperimentName___:ExperimentGroup___:ExperimentKey1___:ExperimentValue1___:ExperimentKey2___:ExperimentValue2___:
		/// </param>
		/// <returns>Experiment data translated from payload</returns>
		internal static ExperimentData fromPayload(string payload) {
			string [] payloadParts = payload.Split(new string [] {ABTESTING_PAYLOAD_SEPARATOR},System.StringSplitOptions.RemoveEmptyEntries);
			if (payloadParts.Length >= ABTESTING_PAYLOAD_MIN_PARTS_SIZE && 
			    payloadParts.Length % 2 == 0) {
				string experimentName = payloadParts[0];
				string experimentGroup = payloadParts[1];
				IDictionary<string, string> experimentValues = new Dictionary<string, string>();
				for (int i = 2; i < payloadParts.Length; i = i+2)
				{
					experimentValues.Add(new KeyValuePair<string, string>(payloadParts[i],payloadParts[i+1]));
				}
				return new ExperimentData(experimentName,experimentGroup,experimentValues);

			} else {
				L.E ("Payload size is unexpected: {0}, expecting at least two parts and each key has value therefore it must be even", payloadParts.Length);
					return null;
			}
		}

	}
}









	

	

	

	


