namespace LineBotCompanyTrip.Models.AzureCognitiveServices.EmotionAPI {

	/// <summary>
	/// Emotion APIに使用するレスポンスEntity
	/// Emotion APIはオブジェクトでなく、配列を返すので
	/// そのオブジェクト一つ分のデータ群
	/// </summary>
	public class ResponseOfEmotionAPI {

		/// <summary>
		/// 顔座標のデータ
		/// </summary>
		public FaceRectangle faceRectangle;

		/// <summary>
		/// 表情情報
		/// </summary>
		public Scores scores;

	}

}