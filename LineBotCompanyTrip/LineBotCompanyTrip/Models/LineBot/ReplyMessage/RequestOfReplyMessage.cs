namespace LineBotCompanyTrip.Models.LineBot.ReplyMessage {

	/// <summary>
	/// Reply Messageに使用するリクエストEntity
	/// </summary>
	public class RequestOfReplyMessage {

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

		}

		/// <summary>
		/// 返信に必要なリプライトークン
		/// </summary>
		public string replyToken;

		/// <summary>
		/// リプライメッセージ(最大5通)
		/// </summary>
		public Message[] messages;

	}

}