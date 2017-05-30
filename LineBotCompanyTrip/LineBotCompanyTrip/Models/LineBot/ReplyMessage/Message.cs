namespace LineBotCompanyTrip.Models.LineBot.ReplyMessage {

	/// <summary>
	/// リプライメッセージ
	/// </summary>
	public class Message {

		/// <summary>
		/// メッセージ種別
		/// </summary>
		public string type;

		/// <summary>
		/// メッセージ本文
		/// </summary>
		public string text;

		/// <summary>
		/// 画像のURL
		/// </summary>
		public string originalContentUrl;

		/// <summary>
		/// プレビュー画像のURL
		/// </summary>
		public string previewImageUrl;

		/// <summary>
		/// タイトル
		/// </summary>
		public string title;

		/// <summary>
		/// 住所
		/// </summary>
		public string address;

		/// <summary>
		/// 緯度
		/// </summary>
		public double latitude;

		/// <summary>
		/// 経度
		/// </summary>
		public double longitude;

		/// <summary>
		/// 代替テキスト
		/// </summary>
		public string altText;

		/// <summary>
		/// テンプレートオブジェクト
		/// </summary>
		public Template template;

	}

}