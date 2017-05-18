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

			Trace.TraceInformation( "Webhook API 開始" );
			Trace.TraceInformation( "Request Token is " + requestToken.ToString() );

			RequestOfWebhook.Event firstEvent;
			#region イベントの取得と入力チェック
			{
				RequestOfWebhook request = requestToken.ToObject<RequestOfWebhook>();
				if( request?.events?[0] == null ) {
					Trace.TraceInformation( "request.events[0]が取得できませんでした" );
					return new HttpResponseMessage( HttpStatusCode.OK );
				}
				firstEvent = request.events[ 0 ];
			}
			#endregion
			
			#region フォローイベント
			if( "follow".Equals( firstEvent.type ) ) {
				Trace.TraceInformation( "フォローイベント通知" );
				LineBotService lineBotService = new LineBotService();
				await lineBotService.CallFollowEvent( firstEvent.replyToken );
				return new HttpResponseMessage( HttpStatusCode.OK );
			}
			#endregion

			#region グループ追加イベント
			if( "join".Equals( firstEvent.type ) ) {
				Trace.TraceInformation( "グループ追加イベント通知" );
				LineBotService lineBotService = new LineBotService();
				await lineBotService.CallJoinEvent( firstEvent.replyToken );
				return new HttpResponseMessage( HttpStatusCode.OK );
			}
			#endregion

			#region メッセージイベント
			if( "message".Equals( firstEvent.type ) ) {

				RequestOfWebhook.Event.MessageObject message;
				#region メッセージオブジェクトの取得と入力チェック
				if( firstEvent.message == null ) {
					Trace.TraceInformation( "request.events[0].messageが取得できませんでした" );
					return new HttpResponseMessage( HttpStatusCode.OK );
				}
				message = firstEvent.message;
				#endregion

				#region テキストメッセージ
				if( "text".Equals( message.type ) ) {
					Trace.TraceInformation( "メッセージイベント通知－テキスト" );
					LineBotService lineBotService = new LineBotService();
					await lineBotService.CallTextMessageEvent( firstEvent.replyToken , message.text );
					return new HttpResponseMessage( HttpStatusCode.OK );
				}
				#endregion

				#region 画像メッセージ
				if( "image".Equals( message.type ) ) {
					Trace.TraceInformation( "メッセージイベント通知－画像" );
					LineBotService lineBotService = new LineBotService();
					await lineBotService.CallImageMessageEvent( firstEvent.replyToken , message.id );
					return new HttpResponseMessage( HttpStatusCode.OK );
				}
				#endregion

				#region 位置情報メッセージ
				if( "location".Equals( message.type ) ) {
					Trace.TraceInformation( "メッセージイベント通知－位置情報" );
					LineBotService lineBotService = new LineBotService();
					await lineBotService.CallLocationMessageEvent(
						firstEvent.replyToken ,
						message.title ,
						message.address ,
						message.latitude ,
						message.longitude
					);
				}

				#endregion

			}
			#endregion

			Trace.TraceInformation( "指定外のイベントが呼ばれました" );

			return new HttpResponseMessage( HttpStatusCode.OK );

		}

	}

}

