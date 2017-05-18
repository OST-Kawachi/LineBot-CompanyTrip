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
			/// イベント種別
			/// </summary>
			public string type { set; get; }

			/// <summary>
			/// リプライトークン
			/// </summary>
			public string replyToken { set; get; }

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