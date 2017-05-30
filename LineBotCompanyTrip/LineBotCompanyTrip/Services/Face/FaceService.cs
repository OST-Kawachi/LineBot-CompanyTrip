using LineBotCompanyTrip.Configurations;
using LineBotCompanyTrip.Models.AzureCognitiveServices.FaceAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace LineBotCompanyTrip.Services.Face{

	/// <summary>
	/// Azure Cognitive Services Face APIに関するサービスクラス
	/// </summary>
	public class FaceService {

		/// <summary>
		/// Face API - Detectを呼び出す
		/// </summary>
		/// <param name="binaryImage">画像のバイナリデータ</param>
		/// <returns>APIレスポンス</returns>
		public async Task<List<ResponseOfFaceDetectAPI>> CallDetect( byte[] binaryImage ) {
			
			Trace.TraceInformation( "Call Face API - Detect Start" );

			MemoryStream binaryStream = new MemoryStream( binaryImage );
			StreamContent content = new StreamContent( binaryStream );
			content.Headers.ContentType = new MediaTypeHeaderValue( "application/octet-stream" );

			HttpClient client = new HttpClient();
			client.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/octet-stream" ) );
			client.DefaultRequestHeaders.Add( "Ocp-Apim-Subscription-Key" , FaceConfig.OcpApimSubscriptionKey );

			try {

				HttpResponseMessage response = await client.PostAsync( FaceConfig.FaceApiUrl , content );
				string resultAsString = await response.Content.ReadAsStringAsync();
				Trace.TraceInformation( "Face API - Detect Result is : " + resultAsString );
				binaryStream.Dispose();
				response.Dispose();
				content.Dispose();
				client.Dispose();
				Trace.TraceInformation( "Call Face API - Detect End" );
				return JsonConvert.DeserializeObject<List<ResponseOfFaceDetectAPI>>( resultAsString );

			}
			catch( ArgumentNullException e ) {
				Trace.TraceError( "Face API - Detect Argument Null Exception " + e.Message );
				binaryStream.Dispose();
				content.Dispose();
				client.Dispose();
				Trace.TraceInformation( "Call Face API - Detect End" );
				return null;
			}
			catch( HttpRequestException e ) {
				Trace.TraceError( "Face API - Detect Http Request Exception " + e.Message );
				binaryStream.Dispose();
				content.Dispose();
				client.Dispose();
				Trace.TraceInformation( "Call Face API - Detect End" );
				return null;
			}
			catch( Exception e ) {
				Trace.TraceError( "Face API - Detect 予期せぬ例外 " + e.Message );
				binaryStream.Dispose();
				content.Dispose();
				client.Dispose();
				Trace.TraceInformation( "Call Face API - Detect End" );
				return null;
			}

		}

		/// <summary>
		/// Face API-Groupを呼び出す
		/// </summary>
		/// <param name="faceIds">Face APIより取得した顔情報を一意に識別するID</param>
		/// <returns>APIレスポンス</returns>
		public async Task<ResponseOfFaceGroupAPI> CallGroup( List<string> faceIds ) {
			
			Trace.TraceInformation( "Call Face API - Group Start" );
			
			string jsonRequest = JsonConvert.SerializeObject( faceIds );
			Trace.TraceInformation( "Face API Group Request is : " + jsonRequest );
			StringContent content = new StringContent( jsonRequest );
			content.Headers.ContentType = new MediaTypeHeaderValue( "application/json" );

			HttpClient client = new HttpClient();
			client.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );
			client.DefaultRequestHeaders.Add( "Ocp-Apim-Subscription-Key" , FaceConfig.OcpApimSubscriptionKey );

			try {

				HttpResponseMessage response = await client.PostAsync( FaceConfig.FaceGroupApiUrl , content );
				string resultAsString = await response.Content.ReadAsStringAsync();
				Trace.TraceInformation( "Face API Group Result is : " + resultAsString );
				response.Dispose();
				content.Dispose();
				client.Dispose();
				Trace.TraceInformation( "Call Face API - Group End" );
				return JsonConvert.DeserializeObject<ResponseOfFaceGroupAPI>( resultAsString );

			}
			catch( ArgumentNullException e ) {
				Trace.TraceError( "Face API - Group Argument Null Exception " + e.Message );
				content.Dispose();
				client.Dispose();
				Trace.TraceInformation( "Call Face API - Group End" );
				return null;
			}
			catch( HttpRequestException e ) {
				Trace.TraceError( "Face API - Group Http Request Exception " + e.Message );
				content.Dispose();
				client.Dispose();
				Trace.TraceInformation( "Call Face API - Group End" );
				return null;
			}
			catch( Exception e ) {
				Trace.TraceError( "Face API - Group 予期せぬ例外 " + e.Message );
				content.Dispose();
				client.Dispose();
				Trace.TraceInformation( "Call Face API - Group End" );
				return null;
			}

		}

	}

}