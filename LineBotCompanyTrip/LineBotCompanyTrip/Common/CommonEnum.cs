namespace LineBotCompanyTrip.Common {

	/// <summary>
	/// Enum定義クラス
	/// </summary>
	public class CommonEnum {

		/// <summary>
		/// テンプレートタイプ
		/// </summary>
		public enum TemplateType {
			buttons ,
			confirm ,
			carousel
		}

		/// <summary>
		/// アクションタイプ
		/// </summary>
		public enum ActionType {
			postback ,
			message ,
			uri
		}

		/// <summary>
		/// メッセージタイプ
		/// </summary>
		public enum MessageType {
			text ,
			image ,
			location ,
			template
		}

		/// <summary>
		/// イベントタイプ
		/// </summary>
		public enum EventType {
			follow ,
			join ,
			unfollow ,
			leave ,
			message ,
			postback
		}

		/// <summary>
		/// ポストバックのイベント
		/// </summary>
		public enum PostbackEvent {
			count ,
			happiness ,
			emotion
		}

		/// <summary>
		/// Emotion APIの表情の種類
		/// </summary>
		public enum EmotionType {
			anger ,
			contempt ,
			disgust ,
			fear ,
			happiness ,
			neutral ,
			sadness ,
			surprise
		}
		
	}

}