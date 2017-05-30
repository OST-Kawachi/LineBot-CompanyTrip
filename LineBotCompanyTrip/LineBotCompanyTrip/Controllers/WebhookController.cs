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
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using LineBotCompanyTrip.Services.SQLServer;

namespace LineBotCompanyTrip.Controllers {

	/// <summary>
	/// LINE BotからのWebhook送信先API
	/// </summary>
	public class WebhookController : ApiController {

		/// <summary>
		/// SQLServerに関するサービス
		/// </summary>
		private SQLServerService sqlServiceServer;
		
		/// <summary>
		/// POSTメソッド
		/// </summary>
		/// <param name="requestToken">リクエストトークン</param>
		/// <returns>常にステータス200のみを返す</returns>
		public async Task<HttpResponseMessage> Post( JToken requestToken ) {

			Trace.TraceInformation( "Webhook API 開始" );
			Trace.TraceInformation( "Request Token is " + requestToken.ToString() );

			//イベントの取得
			Event firstEvent;
			{
				RequestOfWebhook request = requestToken.ToObject<RequestOfWebhook>();
				if( request?.events?[0] == null ) {
					Trace.TraceInformation( "request.events[0]が取得できませんでした" );
					return new HttpResponseMessage( HttpStatusCode.OK );
				}
				firstEvent = request.events[ 0 ];
			}
			
			this.sqlServiceServer = new SQLServerService();

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
				MessageObject message;
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
						message.id ,
						firstEvent.timestamp
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

			// ユーザIDのDB登録
			this.sqlServiceServer.RegistLineInfo( userId , true );

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

			// グループIDのDB登録
			this.sqlServiceServer.RegistLineInfo( groupId , false );

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

			// ユーザIDからわかるDBレコード削除
			this.sqlServiceServer.LeaveLineInfo( userId , true );

			return new HttpResponseMessage( HttpStatusCode.OK );

		}

		/// <summary>
		/// グループ退出時イベント
		/// </summary>
		/// <param name="groupId">グループID</param>
		/// <returns>ステータス200</returns>
		private HttpResponseMessage LeaveEvent( string groupId ) {

			Trace.TraceInformation( "グループ退出イベント通知" );

			// グループIDからわかるDBレコード削除
			this.sqlServiceServer.LeaveLineInfo( groupId , false );

			return new HttpResponseMessage( HttpStatusCode.OK );

		}

		/// <summary>
		/// 画像メッセージ送信時イベント
		/// </summary>
		/// <param name="replyToken">リプライトークン</param>
		/// <param name="userId">ユーザID</param>
		/// <param name="groupId">グループID</param>
		/// <param name="messageId">メッセージID</param>
		/// <param name="timestamp">Webhook受信日時</param>
		/// <returns>ステータス200</returns>
		private async Task<HttpResponseMessage> ImageMessageEvent( 
			string replyToken ,
			string userId ,
			string groupId ,
			string messageId ,
			string timestamp ) {

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

			//画像を加工
			List<byte[]> processedImageBytesList = new List<byte[]>();
			{
				ProcessPictureService processPictureService = new ProcessPictureService();
				foreach( ResponseOfEmotionAPI response in responseOfEmotionAPI ) {
			byte[] processedImageBytes = new byte[ imageBytes.Length ];
			imageBytes.CopyTo( processedImageBytes , 0 );
					processedImageBytes = processPictureService.DrawAnalysis( processedImageBytes , response );
					processedImageBytesList.Add( processedImageBytes );
				}
			}

			//画像をサーバに保存
			string originalUrl = null;
			List<string> processedUrls = new List<string>();
			{
				SavePictureInAzureStorageService savePictureInAzureStorageService = new SavePictureInAzureStorageService();
				originalUrl = savePictureInAzureStorageService.SaveImage( imageBytes , timestamp , true );
				for( int i = 0 ; i < processedImageBytesList.Count ; i++ ) {
					processedUrls.Add( savePictureInAzureStorageService.SaveImage( processedImageBytesList[ i ] , timestamp , false , i ) );
				}
			}

			// TODO Face APIの顔群とEmotionAPIの解析群の紐づけ

			int pictureId = this.sqlServiceServer.RegistPicture( userId , groupId , originalUrl );

			// DBに画像情報を登録
			for( int i = 0 ; i < responseOfEmotionAPI.Count ; i++ ) {
				this.sqlServiceServer.RegistFace( pictureId , responseOfEmotionAPI[ i ] , processedUrls[ i ] );
			}


			//解析結果の通知
			{

				if( responseOfEmotionAPI == null || responseOfEmotionAPI.Count == 0 ) {

					ReplyMessageService replyMessageService = new ReplyMessageService( replyToken );
					await replyMessageService
					.AddTextMessage( "顔は検出できませんでした！\nいいお写真ですね！" )
					.Send();
					
				}
				else {
					ActionCreator actionCreator = new ActionCreator();
					ColumnCreator columnCreator = new ColumnCreator();
					ReplyMessageService replyMessageService = new ReplyMessageService( replyToken );
					columnCreator = columnCreator.CreateColumn();
					for( int i = 0 ; i < processedUrls.Count ; i++ ) {

						string[] resultText = GetAnalysis( responseOfEmotionAPI[ i ] ).Split( '\n' );

						columnCreator = columnCreator
							.AddColumn(
								processedUrls[ i ] ,
								resultText[ 0 ] ,
								resultText[ 1 ] ,
								actionCreator
									.CreateAction( "carousel" )
									.AddMessageAction( "いいね！" , "いいね" )
									.GetActions()
						);
					}
					await replyMessageService.AddCarouselMessage( "解析" , columnCreator.GetColumns() ).Send();

				}
				
			}

			return new HttpResponseMessage( HttpStatusCode.OK );

		}

