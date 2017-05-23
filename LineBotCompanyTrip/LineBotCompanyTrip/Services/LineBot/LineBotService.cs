﻿using LineBotCompanyTrip.Configurations;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace LineBotCompanyTrip.Services.LineBot {

	/// <summary>
	/// LINE Botに関するサービス
	/// </summary>
	public class LineBotService {
		
		/// <summary>
		/// Contentから画像、動画、音声にアクセスするAPIを呼び、バイナリデータを返す
		/// </summary>
		/// <param name="messageId">メッセージID</param>
		/// <returns>バイナリデータ</returns>
		public async Task<Stream> GetContent( string messageId ) {

			HttpClient client = new HttpClient();
			client.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );
			client.DefaultRequestHeaders.Add( "Authorization" , "Bearer {" + LineBotConfig.ChannelAccessToken + "}" );

			try {
				HttpResponseMessage response = await client.GetAsync( LineBotConfig.GetContentUrl( messageId ) );
				Stream result = await response.Content.ReadAsStreamAsync();
				Trace.TraceInformation( "Get Binary Image is : " + result != null ? "SUCCESS" : "FAILED" );
				return result;
			}
			catch( ArgumentNullException e ) {
				Trace.TraceInformation( "Emotion API Argument Null Exception " + e.Message );
				return null;
			}
			catch( HttpRequestException e ) {
				Trace.TraceInformation( "Emotion API Http Request Exception " + e.Message );
				return null;
			}
			catch( Exception e ) {
				Trace.TraceInformation( "予期せぬ例外 " + e.Message );
				return null;
			}

		}

	}

}