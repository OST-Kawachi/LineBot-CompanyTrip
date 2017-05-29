namespace LineBotCompanyTrip.Models.Webhook {

	/// <summary>
	/// イベント情報
	/// </summary>
	public class Event {

		/// <summary>
		/// イベント種別
		/// </summary>
		public string type;

		/// <summary>
		/// リプライトークン
		/// </summary>
		public string replyToken;

		/// <summary>
		/// Webhook受信日時
		/// </summary>
		public string timestamp;

		/// <summary>
		/// ポストバック
		/// </summary>
		public Postback postback;

		/// <summary>
		/// イベント送信元を表すオブジェクト
		/// </summary>
		public Source source;

		/// <summary>
		/// メッセージオブジェクト
		/// </summary>
		public MessageObject message;

	}

}