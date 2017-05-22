namespace LineBotCompanyTrip.Models.AzureCognitiveServices.EmotionAPI {

	/// <summary>
	/// Emotion APIに使用するレスポンスEntity
	/// Emotion APIはオブジェクトでなく、配列を返すので
	/// そのオブジェクト一つ分のデータ群
	/// </summary>
	public class ResponseOfEmotionAPI {

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

		/// <summary>
		/// 表情情報
		/// </summary>
		public class Scores {

			/// <summary>
			/// 怒り
			/// </summary>
			public double anger;

			/// <summary>
			/// 軽蔑
			/// </summary>
			public double contempt;

			/// <summary>
			/// うんざり
			/// </summary>
			public double disgust;

			/// <summary>
			/// 恐れ
			/// </summary>
			public double fear;

			/// <summary>
			/// 幸せ
			/// </summary>
			public double happiness;

			/// <summary>
			/// 無表情
			/// </summary>
			public double neutral;

			/// <summary>
			/// 悲しみ
			/// </summary>
			public double sadness;

			/// <summary>
			/// 驚き
			/// </summary>
			public double surprise;

		}

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