using LineBotCompanyTrip.Models.Webhook;
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
		/// POSTメソッド
		/// </summary>
		/// <param name="requestToken">リクエストトークン</param>
		/// <returns>常にステータス200のみを返す</returns>
		public async Task<HttpResponseMessage> Post( JToken requestToken ) {

			RequestOfWebhook request = requestToken.ToObject<RequestOfWebhook>();

			Trace.TraceInformation( "Webhook API 開始" );
			Trace.TraceInformation( "Request Token is " + requestToken.ToString() );

			LineBotService lineBotService = new LineBotService();

			JToken firstEventToken = requestToken?[ "events" ]?[ 0 ];

			#region イベントの入力チェック
			if( request?.events?[ 0 ] == null ) {
				Trace.TraceInformation( "無効なイベント" );
				return new HttpResponseMessage( HttpStatusCode.OK );
			}
			#endregion

			#region フォローイベント
			if( "follow".Equals( request.events[ 0 ].type ) ) {
				Trace.TraceInformation( "フォローイベント通知" );
				await lineBotService.CallFollowEvent( request.events[ 0 ].replyToken );
				return new HttpResponseMessage( HttpStatusCode.OK );
			}
			#endregion

			#region グループ追加イベント
			if( "join".Equals( request.events[ 0 ].type ) ) {
				Trace.TraceInformation( "グループ追加イベント通知" );
				await lineBotService.CallJoinEvent( request.events[ 0 ].replyToken );
				return new HttpResponseMessage( HttpStatusCode.OK );
			}
			#endregion

			#region メッセージイベント
			if( "message".Equals( request.events[ 0 ].type ) ) {

				#region メッセージイベントの入力チェック
				if( request.events[ 0 ].message == null ) {
					Trace.TraceInformation( "無効なイベント" );
					return new HttpResponseMessage( HttpStatusCode.OK );
				}
				#endregion

				#region テキストメッセージ
				if( "text".Equals( request.events[ 0 ].message.type ) ) {
					Trace.TraceInformation( "メッセージイベント通知－テキスト" );
					await lineBotService.CallTextMessageEvent( request.events[ 0 ].replyToken , request.events[ 0 ].message.text );
					return new HttpResponseMessage( HttpStatusCode.OK );
				}
				#endregion

				#region 画像メッセージ
				if( "image".Equals( request.events[ 0 ].message.type ) ) {
					Trace.TraceInformation( "メッセージイベント通知－画像" );
					await lineBotService.CallImageMessageEvent( request.events[ 0 ].replyToken , request.events[ 0 ].message.id );
					return new HttpResponseMessage( HttpStatusCode.OK );
				}
				#endregion

			}
			#endregion

			Trace.TraceInformation( "指定イベントでない" );

			return new HttpResponseMessage( HttpStatusCode.OK );

		}

	}

}

