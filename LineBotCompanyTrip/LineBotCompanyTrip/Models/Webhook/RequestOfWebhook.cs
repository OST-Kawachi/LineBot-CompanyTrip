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
				public string type { set; get; }

				/// <summary>
				/// ユーザID
				/// </summary>
				public string userId { set; get; }

				/// <summary>
				/// グループID
				/// </summary>
				public string groupId { set; get; }

			}
		
			/// <summary>
			/// メッセージ情報
			/// </summary>
			public class MessageObject {

				/// <summary>
				/// メッセージID
				/// </summary>
				public string id { set; get; }

				/// <summary>
				/// メッセージ種別
				/// </summary>
				public string type { set; get; }

				/// <summary>
				/// メッセージ本文
				/// </summary>
				public string text { set; get; }

				/// <summary>
				/// タイトル
				/// </summary>
				public string title { set; get; }

				/// <summary>
				/// 住所
				/// </summary>
				public string address { set; get; }

				/// <summary>
				/// 緯度
				/// </summary>
				public double latitude { set; get; }

				/// <summary>
				/// 経度
				/// </summary>
				public double longitude { set; get; }
				
			}

			/// <summary>
			/// ポストバック
			/// </summary>
			public class Postback {

				/// <summary>
				/// ポストバックデータ
				/// </summary>
				public string data { set; get; }

			}

			/// <summary>
			/// イベント種別
			/// </summary>
			public string type { set; get; }

			/// <summary>
			/// リプライトークン
			/// </summary>
			public string replyToken { set; get; }
			
			/// <summary>
			/// ポストバック
			/// </summary>
			public Postback postback { set; get; }
			
			/// <summary>
			/// イベント送信元を表すオブジェクト
			/// </summary>
			public Source source { set; get; }

			/// <summary>
			/// メッセージオブジェクト
			/// </summary>
			public MessageObject message { set; get; }

		}

		/// <summary>
		/// イベントリスト
		/// </summary>
		public List<Event> events { set; get; }
		
	}

}