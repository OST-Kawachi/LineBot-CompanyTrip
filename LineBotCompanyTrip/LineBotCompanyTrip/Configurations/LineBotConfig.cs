namespace LineBotCompanyTrip.Configurations {

	/// <summary>
	/// LINE Botに関する設定ファイル
	/// </summary>
	public class LineBotConfig {

		/// <summary>
		/// チャンネルアクセストークン
		/// </summary>
		public static readonly string ChannelAccessToken = "";

		/// <summary>
		/// Reply Message API URL
		/// </summary>
		public static readonly string ReplyMessageUrl = "https://api.line.me/v2/bot/message/reply";

		/// <summary>
		/// コンテンツを返す用のAPIのURLを作成する
		/// </summary>
		/// <param name="messageId">メッセージID</param>
		/// <returns></returns>
		public static string GetContentUrl( string messageId ) {
			return "https://api.line.me/v2/bot/message/" + messageId + "/content";
		}
		
	}

}