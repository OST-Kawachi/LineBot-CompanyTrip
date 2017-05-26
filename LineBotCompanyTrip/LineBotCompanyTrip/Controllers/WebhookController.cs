using LineBotCompanyTrip.Common;
using LineBotCompanyTrip.Models.AzureCognitiveServices.EmotionAPI;
using LineBotCompanyTrip.Models.AzureCognitiveServices.FaceAPI;
using LineBotCompanyTrip.Models.Webhook;
using LineBotCompanyTrip.Services.Emotion;
using LineBotCompanyTrip.Services.Face;
using LineBotCompanyTrip.Services.LineBot;
using LineBotCompanyTrip.Services.ProcessPicture;
using LineBotCompanyTrip.Services.SavePicture;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
			
			//画像を加工
			byte[] processedImageBytes = new byte[ imageBytes.Length ];
			imageBytes.CopyTo( processedImageBytes , 0 );
			{
				foreach( ResponseOfEmotionAPI response in responseOfEmotionAPI ) {
					processedImageBytes = this.DrawAnalysis( processedImageBytes , response );
				}
			}

			//画像をサーバに保存
			string originalUrl = null;
			string processedUrl = null;
			{
				SavePictureInAzureStorageService savePictureInAzureStorageService = new SavePictureInAzureStorageService();
				originalUrl = savePictureInAzureStorageService.SaveImage( imageBytes );
				processedUrl = savePictureInAzureStorageService.SaveImage( processedImageBytes );
			}

			// TODO DBに画像情報を登録

			//解析結果の通知
			{

				string sendText = "";
				if( responseOfEmotionAPI == null || responseOfEmotionAPI.Count == 0 ) {
					sendText = "顔は検出できませんでした！\nいいお写真ですね！";
				}
				else {
					foreach( ResponseOfEmotionAPI resultOfEmotion in responseOfEmotionAPI ) {
						sendText += this.GetAnalysis( resultOfEmotion ) + "\n\n";
					}
				}

				ReplyMessageService replyMessageService = new ReplyMessageService( replyToken );
				await replyMessageService
				.AddTextMessage( "画像が送られてきました！" )
				.AddTextMessage( sendText )
				.AddImageMessage( processedUrl , processedUrl )
				.Send();

			}

			return new HttpResponseMessage( HttpStatusCode.OK );

		}

		/// <summary>
		/// 解析結果を画像のバイナリデータに描画する
		/// </summary>
		/// <param name="imageBytes">画像のバイナリデータ</param>
		/// <param name="response">Emotion APIで取得した結果</param>
		/// <returns>描画後画像のバイナリデータ</returns>
		private byte[] DrawAnalysis( byte[] imageBytes , ResponseOfEmotionAPI response ) {

			ProcessPictureService processPictureService = new ProcessPictureService();

			CommonEnum.EmotionType type = CommonEnum.EmotionType.neutral;
			double value = 0.0;
			this.GetMostEmotion( response , ref type , ref value );

			Pen penColor;
			Brush brushColor;
			string text;
			switch( type ) {
				case CommonEnum.EmotionType.happiness:
					penColor = Pens.Pink;
					brushColor = Brushes.Pink;
					text = "Happy:" + CommonUtil.ConvertDecimalIntoPercentage( value );
					break;
				case CommonEnum.EmotionType.anger:
					penColor = Pens.Red;
					brushColor = Brushes.Red;
					text = "Anger:" + CommonUtil.ConvertDecimalIntoPercentage( value );
					break;
				case CommonEnum.EmotionType.contempt:
					penColor = Pens.Orange;
					brushColor = Brushes.Orange;
					text = "Contempt:" + CommonUtil.ConvertDecimalIntoPercentage( value );
					break;
				case CommonEnum.EmotionType.sadness:
					penColor = Pens.Blue;
					brushColor = Brushes.Blue;
					text = "Sadness:" + CommonUtil.ConvertDecimalIntoPercentage( value );
					break;
				case CommonEnum.EmotionType.disgust:
					penColor = Pens.Aqua;
					brushColor = Brushes.Aqua;
					text = "Disgust:" + CommonUtil.ConvertDecimalIntoPercentage( value );
					break;
				case CommonEnum.EmotionType.fear:
					penColor = Pens.Yellow;
					brushColor = Brushes.Yellow;
					text = "Fear:" + CommonUtil.ConvertDecimalIntoPercentage( value );
					break;
				case CommonEnum.EmotionType.surprise:
					penColor = Pens.Green;
					brushColor = Brushes.Green;
					text = "Surprise:" + CommonUtil.ConvertDecimalIntoPercentage( value );
					break;
				default:
					penColor = Pens.Gray;
					brushColor = Brushes.Gray;
					text = "Neutral:" + CommonUtil.ConvertDecimalIntoPercentage( value );
					break;
			}
			
			imageBytes =  processPictureService.DrawFrame(
				imageBytes ,
				response.faceRectangle.left ,
				response.faceRectangle.top ,
				response.faceRectangle.width ,
				response.faceRectangle.height ,
				penColor
			);

			imageBytes = processPictureService.DrawMessage(
				imageBytes ,
				response.faceRectangle.left ,
				response.faceRectangle.top ,
				penColor ,
				brushColor ,
				text
			);

			return imageBytes;

		}

		/// <summary>
		/// レスポンスから最も近い解析結果を返す
		/// </summary>
		/// <param name="response">レスポンス</param>
		/// <param name="type">表情種別</param>
		/// <param name="value">表情評価</param>
		private void GetMostEmotion( ResponseOfEmotionAPI response , ref CommonEnum.EmotionType type , ref double value ) {

			//幸せ度40%以上で幸せ認定
			double happiness = Math.Truncate( response.scores.happiness * 10000 ) / 10000;
			if( happiness > 0.4 ) {
				type = CommonEnum.EmotionType.happiness;
				value = happiness;
				return;
			}

			//悲しみ度40%以上で幸せ認定
			double sadness = Math.Truncate( response.scores.sadness * 10000 ) / 10000;
			if( sadness > 0.4 ) {
				type = CommonEnum.EmotionType.sadness;
				value = sadness;
				return;
			}

			//ビビり度40%以上で幸せ認定
			double fear = Math.Truncate( response.scores.fear * 10000 ) / 10000;
			if( fear > 0.4 ) {
				type = CommonEnum.EmotionType.fear;
				value = fear;
				return;
			}

			//怒り度40%以上で幸せ認定
			double anger = Math.Truncate( response.scores.anger * 10000 ) / 10000;
			if( anger > 0.4 ) {
				type = CommonEnum.EmotionType.anger;
				value = anger;
				return;
			}

			//軽蔑度40%以上で幸せ認定
			double contempt = Math.Truncate( response.scores.contempt * 10000 ) / 10000;
			if( contempt > 0.4 ) {
				type = CommonEnum.EmotionType.contempt;
				value = contempt;
				return;
			}

			//うんざり度40%以上で幸せ認定
			double disgust = Math.Truncate( response.scores.disgust * 10000 ) / 10000;
			if( disgust > 0.4 ) {
				type = CommonEnum.EmotionType.disgust;
				value = disgust;
				return;
			}

			//驚き度40%以上で幸せ認定
			double surprise = Math.Truncate( response.scores.surprise * 10000 ) / 10000;
			if( surprise > 0.4 ) {
				type = CommonEnum.EmotionType.surprise;
				value = surprise;
				return;
			}

			//どれも40%超えてなかった場合は真顔認定
			double neutral = Math.Truncate( response.scores.neutral * 10000 ) / 10000;
			type = CommonEnum.EmotionType.neutral;
			value = neutral;

		}

		/// <summary>
		/// 解析結果を返す
		/// </summary>
		/// <param name="resultOfEmotion">Emotion APIの結果</param>
		/// <returns>解析結果</returns>
		private string GetAnalysis( ResponseOfEmotionAPI resultOfEmotion ) {

			CommonEnum.EmotionType type = CommonEnum.EmotionType.neutral;
			double value = 0.0;
			this.GetMostEmotion( resultOfEmotion , ref type , ref value );

			string text;
			switch( type ) {

				case CommonEnum.EmotionType.happiness:
					text = "喜び度:" + CommonUtil.ConvertDecimalIntoPercentage( value ) + "！！！\n"
					+ "(*´∇｀*)ﾆﾊﾟｰｯ";
					break;

				case CommonEnum.EmotionType.sadness:
					text = "悲しみ度:" + CommonUtil.ConvertDecimalIntoPercentage( value ) + "！！！\n"
					+ "｡ﾟ(ﾟ´Д｀ﾟ)゜｡ｳｧｧｧﾝ";
					break;

				case CommonEnum.EmotionType.fear:
					text = "ビビり度:" + CommonUtil.ConvertDecimalIntoPercentage( value ) + "！！！\n"
					+ "((( ；ﾟДﾟ)))ｶﾞｸｶﾞｸﾌﾞﾙﾌﾞﾙ";
					break;

				case CommonEnum.EmotionType.anger:
					text = "怒り度:" + CommonUtil.ConvertDecimalIntoPercentage( value ) + "！！！\n"
					+ "ﾝﾓｫｰ!! o(*≧д≦)o″))";
					break;

				case CommonEnum.EmotionType.contempt:
					text = "軽蔑度:" + CommonUtil.ConvertDecimalIntoPercentage( value ) + "！！！\n"
					+ "(￢_￢;)ﾄﾞﾝﾋﾞｷ";
					break;

				case CommonEnum.EmotionType.disgust:
					text = "うんざり度:" + CommonUtil.ConvertDecimalIntoPercentage( value ) + "！！！\n"
					+ "┐(´～｀)┌ ﾔﾚﾔﾚ";
					break;

				case CommonEnum.EmotionType.surprise:
					text = "驚き度:" + CommonUtil.ConvertDecimalIntoPercentage( value ) + "！！！\n"
					+ "工エエェ(ﾟ〇ﾟ ;)ェエエ工";
					break;

				default:
					text = "真顔度:" + CommonUtil.ConvertDecimalIntoPercentage( value ) + "！！！\n"
					+ "(´・_・｀)";
					break;
					
			}

			return text;
			
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
						"たくさん写真撮られた人ランキング" ,
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

