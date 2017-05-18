using LineBotCompanyTrip.Configurations;
using LineBotCompanyTrip.Models.LineBot.ReplyMessage;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace LineBotCompanyTrip.Services.LineBot {

	/// <summary>
	/// ReplyMessage用サービスクラス
	/// </summary>
	public class ReplyMessageService {
		
		/// <summary>
		/// リクエストEntity
		/// </summary>
		private RequestOfReplyMessage Request { set; get; }

		/// <summary>
		/// メッセージの要素数
		/// </summary>
		private int MessagesIndex { set; get; }

		/// <summary>
		/// コンストラクタ
		/// リプライトークンを保持する
		/// </summary>
		/// <param name="replyToken">リプライトークン</param>
		public ReplyMessageService( string replyToken ) {
			
			this.Request = new RequestOfReplyMessage();
			this.Request.replyToken = replyToken;
			this.Request.messages = new RequestOfReplyMessage.Message[ 1 ];
			this.MessagesIndex = 0;

		}

		/// <summary>
		/// テキストメッセージを送信リストに追加する
		/// 初回以外は配列を拡大させながら追加する
		/// 5通目以降は追加されない
		/// </summary>
		/// <param name="messageText">メッセージテキスト</param>
		/// <returns>自身のオブジェクト</returns>
		public ReplyMessageService AddTextMessage( string messageText ) {

			if( this.MessagesIndex == 5 ) {
				Trace.TraceInformation( "5通以上送信できません" );
				return this;
			}
			else if( this.MessagesIndex != 0 ) {
				Array.Resize<RequestOfReplyMessage.Message>( ref this.Request.messages , this.MessagesIndex + 1 );
			}

			RequestOfReplyMessage.Message message = new RequestOfReplyMessage.Message();
			message.type = "text";
			message.text = messageText;

			this.Request.messages[ this.MessagesIndex ] = message;
			this.MessagesIndex++;

			return this;

		}

		/// <summary>
		/// メッセージの送信
		/// </summary>
		/// <returns></returns>
		public async Task Send() {
			
			string jsonRequest = JsonConvert.SerializeObject( this.Request );
			Trace.TraceInformation( "Reply Message Request is : " + jsonRequest );
			StringContent content = new StringContent( jsonRequest );
			content.Headers.ContentType = new MediaTypeHeaderValue( "application/json" );

			HttpClient client = new HttpClient();
			client.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );
			client.DefaultRequestHeaders.Add( "Authorization" , "Bearer {" + LineBotConfig.ChannelAccessToken + "}" );

			HttpResponseMessage response = await client.PostAsync( LineBotConfig.ReplyMessageUrl , content ).ConfigureAwait( false );
			string result = await response.Content.ReadAsStringAsync().ConfigureAwait( false );
			Trace.TraceInformation( "Reply Message Status Code is : " + response.StatusCode );

		}
		
	}

}