		/// <summary>
		/// 解析結果を返す
		/// </summary>
		/// <param name="type">表情タイプ</param>
		/// <param name="value">表情値</param>
		/// <returns></returns>
		private string GetAnalysis( CommonEnum.EmotionType type , double value ) {

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
		/// 解析結果を返す
		/// </summary>
		/// <param name="resultOfEmotion">Emotion APIの結果</param>
		/// <returns>解析結果</returns>
		private string GetAnalysis( ResponseOfEmotionAPI resultOfEmotion ) {

			CommonEnum.EmotionType type = CommonEnum.EmotionType.neutral;
			double value = 0.0;
			CommonUtil.GetMostEmotion( resultOfEmotion , ref type , ref value );

			return GetAnalysis( type , value );
			
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

			// DBよりpostbackステータス更新
			this.sqlServiceServer.UpdatePostback( userId , groupId , true );

			//テンプレートメッセージ通知
			{
				ActionCreator actionCreator = new ActionCreator();
				ReplyMessageService replyMessageService = new ReplyMessageService( replyToken );
				await replyMessageService
				.AddTextMessage( "今まで送られてきた画像を集計するよ" )
				.AddButtonsMessage(
					"ランキング" ,
					"https://linebotcompanytrip.blob.core.windows.net/pictures/original_1495938362818.jpeg" ,
					"ランキング" ,
					"下のボタンを押すとそれぞれのランキングが表示されるよ" ,
					actionCreator
					.CreateAction( "buttons" )
					.AddPostbackAction(
						"よく写った人ランキング" ,
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

			// 初期化状態でないなら何もしない
			if( !this.sqlServiceServer.IsPostbackInitialization( userId , groupId ) ) {
				Trace.TraceInformation( "アクション初期化がされていない" );
				return new HttpResponseMessage( HttpStatusCode.OK );
			}

			// postbackステータス更新
			this.sqlServiceServer.UpdatePostback( userId , groupId , false );
			
			// DBより画像を3枚取得
			string url1 = "";
			string url2 = "";
			string url3 = "";

			// 撮られた回数
			int count1 = 0;
			int count2 = 0;
			int count3 = 0;

			this.sqlServiceServer.GetMostPhotographed( userId , groupId , ref url1 , ref count1 , ref url2 , ref count2 , ref url3 , ref count3 );

			//解析結果の通知
			{

				ColumnCreator columnCreator = new ColumnCreator();
				ActionCreator actionCreator = new ActionCreator();

				ReplyMessageService replyMessageService = new ReplyMessageService( replyToken );
				await replyMessageService
				.AddTextMessage( "よく写真に写ってた方は以下のお三方です！" )
				.AddCarouselMessage(
					"よく写る人ランキング" ,
					columnCreator
						.CreateColumn()
						.AddColumn( 
							url1 , 
							"1位" , 
							count1 + "回" , 
							actionCreator
								.CreateAction( CommonEnum.TemplateType.carousel.ToString() )
								.AddMessageAction( "すごい！" , "すごい！！" )
								.GetActions()
						)
						.AddColumn( 
							url2 , 
							"2位" , 
							count2 + "回" ,
							actionCreator
								.CreateAction( CommonEnum.TemplateType.carousel.ToString() )
								.AddMessageAction( "いいね！" , "いいね！！" )
								.GetActions()
						)
						.AddColumn( 
							url3 , 
							"3位" , 
							count3 + "回" ,
							actionCreator
								.CreateAction( CommonEnum.TemplateType.carousel.ToString() )
								.AddMessageAction( "やったね！" , "やったね！！" )
								.GetActions()
						)
						.GetColumns()
					)
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

			// 初期化状態でないなら何もしない
			if( !this.sqlServiceServer.IsPostbackInitialization( userId , groupId ) ) {
				Trace.TraceInformation( "アクション初期化がされていない" );
				return new HttpResponseMessage( HttpStatusCode.OK );
			}

			// postbackステータス更新
			this.sqlServiceServer.UpdatePostback( userId , groupId , false );

			// DBより画像を3枚取得
			string url1 = "";
			string url2 = "";
			string url3 = "";

			double value1 = 0.0;
			double value2 = 0.0;
			double value3 = 0.0;

			this.sqlServiceServer.GetMostHappiness( userId , groupId , ref url1 , ref value1 , ref url2 , ref value2 , ref url3 , ref value3 );

			//解析結果の通知
			{

				string result1 = CommonUtil.ConvertDecimalIntoPercentage( value1 );
				string result2 = CommonUtil.ConvertDecimalIntoPercentage( value2 );
				string result3 = CommonUtil.ConvertDecimalIntoPercentage( value3 );

				ColumnCreator columnCreator = new ColumnCreator();
				ActionCreator actionCreator = new ActionCreator();

				ReplyMessageService replyMessageService = new ReplyMessageService( replyToken );
				await replyMessageService
				.AddTextMessage( "笑顔の眩しい方は以下のお三方です！" )
				.AddCarouselMessage(
					"笑顔ランキング" ,
					columnCreator
						.CreateColumn()
						.AddColumn(
							url1 ,
							"1位" ,
							"笑顔度：" + result1 ,
							actionCreator
								.CreateAction( CommonEnum.TemplateType.carousel.ToString() )
								.AddMessageAction( "すごい！" , "すごい！！" )
								.GetActions()
						)
						.AddColumn(
							url2 ,
							"2位" ,
							"笑顔度：" + result2 ,
							actionCreator
								.CreateAction( CommonEnum.TemplateType.carousel.ToString() )
								.AddMessageAction( "いいね！" , "いいね！！" )
								.GetActions()
						)
						.AddColumn(
							url3 ,
							"3位" ,
							"笑顔度:" + result3 ,
							actionCreator
								.CreateAction( CommonEnum.TemplateType.carousel.ToString() )
								.AddMessageAction( "やったね！" , "やったね！！" )
								.GetActions()
						)
						.GetColumns()
				)
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

			// 初期化状態でないなら何もしない
			if( !this.sqlServiceServer.IsPostbackInitialization( userId , groupId ) ) {
				Trace.TraceInformation( "アクション初期化がされていない" );
				return new HttpResponseMessage( HttpStatusCode.OK );
			}

			// postbackステータス更新
			this.sqlServiceServer.UpdatePostback( userId , groupId , false );

			// DBより画像を3枚取得
			string url1 = "";
			string url2 = "";
			string url3 = "";

			CommonEnum.EmotionType type1 = CommonEnum.EmotionType.neutral;
			CommonEnum.EmotionType type2 = CommonEnum.EmotionType.neutral;
			CommonEnum.EmotionType type3 = CommonEnum.EmotionType.neutral;

			double value1 = 0.0;
			double value2 = 0.0;
			double value3 = 0.0;

			this.sqlServiceServer.GetMostEmotion( userId , groupId , ref url1 , ref type1 , ref value1 , ref url2 , ref type2 , ref value2 , ref url3 , ref type3 , ref value3 );

			//解析結果の通知
			{

				ColumnCreator columnCreator = new ColumnCreator();
				ActionCreator actionCreator = new ActionCreator();

				ReplyMessageService replyMessageService = new ReplyMessageService( replyToken );
				await replyMessageService
				.AddTextMessage( "笑顔に限らず、表情豊かな方は以下のお三方です！" )
				.AddCarouselMessage(
					"表情豊かランキング" ,
					columnCreator
						.CreateColumn()
						.AddColumn(
							url1 ,
							"1位" ,
							GetAnalysis( type1 , value1 ) ,
							actionCreator
								.CreateAction( CommonEnum.TemplateType.carousel.ToString() )
								.AddMessageAction( "すごい！" , "すごい！！" )
								.GetActions()
						)
						.AddColumn(
							url2 ,
							"2位" ,
							GetAnalysis( type2 , value2 ) ,
							actionCreator
								.CreateAction( CommonEnum.TemplateType.carousel.ToString() )
								.AddMessageAction( "いいね！" , "いいね！！" )
								.GetActions()
						)
						.AddColumn(
							url3 ,
							"3位" ,
							GetAnalysis( type3 , value3 ) ,
							actionCreator
								.CreateAction( CommonEnum.TemplateType.carousel.ToString() )
								.AddMessageAction( "やったね！" , "やったね！！" )
								.GetActions()
						)
						.GetColumns()
				).AddTextMessage( "Σd(ﾟдﾟ*)ｸﾞｯｼﾞｮﾌﾞ" )
				.Send();

			}

			return new HttpResponseMessage( HttpStatusCode.OK );

		}
		
	}

}

