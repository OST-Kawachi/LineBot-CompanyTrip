namespace LineBotCompanyTrip.Models.AzureCognitiveServices.FaceAPI {

	/// <summary>
	/// Face API - Detectに使用するレスポンスEntity
	/// Face API - Detectはオブジェクトでなく、配列を返すので
	/// そのオブジェクト一つ分のデータ群
	/// </summary>
	public class ResponseOfFaceDetectAPI {

		/// <summary>
		/// 顔ID
		/// </summary>
		public string faceId;

	}

}