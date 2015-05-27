using System;
using System.Net;
using System.Runtime.Serialization;
using System.IO;
using Pathfinding.Serialization.JsonFx;
using System.Threading;
using System.Net.NetworkInformation;

namespace Playscape.Internal
{
	/// <summary>
	/// SyncRequest objects represent a URL load request
	/// </summary>
	// 
	// T - T, the type of the result of the request performing
	//
	public class SyncRequest<T> {		
		private const int DEFAULT_TIMEOUT = 60000;

		//
		public string Url { get; private set; }
		public string Method { get; private set; }

		private WebRequest Request;

		/// <summary>
		/// Returns a URL request for a specified Url and HTTP method
		/// 
		/// </summary>
		/// <param name="Url">The URL for the request.</param>
		/// <param name="Method">The HTTP method for the request.</param>
		public SyncRequest (string Url, string Method)
		{
			this.Url = Url;
			this.Method = Method;

			setupRequest ();
		}

		/// <summary>
		/// Setups the request.
		/// </summary>
		private void setupRequest() {
			if (!string.IsNullOrEmpty (Url)) {							
				ServicePointManager.ServerCertificateValidationCallback += (o, ICertificatePolicy, chain, errors) => true;

				HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create (Url);
				webRequest.Method = Method;
				webRequest.Timeout = DEFAULT_TIMEOUT;

				this.Request = webRequest;
			}
		}


		/// <summary>
		/// Sets content type of request
		/// </summary>
		/// <param name="ContentType">Content type.</param>
		public void setContentType(string ContentType) {
			this.Request.ContentType = ContentType;
		}


		/// <summary>
		/// Adds the header.
		/// </summary>
		/// <param name="headerName">The name of the header.</param>
		/// <param name="headerValue">The value of the header.</param>
		public void addHeader(string headerName, string headerValue) {
			this.Request.Headers.Add (headerName, headerValue);
		}


		/// <summary>
		/// Processes the response stream.
		/// </summary>
		/// <returns>The response stream.</returns>
		/// <param name="stream">Stream.</param>
		private T processResponseStream(Stream stream) {
			StreamReader reader = new StreamReader (stream);
			string responseString = reader.ReadToEnd ();
			reader.Close ();

			T responseObject = JsonReader.Deserialize<T> (responseString);
			return responseObject;
		}

		/// <summary>
		/// Start executing of this request.
		/// </summary>
		public T Start() { 
			T response = default(T);

			if (this.Request != null) {
				WebResponse webResponse = null;

				try {
					webResponse = this.Request.GetResponse ();
					response = processResponseStream (webResponse.GetResponseStream ());
				} catch (Exception e) {
					throw e;
				}
			}

			return response;
		}
	}
}
