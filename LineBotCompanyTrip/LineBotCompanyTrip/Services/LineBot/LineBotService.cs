using LineBotCompanyTrip.Configurations;
using LineBotCompanyTrip.Services.Emotion;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace LineBotCompanyTrip.Services.LineBot {

	/// <summary>
	/// LINE Botに関するサービス
	/// </summary>
	public class LineBotService {
		
		/// <summary>
		/// Contentから画像、動画、音声にアクセスするAPIを呼び、バイナリデータを返す
		/// </summary>
		/// <param name="messageId">メッセージID</param>
		/// <returns>バイナリデータ</returns>
		public async Task<Stream> GetContent( string messageId ) {

			HttpClient client = new HttpClient();
			client.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );
			client.DefaultRequestHeaders.Add( "Authorization" , "Bearer {" + LineBotConfig.ChannelAccessToken + "}" );

			HttpResponseMessage response = await client.GetAsync( LineBotConfig.GetContentUrl( messageId ) ).ConfigureAwait( false );
			Stream result = await response.Content.ReadAsStreamAsync().ConfigureAwait( false );
		
			return result;
			
		}
		
	}

}