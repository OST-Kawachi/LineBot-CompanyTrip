
using LineBotCompanyTrip.Configurations;
using LineBotCompanyTrip.Services.Emotion;
using Newtonsoft.Json;
using System.Diagnostics;
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
		/// Reply Messageに使用するリクエストEntity
		/// </summary>
		public class RequestOfReplyMessage {

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
		/// フォロー時のイベント
		/// </summary>
		/// <param name="replyToken">リプライトークン</param>
		/// <returns></returns>
		public async Task CallFollowEvent( string replyToken ) {

			RequestOfReplyMessage requestObject = new RequestOfReplyMessage();
			requestObject.replyToken = replyToken;
			RequestOfReplyMessage.Message message = new RequestOfReplyMessage.Message();
			message.type = "text";
			message.text = "友達追加ありがとうございます！\n仲良くしてくださいね！";
			requestObject.messages = new RequestOfReplyMessage.Message[ 1 ];
			requestObject.messages[ 0 ] = message;

			string jsonRequest = JsonConvert.SerializeObject( requestObject );
			StringContent content = new StringContent( jsonRequest );
			content.Headers.ContentType = new MediaTypeHeaderValue( "application/json" );
			
			HttpClient client = new HttpClient();
			client.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );
			client.DefaultRequestHeaders.Add( "Authorization" , "Bearer {" + LineBotConfig.ChannelAccessToken + "}" );

			HttpResponseMessage response = await client.PostAsync( LineBotConfig.ReplyMessageUrl , content ).ConfigureAwait( false );
			string result = await response.Content.ReadAsStringAsync().ConfigureAwait( false );
			
		}

		/// <summary>
		/// グループ追加時のイベント
		/// </summary>
		/// <param name="replyToken">リプライトークン</param>
		/// <returns></returns>
		public async Task CallJoinEvent( string replyToken ) {

			RequestOfReplyMessage requestObject = new RequestOfReplyMessage();
			requestObject.replyToken = replyToken;
			RequestOfReplyMessage.Message message = new RequestOfReplyMessage.Message();
			message.type = "text";
			message.text = "グループ追加ありがとう！\n仲良くしてくださいね！";
			requestObject.messages = new RequestOfReplyMessage.Message[ 1 ];
			requestObject.messages[ 0 ] = message;

			string jsonRequest = JsonConvert.SerializeObject( requestObject );
			StringContent content = new StringContent( jsonRequest );
			content.Headers.ContentType = new MediaTypeHeaderValue( "application/json" );

			HttpClient client = new HttpClient();
			client.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );
			client.DefaultRequestHeaders.Add( "Authorization" , "Bearer {" + LineBotConfig.ChannelAccessToken + "}" );

			HttpResponseMessage response = await client.PostAsync( LineBotConfig.ReplyMessageUrl , content ).ConfigureAwait( false );
			string result = await response.Content.ReadAsStringAsync().ConfigureAwait( false );

		}

		/// <summary>
		/// テキストメッセージ通知時のイベント
		/// </summary>
		/// <param name="replyToken">リプライトークン</param>
		/// <param name="sentMessage">メッセージ本文</param>
		/// <returns></returns>
		public async Task CallTextMessageEvent( string replyToken , string sentMessage ) {

			RequestOfReplyMessage requestObject = new RequestOfReplyMessage();
			requestObject.replyToken = replyToken;
			RequestOfReplyMessage.Message message = new RequestOfReplyMessage.Message();
			message.type = "text";
			message.text = "メッセージ送られてきました！\n" + sentMessage;
			requestObject.messages = new RequestOfReplyMessage.Message[ 1 ];
			requestObject.messages[ 0 ] = message;

			string jsonRequest = JsonConvert.SerializeObject( requestObject );
			StringContent content = new StringContent( jsonRequest );
			content.Headers.ContentType = new MediaTypeHeaderValue( "application/json" );

			HttpClient client = new HttpClient();
			client.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );
			client.DefaultRequestHeaders.Add( "Authorization" , "Bearer {" + LineBotConfig.ChannelAccessToken + "}" );

			HttpResponseMessage response = await client.PostAsync( LineBotConfig.ReplyMessageUrl , content ).ConfigureAwait( false );
			string result = await response.Content.ReadAsStringAsync().ConfigureAwait( false );

		}

		/// <summary>
		/// 画像メッセージ通知時のイベント
		/// </summary>
		/// <param name="replyToken">リプライトークン</param>
		/// <param name="messageId">メッセージID</param>
		/// <returns></returns>
		public async Task CallImageMessageEvent( string replyToken , string messageId ) {

			#region 画像のバイナリデータをEmotion APIより解析
			string emotionResult = "";
			{
				Stream binaryImage = await this.GetContent( messageId );
				emotionResult = await new EmotionService().Call( binaryImage );
			}
			#endregion
			
			#region 解析結果を通知する
			{

				RequestOfReplyMessage requestObject = new RequestOfReplyMessage();
				requestObject.replyToken = replyToken;
				RequestOfReplyMessage.Message message = new RequestOfReplyMessage.Message();
				message.type = "text";
				message.text = "画像が送られてきました\n" + emotionResult;
				requestObject.messages = new RequestOfReplyMessage.Message[ 1 ];
				requestObject.messages[ 0 ] = message;

				string jsonRequest = JsonConvert.SerializeObject( requestObject );
				StringContent content = new StringContent( jsonRequest );
				content.Headers.ContentType = new MediaTypeHeaderValue( "application/json" );

				HttpClient client = new HttpClient();
				client.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );
				client.DefaultRequestHeaders.Add( "Authorization" , "Bearer {" + LineBotConfig.ChannelAccessToken + "}" );

				HttpResponseMessage response = await client.PostAsync( LineBotConfig.ReplyMessageUrl , content ).ConfigureAwait( false );
				string result = await response.Content.ReadAsStringAsync().ConfigureAwait( false );

			}
			#endregion

		}

		/// <summary>
		/// Contentから画像、動画、音声にアクセスするAPIを呼び、バイナリデータを返す
		/// </summary>
		/// <param name="messageId">メッセージID</param>
		/// <returns>バイナリデータ</returns>
		private async Task<Stream> GetContent( string messageId ) {

			HttpClient client = new HttpClient();
			client.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );
			client.DefaultRequestHeaders.Add( "Authorization" , "Bearer {" + LineBotConfig.ChannelAccessToken + "}" );

			HttpResponseMessage response = await client.GetAsync( LineBotConfig.GetContentUrl( messageId ) ).ConfigureAwait( false );
			Stream result = await response.Content.ReadAsStreamAsync().ConfigureAwait( false );

			return result;

		}

	}

}