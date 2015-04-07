using System;
using System.Net;
using System.Runtime.Serialization;
using System.IO;
using Pathfinding.Serialization.JsonFx;
using System.Threading;
using System.Net.NetworkInformation;

namespace Playscape.Internal
{
	public class AsyncRequest<T> where T : AsyncResponse
	{		
		private const int DEFAULT_TIMEOUT = 60000;

		public string Url { get; private set; }	
		public string Method { get; private set; }
		
		private RequestState requestState;
		private IAsyncResult result;
		private Action<T> callback;
		private bool isExecuting;
			
		public AsyncRequest (string Url, string Method)
			:this (Url, Method, null)
		{

		}

		public AsyncRequest (string Url, string Method, Action<T> callback) 
		{
			if (string.IsNullOrEmpty (Url)) {
				throw new ArgumentNullException("Url");
			}

			if (string.IsNullOrEmpty (Method)) {
				throw new ArgumentNullException("Method");
			}

			this.Url 			= Url;
			this.Method 		= Method;
			this.callback 		= callback;
			this.requestState 	= new RequestState ();
			
			setupRequest ();				
		}

		private void setupRequest() {
			if (!string.IsNullOrEmpty (Url)) {
				HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create (Url);
				webRequest.Method = Method;
				webRequest.Timeout = DEFAULT_TIMEOUT;
				
				this.requestState.Request = webRequest;
			}
		}
		
		public void setContentType(string ContentType) {
			this.requestState.Request.ContentType = ContentType;
		}
		
		public void addHeader(string headerName, string headerValue) {
			this.requestState.Request.Headers.Add (headerName, headerValue);
		}

		private T processResponseStream(Stream stream) {
			StreamReader reader = new StreamReader (stream);
			string responseString = reader.ReadToEnd ();
			reader.Close ();

			T responseObject = JsonReader.Deserialize<T> (responseString);
			return responseObject;
		}

		private void processResponseStreamAsync(IAsyncResult result) {
			RequestState rs = (RequestState) result.AsyncState;
			WebRequest request = rs.Request;

			WebResponse webResponse = request.EndGetResponse (result);

			T response = processResponseStream (webResponse.GetResponseStream());

			reportFinished (response);
		}

		private void reportFinished(T response) {
			this.result = null;

			if (callback != null) {
				callback(response);
			}
		}

		public T Start() {		
			T response = default(T);

			if (this.requestState.Request != null) {
				WebResponse webResponse = null;

				try {
					webResponse = this.requestState.Request.GetResponse ();
					response = processResponseStream (webResponse.GetResponseStream ());
				} catch (Exception e) {
					L.E ("An error occured while fetching Game Configuration: " + e.Message);
					response = (T)Activator.CreateInstance (typeof(T));
					response.Error = e;
				} 
			}

			return response;
		}	

		public void StartAsync() {
			if (this.result == null) {
				this.result = this.requestState.Request.BeginGetResponse (new AsyncCallback (processResponseStreamAsync), this.requestState); 
			}
		}

		
		private class RequestState
		{
			public WebRequest Request;
			public RequestState()
			{
				Request = null;
			}     
		}
	}

	public class AsyncResponse {
		public Exception Error;
	}
}

