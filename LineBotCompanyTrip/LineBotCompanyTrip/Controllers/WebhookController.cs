﻿using LineBotCompanyTrip.Common;
using LineBotCompanyTrip.Models.AzureCognitiveServices.EmotionAPI;
using LineBotCompanyTrip.Models.AzureCognitiveServices.FaceAPI;
using LineBotCompanyTrip.Models.Webhook;
using LineBotCompanyTrip.Services.Emotion;
using LineBotCompanyTrip.Services.Face;
using LineBotCompanyTrip.Services.LineBot;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
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

			//イベントの取得
			RequestOfWebhook.Event firstEvent;
			{
				RequestOfWebhook request = requestToken.ToObject<RequestOfWebhook>();
				if( request?.events?[0] == null ) {
					Trace.TraceInformation( "request.events[0]が取得できませんでした" );
					return new HttpResponseMessage( HttpStatusCode.OK );
				}
				firstEvent = request.events[ 0 ];
			}
			
			//フォロー時イベント
			if( CommonEnum.EventType.follow.ToString().Equals( firstEvent.type ) )
				return await this.FollowEvent( firstEvent.replyToken , firstEvent.source.userId );

			//グループ追加時イベント
			else if( CommonEnum.EventType.join.ToString().Equals( firstEvent.type ) )
				return await this.JoinEvent( firstEvent.replyToken , firstEvent.source.groupId );

			//アンフォロー時イベント
			else if( CommonEnum.EventType.unfollow.ToString().Equals( firstEvent.type ) )
				return this.UnfollowEvent( firstEvent.source.userId );

			//グループ退出時イベント
			else if( CommonEnum.EventType.leave.ToString().Equals( firstEvent.type ) )
				return this.LeaveEvent( firstEvent.source.groupId );

			//メッセージ時イベント
			else if( CommonEnum.EventType.message.ToString().Equals( firstEvent.type ) ) {

				//メッセージオブジェクトの取得
				RequestOfWebhook.Event.MessageObject message;
				{
					if( firstEvent.message == null ) {
						Trace.TraceInformation( "request.events[0].messageが取得できませんでした" );
						return new HttpResponseMessage( HttpStatusCode.OK );
					}
					message = firstEvent.message;
				}

				//「集計して」のテキストメッセージ送信時イベント
				if( CommonEnum.MessageType.text.ToString().Equals( message.type ) && Regex.IsMatch( message.text , @"(.)*集計して(.)*" ) )
					return await this.RankingMessageEvent( 
						firstEvent.replyToken , 
						firstEvent.source.userId ,
						firstEvent.source.groupId
					);
				
				//画像メッセージ送信時イベント
				else if( CommonEnum.MessageType.image.ToString().Equals( message.type ) )
					return await this.ImageMessageEvent( 
						firstEvent.replyToken , 
						firstEvent.source.userId ,
						firstEvent.source.groupId ,
						message.id 
					);

			}

			//Postback時イベント
			else if( CommonEnum.EventType.postback.ToString().Equals( firstEvent.type ) ) {

				//たくさん写真撮られた人ランキング
				if( CommonEnum.PostbackEvent.count.ToString().Equals( firstEvent.postback.data ) )
					return await this.CountRankingEvent( 
						firstEvent.replyToken ,
						firstEvent.source.userId , 
						firstEvent.source.groupId
					);

				//笑顔ランキング
				else if( CommonEnum.PostbackEvent.happiness.ToString().Equals( firstEvent.postback.data ) )
					return await this.HappinessRankingEvent(
						firstEvent.replyToken ,
						firstEvent.source.userId ,
						firstEvent.source.groupId
					);

				//表情豊かランキング
				else if( CommonEnum.PostbackEvent.emotion.ToString().Equals( firstEvent.postback.data ) )
					return await this.EmotionRankingEvent(
						firstEvent.replyToken ,
						firstEvent.source.userId ,
						firstEvent.source.groupId
					);
				
			}

			Trace.TraceInformation( "指定外のイベントが呼ばれました" );
			return new HttpResponseMessage( HttpStatusCode.OK );
			
		}

		/// <summary>
		/// フォロー時イベント
		/// </summary>
		/// <param name="replyToken">リプライトークン</param>
		/// <param name="userId">ユーザID</param>
		/// <returns>ステータス200</returns>
		private async Task<HttpResponseMessage> FollowEvent( string replyToken , string userId ) {

			Trace.TraceInformation( "フォローイベント通知" );

			// TODO ユーザIDのDB登録

			//メッセージの通知
			{
				ReplyMessageService replyMessageService = new ReplyMessageService( replyToken );
				await replyMessageService
					.AddTextMessage( "友達追加ありがとうございます！\n仲良くしてくださいね！" )
					.Send();
			}

			return new HttpResponseMessage( HttpStatusCode.OK );

		}

		/// <summary>
		/// グループ追加時イベント
		/// </summary>
		/// <param name="replyToken">リプライトークン</param>
		/// <param name="groupId">グループID</param>
		/// <returns>ステータス200</returns>
		private async Task<HttpResponseMessage> JoinEvent( string replyToken , string groupId ) {

			Trace.TraceInformation( "グループ追加イベント通知" );

			// TODO グループIDのDB登録

			//メッセージの通知
			{
				ReplyMessageService replyMessageService = new ReplyMessageService( replyToken );
				await replyMessageService
					.AddTextMessage( "グループ追加ありがとうございます！\n仲良くしてくださいね！" )
					.Send();
			}

			return new HttpResponseMessage( HttpStatusCode.OK );

		}

		/// <summary>
		/// アンフォロー時イベント
		/// </summary>
		/// <param name="userId">ユーザID</param>
		/// <returns>ステータス200</returns>
		private HttpResponseMessage UnfollowEvent( string userId ) {

			Trace.TraceInformation( "アンフォローイベント通知" );

			// TODO ユーザIDからわかるDBレコード削除

			return new HttpResponseMessage( HttpStatusCode.OK );

		}

		/// <summary>
		/// グループ退出時イベント
		/// </summary>
		/// <param name="groupId">グループID</param>
		/// <returns>ステータス200</returns>
		private HttpResponseMessage LeaveEvent( string groupId ) {

			Trace.TraceInformation( "グループ退出イベント通知" );

			// TODO グループIDからわかるDBレコード削除

			return new HttpResponseMessage( HttpStatusCode.OK );

		}

		/// <summary>
		/// 画像メッセージ送信時イベント
		/// </summary>
		/// <param name="replyToken">リプライトークン</param>
		/// <param name="userId">ユーザID</param>
		/// <param name="groupId">グループID</param>
		/// <param name="messageId">メッセージID</param>
		/// <returns>ステータス200</returns>
		private async Task<HttpResponseMessage> ImageMessageEvent( 
			string replyToken ,
			string userId ,
			string groupId ,
			string messageId ) {

			Trace.TraceInformation( "メッセージイベント通知－画像" );

			//Contentより画像のバイナリデータ取得
			byte[] imageBytes = null;
			{
				LineBotService lineBotService = new LineBotService();
				imageBytes = await lineBotService.GetContent( messageId );
			}
			
			//Face APIよりFaceIDの取得
			List<ResponseOfFaceAPI> responseOfFaceAPI = null;
			{
				FaceService faceService = new FaceService();
				responseOfFaceAPI = await faceService.Call( imageBytes );
			}
						
			//Emotion APIより解析結果取得
			List<ResponseOfEmotionAPI> responseOfEmotionAPI = null;
			{
				EmotionService emotionService = new EmotionService();
				responseOfEmotionAPI = await emotionService.Call( imageBytes );
			}
			
			// TODO Face APIの顔群とEmotionAPIの解析群の紐づけ

			// TODO DBに画像情報を登録

			// TODO 画像をサーバに保存
			string url = "https://manuke.jp/wp-content/uploads/2016/05/chomado2.jpg";

			//解析結果の通知
			{

				string sendText = "";
				if( responseOfEmotionAPI == null || responseOfEmotionAPI.Count == 0 ) {
					sendText = "顔は検出できませんでした！\nいいお写真ですね！";
				}
				else {
					foreach( ResponseOfEmotionAPI resultOfEmotion in responseOfEmotionAPI ) {
					string text = "\n"
						+ "座標：( " + resultOfEmotion.faceRectangle.left + " , " + resultOfEmotion.faceRectangle.top + " )\n"
						+ "幸せ度：" + CommonUtil.ConvertDecimalIntoPercentage( resultOfEmotion.scores.happiness ) + "\n"
						+ "悲しみ度：" + CommonUtil.ConvertDecimalIntoPercentage( resultOfEmotion.scores.sadness ) + "\n"
						+ "ビビり度：" + CommonUtil.ConvertDecimalIntoPercentage( resultOfEmotion.scores.fear ) + "\n"
						+ "怒り度：" + CommonUtil.ConvertDecimalIntoPercentage( resultOfEmotion.scores.anger ) + "\n"
						+ "軽蔑度：" + CommonUtil.ConvertDecimalIntoPercentage( resultOfEmotion.scores.contempt ) + "\n"
						+ "うんざり度：" + CommonUtil.ConvertDecimalIntoPercentage( resultOfEmotion.scores.disgust ) + "\n"
						+ "真顔度：" + CommonUtil.ConvertDecimalIntoPercentage( resultOfEmotion.scores.neutral ) + "\n"
						+ "驚き度：" + CommonUtil.ConvertDecimalIntoPercentage( resultOfEmotion.scores.surprise ) + "\n";
					sendText += text;
					}
				}

				ReplyMessageService replyMessageService = new ReplyMessageService( replyToken );
				await replyMessageService
				.AddTextMessage( "画像が送られてきました！" )
				.AddTextMessage( sendText )
				.AddImageMessage( url , url )
				.Send();

			}

			return new HttpResponseMessage( HttpStatusCode.OK );

		}

		/// <summary>
		/// 「集計して」メッセージ送信時イベント
		/// </summary>
		/// <param name="replyToken">リプライトークン</param>
		/// <param name="userId">ユーザID</param>
		/// <param name="groupId">グループID</param>
		/// <returns>ステータス200</returns>
		private async Task<HttpResponseMessage> RankingMessageEvent( 
			string replyToken ,
			string userId ,
			string groupId
		) {

			Trace.TraceInformation( "メッセージイベント通知－集計" );

			// TODO DBよりpostbackステータス更新

			//テンプレートメッセージ通知
			{
				ReplyMessageService.ActionCreator actionCreator = new ReplyMessageService.ActionCreator();
				ReplyMessageService replyMessageService = new ReplyMessageService( replyToken );
				await replyMessageService
				.AddTextMessage( "今まで送られてきた画像を集計するよ" )
				.AddButtonsMessage(
					"ランキング" ,
					"https://manuke.jp/wp-content/uploads/2016/05/chomado2.jpg" ,
					"ランキング" ,
					"下のボタンを押すとそれぞれのランキングが表示されるよ" ,
					actionCreator
					.CreateAction( "buttons" )
					.AddPostbackAction(
						"たくさん写真撮られた人" ,
						CommonEnum.PostbackEvent.count.ToString() ,
						"誰がたくさん撮られたの？"
					)
					.AddPostbackAction(
						"笑顔ランキング" ,
						CommonEnum.PostbackEvent.happiness.ToString() ,
						"笑顔ランキング見せて！"
					)
					.AddPostbackAction(
						"表情豊かランキング" ,
						CommonEnum.PostbackEvent.emotion.ToString() ,
						"表情豊かなのは誰！？"
					)
					.GetActions()
				)
				.Send();
			}

			return new HttpResponseMessage( HttpStatusCode.OK );

		}

		/// <summary>
		/// よく写真に撮られる人ランキングイベント
		/// </summary>
		/// <param name="replyToken">リプライトークン</param>
		/// <param name="userId">ユーザID</param>
		/// <param name="groupId">グループID</param>
		/// <returns>ステータス200</returns>
		private async Task<HttpResponseMessage> CountRankingEvent( 
			string replyToken ,
			string userId ,
			string groupId 
		) {

			Trace.TraceInformation( "Postbackイベント通知－よく写真に撮られる人ランキング" );

			// TODO postbackステータス更新

			// TODO DBより画像を3枚取得
			string url1 = "https://manuke.jp/wp-content/uploads/2016/05/chomado2.jpg";
			string url2 = "https://manuke.jp/wp-content/uploads/2016/05/chomado2.jpg";
			string url3 = "https://manuke.jp/wp-content/uploads/2016/05/chomado2.jpg";

			//解析結果の通知
			{

				ReplyMessageService replyMessageService = new ReplyMessageService( replyToken );
				await replyMessageService
				.AddTextMessage( "よく写真に写ってた方は以下のお三方です！" )
				.AddImageMessage( url1 , url1 )
				.AddImageMessage( url2 , url2 )
				.AddImageMessage( url3 , url3 )
				.AddTextMessage( "たっくさん撮られましたね！" )
				.Send();

			}

			return new HttpResponseMessage( HttpStatusCode.OK );

		}

		/// <summary>
		/// 笑顔ランキング
		/// </summary>
		/// <param name="replyToken">リプライトークン</param>
		/// <param name="userId">ユーザID</param>
		/// <param name="groupId">グループID</param>
		/// <returns>ステータス200</returns>
		private async Task<HttpResponseMessage> HappinessRankingEvent(
			string replyToken ,
			string userId ,
			string groupId
		) {

			Trace.TraceInformation( "Postbackイベント通知－笑顔ランキング" );

			// TODO postbackステータス更新

			// TODO DBより画像を3枚取得
			string url1 = "https://manuke.jp/wp-content/uploads/2016/05/chomado2.jpg";
			string url2 = "https://manuke.jp/wp-content/uploads/2016/05/chomado2.jpg";
			string url3 = "https://manuke.jp/wp-content/uploads/2016/05/chomado2.jpg";

			//解析結果の通知
			{

				ReplyMessageService replyMessageService = new ReplyMessageService( replyToken );
				await replyMessageService
				.AddTextMessage( "笑顔の眩しい方は以下のお三方です！" )
				.AddImageMessage( url1 , url1 )
				.AddImageMessage( url2 , url2 )
				.AddImageMessage( url3 , url3 )
				.AddTextMessage( "(⌒-⌒)ﾆｺﾆｺ" )
				.Send();

			}

			return new HttpResponseMessage( HttpStatusCode.OK );

		}

		/// <summary>
		/// 表情豊かランキング
		/// </summary>
		/// <param name="replyToken">リプライトークン</param>
		/// <param name="userId">ユーザID</param>
		/// <param name="groupId">グループID</param>
		/// <returns>ステータス200</returns>
		private async Task<HttpResponseMessage> EmotionRankingEvent(
			string replyToken ,
			string userId ,
			string groupId
		) {

			Trace.TraceInformation( "Postbackイベント通知－表情豊かランキング" );

			// TODO postbackステータス更新

			// TODO DBより画像を3枚取得
			string url1 = "https://manuke.jp/wp-content/uploads/2016/05/chomado2.jpg";
			string url2 = "https://manuke.jp/wp-content/uploads/2016/05/chomado2.jpg";
			string url3 = "https://manuke.jp/wp-content/uploads/2016/05/chomado2.jpg";

			//解析結果の通知
			{

				ReplyMessageService replyMessageService = new ReplyMessageService( replyToken );
				await replyMessageService
				.AddTextMessage( "笑顔に限らず、表情豊かな方は以下のお三方です！" )
				.AddImageMessage( url1 , url1 )
				.AddImageMessage( url2 , url2 )
				.AddImageMessage( url3 , url3 )
				.AddTextMessage( "Σd(ﾟдﾟ*)ｸﾞｯｼﾞｮﾌﾞ" )
				.Send();

			}

			return new HttpResponseMessage( HttpStatusCode.OK );

		}
		
	}

}

