namespace LineBotCompanyTrip.Configurations {

	/// <summary>
	/// Azure Cognitive Services Emotion APIの設定クラス
	/// </summary>
	public class EmotionConfig {

		/// <summary>
		/// サブスクリプションキー
		/// </summary>
		private static readonly string ocpApimSubscriptionKey = "";

		/// <summary>
		/// サブスクリプションキー
		/// </summary>
		public static string OcpApimSubscriptionKey => ocpApimSubscriptionKey;

		/// <summary>
		/// Emotion APIのURL
		/// </summary>
		private static readonly string emotionApiUrl = "https://westus.api.cognitive.microsoft.com/emotion/v1.0/recognize";

		/// <summary>
		/// Emotion APIのURL
		/// </summary>
		public static string EmotionApiUrl => emotionApiUrl;

	}

}