using LineBotCompanyTrip.Configurations;
using LineBotCompanyTrip.Models.AzureCognitiveServices.EmotionAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace LineBotCompanyTrip.Services.Emotion {

	/// <summary>
	/// Azure Cognitive Services Emotion APIに関するサービスクラス
	/// </summary>
	public class EmotionService {

		/// <summary>
		/// Emotion APIを呼び出す
		/// </summary>
		/// <param name="binaryImage">画像のバイナリデータ</param>
		/// <returns>APIレスポンス</returns>
		public async Task<List<ResponseOfEmotionAPI>> Call( byte[] binaryImage ) {
			
			StreamContent content = new StreamContent( new MemoryStream( binaryImage ) );
			content.Headers.ContentType = new MediaTypeHeaderValue( "application/octet-stream" );
			
			HttpClient client = new HttpClient();
			client.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/octet-stream" ) );
			client.DefaultRequestHeaders.Add( "Ocp-Apim-Subscription-Key" , EmotionConfig.OcpApimSubscriptionKey );

			try {

				HttpResponseMessage response = await client.PostAsync( EmotionConfig.EmotionApiUrl , content );
				string resultAsString = await response.Content.ReadAsStringAsync();
				Trace.TraceInformation( "Emotion API Result is : " + resultAsString );
				return JsonConvert.DeserializeObject<List<ResponseOfEmotionAPI>>( resultAsString );

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