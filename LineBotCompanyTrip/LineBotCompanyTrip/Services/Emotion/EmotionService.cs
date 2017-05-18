using LineBotCompanyTrip.Configurations;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace LineBotCompanyTrip.Services.Emotion {

	/// <summary>
	/// Azure Cognitive Services Emotion APIに関するサービスクラス
	/// </summary>
	public class EmotionService {

		/// <summary>
		/// Emotion APIを呼び出す
		/// </summary>
		/// <param name="binaryImage">画像のバイナリデータ</param>
		/// <returns>APIレスポンス</returns>
		public async Task<string> Call( Stream binaryImage ) {

			StreamContent content = new StreamContent( binaryImage );
			content.Headers.ContentType = new MediaTypeHeaderValue( "application/octet-stream" );

			HttpClient client = new HttpClient();
			client.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/octet-stream" ) );
			client.DefaultRequestHeaders.Add( "Ocp-Apim-Subscription-Key" , EmotionConfig.OcpApimSubscriptionKey );

			HttpResponseMessage response = await client.PostAsync( EmotionConfig.EmotionApiUrl , content ).ConfigureAwait( false );
			string result = await response.Content.ReadAsStringAsync().ConfigureAwait( false );
			return result;

		}

	}

}