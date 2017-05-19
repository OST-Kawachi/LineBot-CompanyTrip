using LineBotCompanyTrip.Configurations;
using LineBotCompanyTrip.Services.Emotion;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using LineBotCompanyTrip.Models.AzureCognitiveServices.EmotionAPI;
using System.Collections.Generic;
using System.Diagnostics;
using LineBotCompanyTrip.Common;

namespace LineBotCompanyTrip.Services.LineBot {

	/// <summary>
	/// LINE Botに関するサービス
	/// </summary>
	public class LineBotService {
		
		/// <summary>
		/// フォロー時のイベント
		/// </summary>
		/// <param name="replyToken">リプライトークン</param>
		/// <returns></returns>
		public async Task CallFollowEvent( string replyToken ) {

			ReplyMessageService replyMessageService = new ReplyMessageService( replyToken );
			await replyMessageService
				.AddTextMessage( "友達追加ありがとうございます！" )
				.AddTextMessage( "仲良くしてくださいね！" )
				.Send();
			
		}

		/// <summary>
		/// グループ追加時のイベント
		/// </summary>
		/// <param name="replyToken">リプライトークン</param>
		/// <returns></returns>
		public async Task CallJoinEvent( string replyToken ) {

			ReplyMessageService replyMessageService = new ReplyMessageService( replyToken );
			await replyMessageService
				.AddTextMessage( "グループ追加ありがとうございます！" )
				.AddTextMessage( "仲良くしてくださいね！" )
				.Send();

		}

		/// <summary>
		/// テキストメッセージ通知時のイベント
		/// </summary>
		/// <param name="replyToken">リプライトークン</param>
		/// <param name="sentMessage">メッセージ本文</param>
		/// <returns></returns>
		public async Task CallTextMessageEvent( string replyToken , string sentMessage ) {

			ReplyMessageService.ActionCreator actionCreator = new ReplyMessageService.ActionCreator();

			ReplyMessageService replyMessageService = new ReplyMessageService( replyToken );
			await replyMessageService
				.AddTextMessage( "メッセージ送られてきました！" )
				.AddTextMessage( sentMessage )
				.AddButtonsMessage(
					"代替テキスト" ,
					"https://manuke.jp/wp-content/uploads/2016/05/chomado2.jpg" ,
					"タイトル" ,
					"テキスト" ,
					actionCreator
						.CreateAction( "buttons" )
						.AddMessageAction( "POSTラベル" , "テキスト" )
						.AddMessageAction( "二つ目" , "テキスト" )
						.AddUriAction( "URIラベル" , "https://www.google.co.jp/" )
						.GetActions()
				)
				.AddConfirmMessage(
					"代替テキスト" ,
					"テキスト" ,
					actionCreator
						.CreateAction( "confirm" )
						.AddPostbackAction( "POSTラベル" , "データ" , "テキスト" )
						.AddUriAction( "URIラベル" , "https://www.google.co.jp/" )
						.GetActions() )
				.Send();

		}

		/// <summary>
		/// 画像メッセージ通知時のイベント
		/// </summary>
		/// <param name="replyToken">リプライトークン</param>
		/// <param name="messageId">メッセージID</param>
		/// <returns></returns>
		public async Task CallImageMessageEvent( string replyToken , string messageId ) {

			#region 画像のバイナリデータをEmotion APIより解析
			List<ResponseOfEmotionAPI> emotionResult;
			{
				Stream binaryImage = await this.GetContent( messageId );
				emotionResult = await new EmotionService().Call( binaryImage );
				Trace.TraceInformation( "Emotion Result is : " + emotionResult );
			}
			#endregion
			
			#region 解析結果を通知する
			{

				string sendText = "";
				foreach( ResponseOfEmotionAPI resultOfEmotion in emotionResult ) {
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

				ReplyMessageService replyMessageService = new ReplyMessageService( replyToken );
				await replyMessageService
					.AddTextMessage( "画像が送られてきました！" )
					.AddTextMessage( sendText )
					.AddImageMessage( "https://manuke.jp/wp-content/uploads/2016/05/chomado3.jpg" , "https://pbs.twimg.com/media/CvXQ3pyUIAEhWXz.jpg" )
					.Send();

			}
			#endregion
			
		}

		/// <summary>
		/// 位置情報メッセージ通知時のイベント
		/// </summary>
		/// <param name="replyToken">リプライトークン</param>
		/// <param name="title">タイトル</param>
		/// <param name="address">住所</param>
		/// <param name="latitude">緯度</param>
		/// <param name="longitude">経度</param>
		/// <returns></returns>
		public async Task CallLocationMessageEvent( string replyToken , string title , string address , double latitude , double longitude ) {

			string text = "" +
				"タイトル：" + title + "\n" +
				"住所：" + address + "\n" +
				"緯度：" + latitude + "\n" +
				"経度：" + longitude;

			ReplyMessageService replyMessageService = new ReplyMessageService( replyToken );
			await replyMessageService
				.AddTextMessage( "位置情報が送られてきました！" )
				.AddTextMessage( text )
				.AddLocationMessage( title , address , latitude , longitude )
				.Send();

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