namespace LineBotCompanyTrip.Models.AzureCognitiveServices.EmotionAPI {

	/// <summary>
	/// 顔の座標データ
	/// </summary>
	public class FaceRectangle {

		/// <summary>
		/// 画像上からの位置
		/// </summary>
		public int top;

		/// <summary>
		/// 画像左からの位置
		/// </summary>
		public int left;

		/// <summary>
		/// 顔の高さ
		/// </summary>
		public int height;

		/// <summary>
		/// 顔の幅
		/// </summary>
		public int width;

	}

}