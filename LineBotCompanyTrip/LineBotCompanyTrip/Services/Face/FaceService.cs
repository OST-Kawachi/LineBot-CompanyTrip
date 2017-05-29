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
		/// Face APIを呼び出す
		/// </summary>
		/// <param name="binaryImage">画像のバイナリデータ</param>
		/// <returns>APIレスポンス</returns>
		public async Task<List<ResponseOfFaceAPI>> Call( byte[] binaryImage ) {

			MemoryStream binaryStream = new MemoryStream( binaryImage );
			StreamContent content = new StreamContent( binaryStream );
			content.Headers.ContentType = new MediaTypeHeaderValue( "application/octet-stream" );

			HttpClient client = new HttpClient();
			client.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/octet-stream" ) );
			client.DefaultRequestHeaders.Add( "Ocp-Apim-Subscription-Key" , FaceConfig.OcpApimSubscriptionKey );

			try {

				HttpResponseMessage response = await client.PostAsync( FaceConfig.FaceApiUrl , content );
				string resultAsString = await response.Content.ReadAsStringAsync();
				Trace.TraceInformation( "Face API Result is : " + resultAsString );
				binaryStream.Dispose();
				response.Dispose();
				content.Dispose();
				client.Dispose();
				return JsonConvert.DeserializeObject<List<ResponseOfFaceAPI>>( resultAsString );

			}
			catch( ArgumentNullException e ) {
				Trace.TraceInformation( "Emotion API Argument Null Exception " + e.Message );
				binaryStream.Dispose();
				content.Dispose();
				client.Dispose();
				return null;
			}
			catch( HttpRequestException e ) {
				Trace.TraceInformation( "Emotion API Http Request Exception " + e.Message );
				binaryStream.Dispose();
				content.Dispose();
				client.Dispose();
				return null;
			}
			catch( Exception e ) {
				Trace.TraceInformation( "予期せぬ例外 " + e.Message );
				binaryStream.Dispose();
				content.Dispose();
				client.Dispose();
				return null;
			}

		}

	}

}