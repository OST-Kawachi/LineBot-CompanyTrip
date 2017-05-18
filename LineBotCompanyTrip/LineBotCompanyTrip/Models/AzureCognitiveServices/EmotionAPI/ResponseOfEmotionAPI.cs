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
			public int top { set; get; }
			
			/// <summary>
			/// 画像左からの位置
			/// </summary>
			public int left { set; get; }

			/// <summary>
			/// 顔の高さ
			/// </summary>
			public int height { set; get; }

			/// <summary>
			/// 顔の幅
			/// </summary>
			public int width { set; get; }

		}

		/// <summary>
		/// 表情情報
		/// </summary>
		public class Scores {

			/// <summary>
			/// 怒り
			/// </summary>
			public double anger { set; get; }

			/// <summary>
			/// 軽蔑
			/// </summary>
			public double contempt { set; get; }

			/// <summary>
			/// うんざり
			/// </summary>
			public double disgust { set; get; }
			
			/// <summary>
			/// 恐れ
			/// </summary>
			public double fear { set; get; }

			/// <summary>
			/// 幸せ
			/// </summary>
			public double happiness { set; get; }

			/// <summary>
			/// 無表情
			/// </summary>
			public double neutral { set; get; }

			/// <summary>
			/// 悲しみ
			/// </summary>
			public double sadness { set; get; }

			/// <summary>
			/// 驚き
			/// </summary>
			public double surprise { set; get; }
			
		}

		/// <summary>
		/// 顔座標のデータ
		/// </summary>
		public FaceRectangle faceRectangle { set; get; }

		/// <summary>
		/// 表情情報
		/// </summary>
		public Scores scores { set; get; }

	}

}