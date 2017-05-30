namespace LineBotCompanyTrip.Models.LineBot.ReplyMessage {

	/// <summary>
	/// ボタン押下時アクション
	/// </summary>
	public class Action {

		/// <summary>
		/// アクション種別
		/// </summary>
		public string type;

		/// <summary>
		/// アクションの表示名
		/// </summary>
		public string label;

		/// <summary>
		/// postback eventに渡されるデータ
		/// </summary>
		public string data;

		/// <summary>
		/// アクション実行時に送信されるテキスト
		/// </summary>
		public string text;

		/// <summary>
		/// URI
		/// </summary>
		public string uri;

	}

}