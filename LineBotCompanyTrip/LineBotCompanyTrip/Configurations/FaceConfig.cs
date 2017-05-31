namespace LineBotCompanyTrip.Configurations {

	/// <summary>
	/// Azure Cognitive Services Face APIの設定クラス
	/// </summary>
	public class FaceConfig {

		/// <summary>
		/// サブスクリプションキー
		/// </summary>
		private static readonly string ocpApimSubscriptionKey = "";

		/// <summary>
		/// サブスクリプションキー
		/// </summary>
		public static string OcpApimSubscriptionKey => ocpApimSubscriptionKey;

		/// <summary>
		/// Face API DetectのURL
		/// </summary>
		private static readonly string faceDetectApiUrl = "https://westus.api.cognitive.microsoft.com/face/v1.0/detect/";

		/// <summary>
		/// Face API DetectのURL
		/// </summary>
		public static string FaceDetectApiUrl => faceDetectApiUrl;

		/// <summary>
		/// FaceIDグループ化APIのURL
		/// </summary>
		private static readonly string faceGroupApiUrl = "https://westus.api.cognitive.microsoft.com/face/v1.0/group";

		/// <summary>
		/// FaceIDグループ化APIのURL
		/// </summary>
		public static string FaceGroupApiUrl => faceGroupApiUrl;

	}

}