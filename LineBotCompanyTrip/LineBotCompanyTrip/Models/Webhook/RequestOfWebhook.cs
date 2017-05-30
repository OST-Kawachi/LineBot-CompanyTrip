using System.Collections.Generic;

namespace LineBotCompanyTrip.Models.Webhook {

	/// <summary>
	/// Webhookに使用するリクエストEntity
	/// </summary>
	public class RequestOfWebhook {

		/// <summary>
		/// イベントリスト
		/// </summary>
		public List<Event> events;

	}

}