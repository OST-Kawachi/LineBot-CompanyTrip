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
		/// Emotion API - Recognitionを呼び出す
		/// </summary>
		/// <param name="binaryImage">画像のバイナリデータ</param>
		/// <returns>APIレスポンス</returns>
		public async Task<List<ResponseOfEmotionRecognitionAPI>> CallRecognition( byte[] binaryImage ) {

			Trace.TraceInformation( "Call Emotion API - Recognition Start" );
			
			MemoryStream bynaryStream = new MemoryStream( binaryImage );
			StreamContent content = new StreamContent( bynaryStream );
			content.Headers.ContentType = new MediaTypeHeaderValue( "application/octet-stream" );
			
			HttpClient client = new HttpClient();
			client.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/octet-stream" ) );
			client.DefaultRequestHeaders.Add( "Ocp-Apim-Subscription-Key" , EmotionConfig.OcpApimSubscriptionKey );

			try {

				HttpResponseMessage response = await client.PostAsync( EmotionConfig.EmotionApiUrl , content );
				string resultAsString = await response.Content.ReadAsStringAsync();
				Trace.TraceInformation( "Emotion API - Recognition Result is : " + resultAsString );
				bynaryStream.Dispose();
				response.Dispose();
				content.Dispose();
				client.Dispose();
				Trace.TraceInformation( "Call Emotion API - Recognition End" );
				return JsonConvert.DeserializeObject<List<ResponseOfEmotionRecognitionAPI>>( resultAsString );

			}
			catch( ArgumentNullException e ) {
				Trace.TraceError( "Emotion API - Recognition Argument Null Exception " + e.Message );
				bynaryStream.Dispose();
				content.Dispose();
				client.Dispose();
				Trace.TraceInformation( "Call Emotion API - Recognition End" );
				return null;
			}
			catch( HttpRequestException e ) {
				Trace.TraceError( "Emotion API - Recognition Http Request Exception " + e.Message );
				bynaryStream.Dispose();
				content.Dispose();
				client.Dispose();
				Trace.TraceInformation( "Call Emotion API - Recognition End" );
				return null;
			}
			catch( Exception e ) {
				Trace.TraceError( "Emotion API - Recognition 予期せぬ例外 " + e.Message );
				bynaryStream.Dispose();
				content.Dispose();
				client.Dispose();
				Trace.TraceInformation( "Call Emotion API - Recognition End" );
				return null;
			}
			
		}

	}

}