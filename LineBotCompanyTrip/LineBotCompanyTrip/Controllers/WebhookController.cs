using LineBotCompanyTrip.Services.LineBot;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
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

			Trace.TraceInformation( "Webhook API 開始" );
			Trace.TraceInformation( "Request Token is " + requestToken.ToString() );
			
			LineBotService lineBotService = new LineBotService();


			JToken firstEventToken = requestToken?[ "events" ]?[0];

			#region イベントの入力チェック
			if( firstEventToken == null ) {
				Trace.TraceInformation( "無効なイベント" );
				return new HttpResponseMessage( HttpStatusCode.OK );
			}
			#endregion
			
			#region フォローイベント
			if( "follow".Equals( firstEventToken[ "type" ].ToString() ) ) {
				Trace.TraceInformation( "フォローイベント通知" );
				await lineBotService.CallFollowEvent( firstEventToken[ "replyToken" ].ToString() );
				return new HttpResponseMessage( HttpStatusCode.OK );
			}
			#endregion

			#region グループ追加イベント
			if( "join".Equals( firstEventToken[ "type" ].ToString() ) ) {
				Trace.TraceInformation( "グループ追加イベント通知" );
				await lineBotService.CallJoinEvent( firstEventToken[ "replyToken" ].ToString() );
				return new HttpResponseMessage( HttpStatusCode.OK );
			}
			#endregion

			#region メッセージイベント
			if( "message".Equals( firstEventToken[ "type" ].ToString() ) ) {

				#region メッセージイベントの入力チェック
				if( firstEventToken[ "message" ] == null ) {
					Trace.TraceInformation( "無効なイベント" );
					return new HttpResponseMessage( HttpStatusCode.OK );
				}
				#endregion

				#region テキストメッセージ
				if( "text".Equals( firstEventToken[ "message" ][ "type" ]?.ToString() ) ) {
					Trace.TraceInformation( "メッセージイベント通知－テキスト" );
					await lineBotService.CallTextMessageEvent( firstEventToken[ "replyToken" ].ToString() , firstEventToken[ "message" ][ "text" ].ToString() );
					return new HttpResponseMessage( HttpStatusCode.OK );
				}
				#endregion

				#region 画像メッセージ
				if( "image".Equals( firstEventToken[ "message" ][ "type" ]?.ToString() ) ) {
					Trace.TraceInformation( "メッセージイベント通知－画像" );
					await lineBotService.CallImageMessageEvent( firstEventToken[ "replyToken" ].ToString() , firstEventToken[ "message" ][ "id" ].ToString() );
					return new HttpResponseMessage( HttpStatusCode.OK );
				}
				#endregion

			}
			#endregion

			Trace.TraceInformation( "指定イベントでない" );

			//常にステータス200を返す
			return new HttpResponseMessage( HttpStatusCode.OK );
			
		}

	}

}

