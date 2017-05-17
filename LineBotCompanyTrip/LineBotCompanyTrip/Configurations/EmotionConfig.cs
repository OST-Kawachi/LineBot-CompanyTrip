﻿namespace LineBotCompanyTrip.Configurations {

	/// <summary>
	/// Azure Cognitive Services Emotion APIの設定クラス
	/// </summary>
	public class EmotionConfig {

		/// <summary>
		/// サブスクリプションキー
		/// </summary>
		public static readonly string OcpApimSubscriptionKey = "";

		/// <summary>
		/// Emotion APIのURL
		/// </summary>
		public static readonly string EmotionApiUrl = "https://westus.api.cognitive.microsoft.com/emotion/v1.0/recognize";

	}

}