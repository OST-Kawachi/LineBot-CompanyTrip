namespace LineBotCompanyTrip.Models.AzureCognitiveServices.FaceAPI {

	/// <summary>
	/// Face APIに使用するレスポンスEntity
	/// Face APIはオブジェクトでなく、配列を返すので
	/// そのオブジェクト一つ分のデータ群
	/// </summary>
	public class ResponseOfFaceAPI {

		/// <summary>
		/// 顔ID
		/// </summary>
		public string faceId;

	}

}