using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Playscape.Internal;

namespace Playscape.ABTesting {

	public class ExperimentData {

		private const string ABTESTING_PAYLOAD_SEPARATOR = "___:";
		private const int ABTESTING_PAYLOAD_MIN_PARTS_SIZE = 2;

		private IDictionary<string,string> mExperimentValuesMap;
		private string mExperimentName;
		private string mExperimentGroup;

		private ExperimentData(string experimentName, string experimentGroup, IDictionary<string, string> experimentValuesMap)
		{
			this.mExperimentName = experimentName;
			this.mExperimentGroup = experimentGroup;
			this.mExperimentValuesMap = experimentValuesMap;
		}


		public string getExperimentName()
		{
			return this.mExperimentName;
		}
		
		public string getExperimentGroup()
		{
			return this.mExperimentGroup;
		}
		
		public IDictionary<string,string>  getExperimentValues()
		{
			return this.mExperimentValuesMap;
		}

		public static ExperimentData fromPayload(string payload) {
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









	

	

	

	


