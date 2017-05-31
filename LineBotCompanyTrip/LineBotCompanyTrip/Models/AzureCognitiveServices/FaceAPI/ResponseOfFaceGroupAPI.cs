namespace LineBotCompanyTrip.Models.AzureCognitiveServices.FaceAPI {

	/// <summary>
	/// Face API - GroupのResponseEntity
	/// </summary>
	public class ResponseOfFaceGroupAPI {

		/// <summary>
		/// グループ化された顔ID群
		/// </summary>
		public string[][] groups;

		/// <summary>
		/// グループ化できなかった顔ID群
		/// </summary>
		public string[] messyGroup;

	}

}