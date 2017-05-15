using LineBotCompanyTrip.Configurations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace LineBotCompanyTrip.Controllers {

	/// <summary>
	/// LINE BotからのWebhook送信先API
	/// </summary>
	public class WebhookController : ApiController {

		/// <summary>
		/// Reply Messageに使用するリクエストEntity
		/// </summary>
		private class RequestOfReplyMessage {

			/// <summary>
			/// リプライメッセージ
			/// </summary>
			public class Message {

				/// <summary>
				/// メッセージ種別
				/// </summary>
				public string type;

				/// <summary>
				/// メッセージ本文
				/// </summary>
				public string text;

			}

			/// <summary>
			/// 返信に必要なリプライトークン
			/// </summary>
			public string replyToken;

			/// <summary>
			/// リプライメッセージ(最大5通)
			/// </summary>
			public Message[] messages;

		}

		/// <summary>
		/// POSTメソッド
		/// </summary>
		/// <param name="requestToken">リクエストトークン</param>
		/// <returns>常にステータス200のみを返す</returns>
		public async Task<HttpResponseMessage> Post( JToken requestToken ) {
			
			#region 入力チェックと通知から必要な情報の取得
			string replyToken = "";
			string sentMessage = "";
			{
				foreach( JToken events in requestToken[ "events" ] ) {
					replyToken = events[ "replyToken" ].ToString();
					sentMessage = events[ "message" ][ "text" ].ToString();
				}
			}
			#endregion

			#region 通知に対するリプライを返す
			{
				StringContent content = this.createContent( replyToken );
				
				HttpClient client = new HttpClient();
				client.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );
				client.DefaultRequestHeaders.Add( "Authorization" , "Bearer {" + LineBotConfig.ChannelAccessToken + "}" );

				HttpResponseMessage response = await client.PostAsync( LineBotConfig.ReplyMessageUrl , content ).ConfigureAwait( false );
				string result = await response.Content.ReadAsStringAsync().ConfigureAwait( false );
					
			}
			#endregion

			//常にステータス200を返す
			return new HttpResponseMessage( HttpStatusCode.OK );

		}

		/// <summary>
		/// 返信用Content作成
		/// </summary>
		/// <param name="replyToken">リプライトークン</param>
		/// <returns>Content</returns>
		private StringContent createContent( string replyToken ) {

			RequestOfReplyMessage requestObject = new RequestOfReplyMessage();
			requestObject.replyToken = replyToken;
			RequestOfReplyMessage.Message message = new RequestOfReplyMessage.Message();
			message.type = "text";
			message.text = "test";
			requestObject.messages = new RequestOfReplyMessage.Message[ 1 ];
			requestObject.messages[ 0 ] = message;

			string jsonRequest = JsonConvert.SerializeObject( requestObject );
			StringContent content = new StringContent( jsonRequest );
			content.Headers.ContentType = new MediaTypeHeaderValue( "application/json" );
			
			return content;

		}


	}

}

