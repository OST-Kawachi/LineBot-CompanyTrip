using LineBotCompanyTrip.Configurations;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace LineBotCompanyTrip.Services.Emotion {

	/// <summary>
	/// Azure Cognitive Services Emotion APIに関するサービスクラス
	/// </summary>
	public class EmotionService {

		/// <summary>
		/// EmotionAPIに使用するリクエストEntity
		/// </summary>
		private class RequestsOfEmotion {

			/// <summary>
			/// URL
			/// </summary>
			public string url;

		}
		
		/// <summary>
		/// Emotion APIを呼び出す
		/// </summary>
		/// <param name="imageUrl">画像パス</param>
		/// <returns>APIレスポンス</returns>
		public async Task<string> Call( string imageUrl ) {

			RequestsOfEmotion requestObject = new RequestsOfEmotion();
			requestObject.url = imageUrl;
			string jsonRequest = JsonConvert.SerializeObject( requestObject );
			StringContent content = new StringContent( jsonRequest );
			content.Headers.ContentType = new MediaTypeHeaderValue( "application/json" );
			
			HttpClient client = new HttpClient();
			client.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );
			client.DefaultRequestHeaders.Add( "Ocp-Apim-Subscription-Key" , EmotionConfig.OcpApimSubscriptionKey );

			HttpResponseMessage response = await client.PostAsync( EmotionConfig.EmotionApiUrl , content ).ConfigureAwait( false );
			string result = await response.Content.ReadAsStringAsync().ConfigureAwait( false );
			return result;

		}
		
	}

}