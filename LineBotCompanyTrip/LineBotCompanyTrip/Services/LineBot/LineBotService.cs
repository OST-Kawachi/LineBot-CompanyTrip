﻿using LineBotCompanyTrip.Configurations;
using LineBotCompanyTrip.Services.Emotion;
using LineBotCompanyTrip.Models.LineBot.ReplyMessage;
using Newtonsoft.Json;
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
			List<ResponseOfEmotionAPI> emotionResult;
			{
				Stream binaryImage = await this.GetContent( messageId );
				emotionResult = await new EmotionService().Call( binaryImage );
				Trace.TraceInformation( "Emotion Result is : " + emotionResult );
			}
			#endregion
			
			#region 解析結果を通知する
			{

				RequestOfReplyMessage requestObject = new RequestOfReplyMessage();
				requestObject.replyToken = replyToken;
				RequestOfReplyMessage.Message message = new RequestOfReplyMessage.Message();
				message.type = "text";

				#region テキストの作成
				message.text = "画像が送られてきました";
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
					message.text += text;
				}
				#endregion
				
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
		/// 位置情報メッセージ通知時のイベント
		/// </summary>
		/// <param name="replyToken">リプライトークン</param>
		/// <param name="title">タイトル</param>
		/// <param name="address">住所</param>
		/// <param name="latitude">緯度</param>
		/// <param name="longitude">経度</param>
		/// <returns></returns>
		public async Task CallLocationMessageEvent( string replyToken , string title , string address , double latitude , double longitude ) {

			#region 通知する
			{

				RequestOfReplyMessage requestObject = new RequestOfReplyMessage();
				requestObject.replyToken = replyToken;
				RequestOfReplyMessage.Message message = new RequestOfReplyMessage.Message();
				message.type = "text";
				message.text = "位置情報が送られてきました\n" +
					"タイトル：" + title + "\n" +
					"住所：" + address + "\n" +
					"緯度：" + latitude + "\n" +
					"経度：" + longitude;
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