/*
 */
using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;

namespace KQDeskop
{
	/// <summary>
	/// A class to send data to KQ Analytics
	/// </summary>
	public class KQAnalyticsClient
	{
		public string KQScriptPath;
		public string TrackingId;
		
		string _kqSessionId;
		static string _kqSessionIdFn = "_kqSessId";
		
		/// <summary>
		/// Initializes a new KQ Analytics Client object
		/// </summary>
		/// <param name="kqScriptPath">The path to `kq.php` on the server</param>
		/// <param name="trackingId">The tracking ID to associate traffic from this client with</param>
		public KQAnalyticsClient(string kqScriptPath, string trackingId) : this(kqScriptPath, trackingId, LoadSessionId())
		{
			
		}
		
		/// <summary>
		/// Initializes a new KQ Analytics Client object with the Session ID set
		/// </summary>
		/// <param name="kqScriptPath"></param>
		/// <param name="trackingId"></param>
		/// <param name="kqSessionId"></param>
		protected KQAnalyticsClient(string kqScriptPath, string trackingId, string kqSessionId)
		{
			KQScriptPath = kqScriptPath;
			TrackingId = trackingId;
			this._kqSessionId = kqSessionId;
			SaveSessionId(this._kqSessionId);
		}
		
		private static void SaveSessionId(string sessionId)
		{
			IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);			
			using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(_kqSessionIdFn, FileMode.CreateNew, isoStore))
            {
                using (StreamWriter writer = new StreamWriter(isoStream))
                {
                    writer.WriteLine(sessionId);
                }
            }
		}
		
		private static string LoadSessionId()
		{
			string sessionId;
			IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
			if (isoStore.FileExists(_kqSessionIdFn))
			{
				using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(_kqSessionIdFn, FileMode.Open, isoStore))
				{
					using (StreamReader reader = new StreamReader(isoStream))
					{
						sessionId = reader.ReadToEnd();
					}
				}
			}
			else
			{
				sessionId = RandomString(26);
			}
			return sessionId;
		}
		
		public static string RandomString(int length)
		{
		    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		    var random = new Random();
		    return new string(Enumerable.Repeat(chars, length)
		      .Select(s => s[random.Next(s.Length)]).ToArray());
		}
		
		public void SendRequest(string payload="KQDesktop")
		{
			var wc = new WebClient();
			string requestUrl = string.Format("{0}?url={1}&sessid={2}&tid={3}", KQScriptPath, Uri.EscapeDataString(payload), _kqSessionId, TrackingId);
			wc.DownloadString(requestUrl);
		}
	}
}
