using LineBotCompanyTrip.Common;
using System;

namespace LineBotCompanyTrip.Services.LineBot {

	/// <summary>
	/// テンプレートに使用するアクション作成クラス
	/// </summary>
	public class ActionCreator {

		/// <summary>
		/// アクション
		/// </summary>
		private Models.LineBot.ReplyMessage.Action[] actions;

		/// <summary>
		/// アクション配列の長さ
		/// </summary>
		private int ActionsIndex { set; get; }

		/// <summary>
		/// アクション配列の最大値
		/// </summary>
		private int MaxIndex { set; get; }

		/// <summary>
		/// アクション配列を作成する
		/// </summary>
		/// <param name="templateType">テンプレート種別</param>
		/// <returns>自身のオブジェクト</returns>
		public ActionCreator CreateAction( string templateType ) {

			this.actions = new Models.LineBot.ReplyMessage.Action[ 1 ];

			if( CommonEnum.TemplateType.buttons.ToString().Equals( templateType ) ) {
				this.MaxIndex = 4;
			}
			else if( CommonEnum.TemplateType.confirm.ToString().Equals( templateType ) ) {
				this.MaxIndex = 2;
			}
			else if( CommonEnum.TemplateType.carousel.ToString().Equals( templateType ) ) {
				this.MaxIndex = 3;
			}

			this.ActionsIndex = 0;

			return this;

		}

		/// <summary>
		/// タップ時にdataで指定された文字列がpostback eventとしてWebhookで通知されるアクションを追加する
		/// 2つめ以降のアクションは配列を作成しながら追加する
		/// アクションアイテムの上限を超えた場合は何もしない
		/// </summary>
		/// <param name="label">アクション表示名</param>
		/// <param name="data">Webhookに送信される文字列データ</param>
		/// <param name="text">アクション実行時に送信されるテキスト</param>
		/// <returns>自身のオブジェクト</returns>
		public ActionCreator AddPostbackAction( string label , string data , string text ) {

			if( this.ActionsIndex == this.MaxIndex ) {
				return this;
			}
			else if( this.ActionsIndex != 0 ) {
				Array.Resize( ref this.actions , this.ActionsIndex + 1 );
			}

			Models.LineBot.ReplyMessage.Action action = new Models.LineBot.ReplyMessage.Action() {
				type = CommonEnum.ActionType.postback.ToString() ,
				label = label ,
				data = data ,
				text = text
			};

			this.actions[ this.ActionsIndex ] = action;
			this.ActionsIndex++;

			return this;

		}

		/// <summary>
		/// タップ時にtextで指定された文字列がユーザの発言として送信されるアクションを追加する
		/// 2つめ以降のアクションは配列を作成しながら追加する
		/// </summary>
		/// <param name="label">アクション表示名</param>
		/// <param name="text">アクション実行時に送信されるテキスト</param>
		/// <returns>自身のオブジェクト</returns>
		public ActionCreator AddMessageAction( string label , string text ) {

			if( this.ActionsIndex == this.MaxIndex ) {
				return this;
			}
			else if( this.ActionsIndex != 0 ) {
				Array.Resize( ref this.actions , this.ActionsIndex + 1 );
			}

			Models.LineBot.ReplyMessage.Action action = new Models.LineBot.ReplyMessage.Action() {
				type = CommonEnum.ActionType.message.ToString() ,
				label = label ,
				text = text
			};

			this.actions[ this.ActionsIndex ] = action;
			this.ActionsIndex++;

			return this;

		}

		/// <summary>
		/// タップ時にuriで指定されたURIを開くアクションを追加する
		/// 2つめ以降のアクションは配列を作成しながら追加する
		/// </summary>
		/// <param name="label">アクション表示名</param>
		/// <param name="uri">URI</param>
		/// <returns>自身のオブジェクト</returns>
		public ActionCreator AddUriAction( string label , string uri ) {

			if( this.ActionsIndex == this.MaxIndex ) {
				return this;
			}
			else if( this.ActionsIndex != 0 ) {
				Array.Resize( ref this.actions , this.ActionsIndex + 1 );
			}

			Models.LineBot.ReplyMessage.Action action = new Models.LineBot.ReplyMessage.Action() {
				type = CommonEnum.ActionType.uri.ToString() ,
				label = label ,
				uri = uri
			};

			this.actions[ this.ActionsIndex ] = action;
			this.ActionsIndex++;

			return this;

		}

		/// <summary>
		/// アクションの配列を返す
		/// </summary>
		/// <returns>アクションの配列</returns>
		public Models.LineBot.ReplyMessage.Action[] GetActions() => this.actions;

	}

}