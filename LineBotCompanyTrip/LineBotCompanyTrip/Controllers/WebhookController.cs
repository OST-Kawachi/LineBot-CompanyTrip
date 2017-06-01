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
using System;

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

			Trace.TraceInformation( "Webhook API Start" );

			Trace.TraceInformation( "Request Token is : " + requestToken.ToString() );

			//イベントの取得
			Event firstEvent;
			{
				RequestOfWebhook request = requestToken.ToObject<RequestOfWebhook>();
				if( request?.events?[0] == null ) {
					Trace.TraceError( "Request.Events[0] is Nothing" );
					return new HttpResponseMessage( HttpStatusCode.OK );
				}
				firstEvent = request.events[ 0 ];
			}
			
			this.sqlServiceServer = new SQLServerService();

			bool isUserId = firstEvent.source.type.Equals( "user" );
			string userIdOrGroupId = isUserId ? firstEvent.source.userId : firstEvent.source.groupId;
			Trace.TraceInformation( "Is User Id is : " + isUserId );
			Trace.TraceInformation( "User Id Or Group Id is : " + userIdOrGroupId );

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
						Trace.TraceError( "Request.Events[0].Message is Nothing" );
						return new HttpResponseMessage( HttpStatusCode.OK );
					}
					message = firstEvent.message;
				}

				//「集計して」のテキストメッセージ送信時イベント
				if( CommonEnum.MessageType.text.ToString().Equals( message.type ) && Regex.IsMatch( message.text , @"(.)*集計して(.)*" ) )
					return await this.RankingMessageEvent( 
						firstEvent.replyToken , 
						isUserId ,
						userIdOrGroupId
					);
				
				//画像メッセージ送信時イベント
				else if( CommonEnum.MessageType.image.ToString().Equals( message.type ) )
					return await this.ImageMessageEvent( 
						firstEvent.replyToken ,
						isUserId ,
						userIdOrGroupId ,
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
						isUserId ,
						userIdOrGroupId
					);

				//笑顔ランキング
				else if( CommonEnum.PostbackEvent.happiness.ToString().Equals( firstEvent.postback.data ) )
					return await this.HappinessRankingEvent(
						firstEvent.replyToken ,
						isUserId ,
						userIdOrGroupId
					);

				//表情豊かランキング
				else if( CommonEnum.PostbackEvent.emotion.ToString().Equals( firstEvent.postback.data ) )
					return await this.EmotionRankingEvent(
						firstEvent.replyToken ,
						isUserId ,
						userIdOrGroupId
					);
				
			}

			Trace.TraceInformation( "指定外のイベント" );
			
			return new HttpResponseMessage( HttpStatusCode.OK );
			
		}

		/// <summary>
		/// フォロー時イベント
		/// </summary>
		/// <param name="replyToken">リプライトークン</param>
		/// <param name="userId">ユーザID</param>
		/// <returns>ステータス200</returns>
		private async Task<HttpResponseMessage> FollowEvent( string replyToken , string userId ) {

			Trace.TraceInformation( "Follow Event Start" );
			
			this.sqlServiceServer.RegistLine( userId , true );

			//メッセージの通知
			{
				ReplyMessageService replyMessageService = new ReplyMessageService( replyToken );
				await replyMessageService
					.AddTextMessage( "友達追加ありがとうございます！\n仲良くしてくださいね！" )
					.Send();
			}

			Trace.TraceInformation( "Follow Event End" );

			return new HttpResponseMessage( HttpStatusCode.OK );

		}

		/// <summary>
		/// グループ追加時イベント
		/// </summary>
		/// <param name="replyToken">リプライトークン</param>
		/// <param name="groupId">グループID</param>
		/// <returns>ステータス200</returns>
		private async Task<HttpResponseMessage> JoinEvent( string replyToken , string groupId ) {

			Trace.TraceInformation( "Join Event Start" );
			
			this.sqlServiceServer.RegistLine( groupId , false );

			//メッセージの通知
			{
				ReplyMessageService replyMessageService = new ReplyMessageService( replyToken );
				await replyMessageService
					.AddTextMessage( "グループ追加ありがとうございます！\n仲良くしてくださいね！" )
					.Send();
			}

			Trace.TraceInformation( "Join Event End" );

			return new HttpResponseMessage( HttpStatusCode.OK );

		}

		/// <summary>
		/// アンフォロー時イベント
		/// </summary>
		/// <param name="userId">ユーザID</param>
		/// <returns>ステータス200</returns>
		private HttpResponseMessage UnfollowEvent( string userId ) {
			
			Trace.TraceInformation( "Unfollow Event Start" );
			
			this.sqlServiceServer.LeaveLine( userId , true );

			Trace.TraceInformation( "Unfollow Event End" );

			return new HttpResponseMessage( HttpStatusCode.OK );

		}

		/// <summary>
		/// グループ退出時イベント
		/// </summary>
		/// <param name="groupId">グループID</param>
		/// <returns>ステータス200</returns>
		private HttpResponseMessage LeaveEvent( string groupId ) {

			Trace.TraceInformation( "Leave Event Start" );
			
			this.sqlServiceServer.LeaveLine( groupId , false );
			
			Trace.TraceInformation( "Leave Event End" );

			return new HttpResponseMessage( HttpStatusCode.OK );

		}

		/// <summary>
		/// 画像メッセージ送信時イベント
		/// </summary>
		/// <param name="replyToken">リプライトークン</param>
		/// <param name="isUserId">ユーザIDかどうか</param>
		/// <param name="id">ユーザIDまたはグループID</param>
		/// <param name="messageId">メッセージID</param>
		/// <param name="timestamp">Webhook受信日時</param>
		/// <returns>ステータス200</returns>
		private async Task<HttpResponseMessage> ImageMessageEvent( 
			string replyToken ,
			bool isUserId ,
			string id ,
			string messageId ,
			string timestamp ) {

			Trace.TraceInformation( "Image Message Event Start" );

			//Contentより画像のバイナリデータ取得
			byte[] imageBytes = null;
			{
				LineBotService lineBotService = new LineBotService();
				imageBytes = await lineBotService.GetContent( messageId );
			}
			
			//Face APIよりFaceIDの取得
			List<ResponseOfFaceDetectAPI> responseOfFaceDetectAPI = null;
			{
				FaceService faceService = new FaceService();
				responseOfFaceDetectAPI = await faceService.CallDetect( imageBytes );
			}
						
			//Emotion APIより解析結果取得
			List<ResponseOfEmotionRecognitionAPI> responseOfEmotionRecognitionAPI = null;
			{
				EmotionService emotionService = new EmotionService();
				responseOfEmotionRecognitionAPI = await emotionService.CallRecognition( imageBytes );
			}

			//画像を加工
			List<byte[]> processedImageBytesList = new List<byte[]>();
			{
				ProcessPictureService processPictureService = new ProcessPictureService();
				foreach( ResponseOfEmotionRecognitionAPI response in responseOfEmotionRecognitionAPI ) {
					byte[] processedImageBytes = new byte[ imageBytes.Length ];
					imageBytes.CopyTo( processedImageBytes , 0 );
					processedImageBytes = processPictureService.DrawAnalysisOnPicture( processedImageBytes , response );
					processedImageBytesList.Add( processedImageBytes );
				}
			}

			//画像をサーバに保存
			string originalUrl = null;
			List<string> processedUrls = new List<string>();
			{
				SavePictureInAzureStorageService savePictureInAzureStorageService = new SavePictureInAzureStorageService();
				originalUrl = savePictureInAzureStorageService.StorePicture( imageBytes , timestamp , true );
				for( int i = 0 ; i < processedImageBytesList.Count ; i++ ) {
					processedUrls.Add( savePictureInAzureStorageService.StorePicture( processedImageBytesList[ i ] , timestamp , false , i ) );
				}
			}


			// DBに画像情報を登録
			int pictureId = this.sqlServiceServer.RegistPicture( isUserId , id , originalUrl );
			for( int i = 0 ; i < responseOfEmotionRecognitionAPI.Count ; i++ ) {
				this.sqlServiceServer.RegistFace( pictureId , responseOfEmotionRecognitionAPI[ i ] , processedUrls[ i ] , responseOfFaceDetectAPI[i].faceId );
			}
			
			//解析結果の通知
			{

				if( responseOfEmotionRecognitionAPI == null || responseOfEmotionRecognitionAPI.Count == 0 ) {

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

						string[] resultText = GetAnalysis( responseOfEmotionRecognitionAPI[ i ] ).Split( '\n' );

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

						//5枚おきにカルーセルを変更する
						if( i == 5 * 1 - 1 || i == 5 * 2 - 1 || i == 5 * 3 - 1 || i == 5 * 4 - 1 || i == 5 * 5 - 1 ) {
							replyMessageService.AddCarouselMessage( "解析" , columnCreator.GetColumns() );
							columnCreator.CreateColumn();
						}

					}
					await replyMessageService.AddCarouselMessage( "解析" , columnCreator.GetColumns() ).Send();

				}
				
			}

			Trace.TraceInformation( "Image Message Event End" );

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
		private string GetAnalysis( ResponseOfEmotionRecognitionAPI resultOfEmotion ) {

			CommonEnum.EmotionType type = CommonEnum.EmotionType.neutral;
			double value = 0.0;
			CommonUtil.GetMostEmotion( resultOfEmotion , ref type , ref value );

			return GetAnalysis( type , value );
			
		}

		/// <summary>
		/// 「集計して」メッセージ送信時イベント
		/// </summary>
		/// <param name="replyToken">リプライトークン</param>
		/// <param name="isUserId">ユーザIDかどうか</param>
		/// <param name="id">ユーザIDまたはグループID</param>
		/// <returns>ステータス200</returns>
		private async Task<HttpResponseMessage> RankingMessageEvent( 
			string replyToken ,
			bool isUserId ,
			string id
		) {

			Trace.TraceInformation( "Ranking Message Event Start" );
			
			this.sqlServiceServer.UpdatePostback( isUserId , id , true );

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

			Trace.TraceInformation( "Ranking Message Event End" );

			return new HttpResponseMessage( HttpStatusCode.OK );

		}

		/// <summary>
		/// よく写真に撮られる人ランキングイベント
		/// </summary>
		/// <param name="replyToken">リプライトークン</param>
		/// <param name="isUserId">ユーザIDかどうか</param>
		/// <param name="id">ユーザIDまたはグループID</param>
		/// <returns>ステータス200</returns>
		private async Task<HttpResponseMessage> CountRankingEvent( 
			string replyToken ,
			bool isUserId ,
			string id 
		) {
			
			Trace.TraceInformation( "Count Ranking Event Start" );

			//PostbackStatusの確認と更新
			{

				if( this.sqlServiceServer.IsPostbackInitialization( isUserId , id ) ) {
					Trace.TraceError( "Postback Is Not Initialization" );
					Trace.TraceInformation( "Count Ranking Event End" );
					return new HttpResponseMessage( HttpStatusCode.OK );
				}
				this.sqlServiceServer.UpdatePostback( isUserId , id , false );

			}

			List<string> faceIds = this.sqlServiceServer.GetFaceIds( isUserId , id );

			//Face APIよりFaceIDのグループ分け
			ResponseOfFaceGroupAPI responseOfFaceGroupAPI = null;
			{
				FaceService faceService = new FaceService();
				responseOfFaceGroupAPI = await faceService.CallGroup( faceIds );
			}

			//グループ分けされたFaceIdを数の多いものから順に3つ取得
			int count1 = 0;
			int count2 = 0;
			int count3 = 0;
			string url1 = "";
			string url2 = "";
			string url3 = "";
			{

				string faceId1 = "";
				string faceId2 = "";
				string faceId3 = "";
				
				List<string[]> groups = new List<string[]>();
				foreach( string[] ids in responseOfFaceGroupAPI.groups ) {
					groups.Add( ids );
				}
				groups.Sort( ( x , y ) => y.Length - x.Length );

				count1 = groups[ 0 ].Length;
				faceId1 = groups[ 0 ][ 0 ];

				count2 = groups[ 1 ].Length;
				faceId2 = groups[ 1 ][ 0 ];

				count3 = groups[ 2 ].Length;
				faceId3 = groups[ 2 ][ 0 ];
				
				url1 = this.sqlServiceServer.GetFaceUrl( faceId1 );
				url2 = this.sqlServiceServer.GetFaceUrl( faceId2 );
				url3 = this.sqlServiceServer.GetFaceUrl( faceId3 );

			}
			
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

			Trace.TraceInformation( "Count Ranking Event End" );

			return new HttpResponseMessage( HttpStatusCode.OK );

		}

		/// <summary>
		/// 笑顔ランキング
		/// </summary>
		/// <param name="replyToken">リプライトークン</param>
		/// <param name="isUserId">ユーザIDかどうか</param>
		/// <param name="id">ユーザIDまたはグループID</param>
		/// <returns>ステータス200</returns>
		private async Task<HttpResponseMessage> HappinessRankingEvent(
			string replyToken ,
			bool isUserId ,
			string id
		) {

			Trace.TraceInformation( "Happiness Ranking Event Start" );

			//PostbackStatusの確認と更新
			{

				if( this.sqlServiceServer.IsPostbackInitialization( isUserId , id ) ) {
					Trace.TraceError( "Postback Is Not Initialization" );
					Trace.TraceInformation( "Happiness Ranking Event End" );
					return new HttpResponseMessage( HttpStatusCode.OK );
				}
				this.sqlServiceServer.UpdatePostback( isUserId , id , false );

			}
			
			string url1 = "";
			string url2 = "";
			string url3 = "";

			double value1 = 0.0;
			double value2 = 0.0;
			double value3 = 0.0;

			this.sqlServiceServer.GetMostHappiness( isUserId , id , ref url1 , ref value1 , ref url2 , ref value2 , ref url3 , ref value3 );

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

			Trace.TraceInformation( "Happiness Ranking Event End" );

			return new HttpResponseMessage( HttpStatusCode.OK );

		}

		/// <summary>
		/// 表情豊かランキング
		/// </summary>
		/// <param name="replyToken">リプライトークン</param>
		/// <param name="isUserId">ユーザIDかどうか</param>
		/// <param name="id">ユーザIDまたはグループID</param>
		/// <returns>ステータス200</returns>
		private async Task<HttpResponseMessage> EmotionRankingEvent(
			string replyToken ,
			bool isUserId ,
			string id
		) {

			Trace.TraceInformation( "Emotion Ranking Event Start" );

			//PostbackStatusの確認と更新
			{

				if( this.sqlServiceServer.IsPostbackInitialization( isUserId , id ) ) {
					Trace.TraceError( "Postback Is Not Initialization" );
					Trace.TraceInformation( "Emotion Ranking Event End" );
					return new HttpResponseMessage( HttpStatusCode.OK );
				}
				this.sqlServiceServer.UpdatePostback( isUserId , id , false );

			}

			string url1 = "";
			string url2 = "";
			string url3 = "";

			CommonEnum.EmotionType type1 = CommonEnum.EmotionType.neutral;
			CommonEnum.EmotionType type2 = CommonEnum.EmotionType.neutral;
			CommonEnum.EmotionType type3 = CommonEnum.EmotionType.neutral;

			double value1 = 0.0;
			double value2 = 0.0;
			double value3 = 0.0;

			this.sqlServiceServer.GetMostEmotion( isUserId , id , ref url1 , ref type1 , ref value1 , ref url2 , ref type2 , ref value2 , ref url3 , ref type3 , ref value3 );

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

			Trace.TraceInformation( "Emotion Ranking Event End" );
			
			return new HttpResponseMessage( HttpStatusCode.OK );

		}
		
	}

}

