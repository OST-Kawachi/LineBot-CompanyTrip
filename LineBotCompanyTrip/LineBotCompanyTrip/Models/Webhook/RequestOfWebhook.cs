using System.Collections.Generic;

namespace LineBotCompanyTrip.Models.Webhook {

	/// <summary>
	/// Webhookに使用するリクエストEntity
	/// </summary>
	public class RequestOfWebhook {

		/// <summary>
		/// イベント情報
		/// </summary>
		public class Event {

			/// <summary>
			/// イベント送信元を表すオブジェクト
			/// </summary>
			public class Source {

				/// <summary>
				/// 種別
				/// </summary>
				public string type;

				/// <summary>
				/// ユーザID
				/// </summary>
				public string userId;

				/// <summary>
				/// グループID
				/// </summary>
				public string groupId;

			}
		
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

			/// <summary>
			/// ポストバック
			/// </summary>
			public class Postback {

				/// <summary>
				/// ポストバックデータ
				/// </summary>
				public string data;

			}

			/// <summary>
			/// イベント種別
			/// </summary>
			public string type;

			/// <summary>
			/// リプライトークン
			/// </summary>
			public string replyToken;

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

		/// <summary>
		/// イベントリスト
		/// </summary>
		public List<Event> events;

	}

}