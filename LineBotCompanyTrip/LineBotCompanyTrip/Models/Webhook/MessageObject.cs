namespace LineBotCompanyTrip.Models.Webhook {

	/// <summary>
	/// メッセージ情報
	/// </summary>
	public class MessageObject {

		/// <summary>
		/// メッセージID
		/// </summary>
		public string id;

		/// <summary>
		/// メッセージ種別
		/// </summary>
		public string type;

		/// <summary>
		/// メッセージ本文
		/// </summary>
		public string text;

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

	}

}