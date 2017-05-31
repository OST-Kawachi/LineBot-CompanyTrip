namespace LineBotCompanyTrip.Configurations {

	/// <summary>
	/// LINE Botに関する設定ファイル
	/// </summary>
	public class LineBotConfig {

		/// <summary>
		/// チャンネルアクセストークン
		/// </summary>
		private static readonly string channelAccessToken = "";

		/// <summary>
		/// チャンネルアクセストークン
		/// </summary>
		public static string ChannelAccessToken => channelAccessToken;

		/// <summary>
		/// Reply Message API URL
		/// </summary>
		private static readonly string replyMessageUrl = "https://api.line.me/v2/bot/message/reply";

		/// <summary>
		/// Reply Message API URL
		/// </summary>
		public static string ReplyMessageUrl => replyMessageUrl;

		/// <summary>
		/// コンテンツを返す用のAPIのURLを作成する
		/// </summary>
		/// <param name="messageId">メッセージID</param>
		/// <returns></returns>
		public static string GetContentUrl( string messageId ) => "https://api.line.me/v2/bot/message/" + messageId + "/content";
		
	}

}