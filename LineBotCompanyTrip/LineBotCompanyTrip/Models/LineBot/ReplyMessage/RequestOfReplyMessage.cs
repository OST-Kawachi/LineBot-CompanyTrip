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
			/// テンプレートオブジェクト
			/// </summary>
			public class Template {
				
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

				/// <summary>
				/// カラム
				/// </summary>
				public class Column {

					/// <summary>
					/// 画像のURL
					/// </summary>
					public string thumbnailImageUrl;

					/// <summary>
					/// タイトル
					/// </summary>
					public string title;

					/// <summary>
					/// テキスト
					/// </summary>
					public string text;

					/// <summary>
					/// アクション
					/// </summary>
					public Action[] actions;

				}

				/// <summary>
				/// テンプレート種別
				/// </summary>
				public string type;

				/// <summary>
				/// 画像のURL
				/// </summary>
				public string thumbnailImageUrl;

				/// <summary>
				/// タイトル
				/// </summary>
				public string title;

				/// <summary>
				/// 説明文
				/// </summary>
				public string text;

				/// <summary>
				/// ボタン押下時アクション
				/// </summary>
				public Action[] actions;

				/// <summary>
				/// カラム
				/// </summary>
				public Column[] columns;

			}

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