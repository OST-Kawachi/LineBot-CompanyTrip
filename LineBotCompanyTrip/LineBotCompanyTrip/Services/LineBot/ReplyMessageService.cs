using LineBotCompanyTrip.Configurations;
using LineBotCompanyTrip.Models.LineBot.ReplyMessage;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace LineBotCompanyTrip.Services.LineBot {

	/// <summary>
	/// ReplyMessage用サービスクラス
	/// </summary>
	public class ReplyMessageService {
		
		/// <summary>
		/// リクエストEntity
		/// </summary>
		private RequestOfReplyMessage Request { set; get; }

		/// <summary>
		/// メッセージの要素数
		/// </summary>
		private int MessagesIndex { set; get; }

		/// <summary>
		/// メッセージの最大要素数
		/// </summary>
		private readonly int MaxIndex = 5;

		/// <summary>
		/// テンプレートに使用するアクション作成クラス
		/// </summary>
		public class ActionCreator {

			/// <summary>
			/// アクション
			/// </summary>
			private RequestOfReplyMessage.Message.Template.Action[] actions;

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

				this.actions = new RequestOfReplyMessage.Message.Template.Action[ 1 ];

				if( "buttons".Equals( templateType ) ) {
					this.MaxIndex = 4;
				}
				else if( "confirm".Equals( templateType ) ) {
					this.MaxIndex = 2;
				}
				else if( "carousel".Equals( templateType ) ) {
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
					Array.Resize<RequestOfReplyMessage.Message.Template.Action>( ref this.actions , this.ActionsIndex + 1 );
				}

				RequestOfReplyMessage.Message.Template.Action action = new RequestOfReplyMessage.Message.Template.Action();
				action.type = "postback";
				action.label = label;
				action.data = data;
				action.text = text;

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
					Array.Resize<RequestOfReplyMessage.Message.Template.Action>( ref this.actions , this.ActionsIndex + 1 );
				}

				RequestOfReplyMessage.Message.Template.Action action = new RequestOfReplyMessage.Message.Template.Action();
				action.type = "message";
				action.label = label;
				action.text = text;

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
					Array.Resize<RequestOfReplyMessage.Message.Template.Action>( ref this.actions , this.ActionsIndex + 1 );
				}

				RequestOfReplyMessage.Message.Template.Action action = new RequestOfReplyMessage.Message.Template.Action();
				action.type = "uri";
				action.label = label;
				action.uri = uri;

				this.actions[ this.ActionsIndex ] = action;
				this.ActionsIndex++;

				return this;

			}

			/// <summary>
			/// アクションの配列を返す
			/// </summary>
			/// <returns>アクションの配列</returns>
			public RequestOfReplyMessage.Message.Template.Action[] GetActions() {

				return this.actions;

			}
			

		}
		
		/// <summary>
		/// コンストラクタ
		/// リプライトークンを保持する
		/// </summary>
		/// <param name="replyToken">リプライトークン</param>
		public ReplyMessageService( string replyToken ) {
			
			this.Request = new RequestOfReplyMessage();
			this.Request.replyToken = replyToken;
			this.Request.messages = new RequestOfReplyMessage.Message[ 1 ];
			this.MessagesIndex = 0;

		}

		/// <summary>
		/// テキストメッセージを送信リストに追加する
		/// 初回以外は配列を拡大させながら追加する
		/// 5通目以降は追加されない
		/// </summary>
		/// <param name="messageText">メッセージテキスト</param>
		/// <returns>自身のオブジェクト</returns>
		public ReplyMessageService AddTextMessage( string messageText ) {

			if( this.MessagesIndex == this.MaxIndex ) {
				Trace.TraceInformation( this.MaxIndex + "通以上送信できません" );
				return this;
			}
			else if( this.MessagesIndex != 0 ) {
				Array.Resize<RequestOfReplyMessage.Message>( ref this.Request.messages , this.MessagesIndex + 1 );
			}

			RequestOfReplyMessage.Message message = new RequestOfReplyMessage.Message();
			message.type = "text";
			message.text = messageText;

			this.Request.messages[ this.MessagesIndex ] = message;
			this.MessagesIndex++;

			return this;

		}

		/// <summary>
		/// 画像メッセージを送信リストに追加する
		/// 初回以外は配列を拡大させながら追加する
		/// 5通目以降は追加されない
		/// </summary>
		/// <param name="originalContentUrl">画像URL</param>
		/// <param name="previewImageUrl">プレビュー画像URL</param>
		/// <returns>自身のオブジェクト</returns>
		public ReplyMessageService AddImageMessage( string originalContentUrl , string previewImageUrl ) {

			if( this.MessagesIndex == this.MaxIndex ) {
				Trace.TraceInformation( this.MaxIndex + "通以上送信できません" );
				return this;
			}
			else if( this.MessagesIndex != 0 ) {
				Array.Resize<RequestOfReplyMessage.Message>( ref this.Request.messages , this.MessagesIndex + 1 );
			}

			RequestOfReplyMessage.Message message = new RequestOfReplyMessage.Message();
			message.type = "image";
			message.originalContentUrl = originalContentUrl;
			message.previewImageUrl = previewImageUrl;

			this.Request.messages[ this.MessagesIndex ] = message;
			this.MessagesIndex++;

			return this;

		}

		/// <summary>
		/// 位置情報メッセージを送信リストに追加する
		/// 初回以外は配列を拡大させながら追加する
		/// 5通目以降は追加されない
		/// </summary>
		/// <param name="title">タイトル</param>
		/// <param name="address">アドレス</param>
		/// <param name="latitude">緯度</param>
		/// <param name="longitude">経度</param>
		/// <returns>自身のオブジェクト</returns>
		public ReplyMessageService AddLocationMessage( 
			string title , 
			string address , 
			double latitude , 
			double longitude 
		) {

			if( this.MessagesIndex == this.MaxIndex ) {
				Trace.TraceInformation( this.MaxIndex + "通以上送信できません" );
				return this;
			}
			else if( this.MessagesIndex != 0 ) {
				Array.Resize<RequestOfReplyMessage.Message>( ref this.Request.messages , this.MessagesIndex + 1 );
			}

			RequestOfReplyMessage.Message message = new RequestOfReplyMessage.Message();
			message.type = "location";
			message.title = title;
			message.address = address;
			message.latitude = latitude;
			message.longitude = longitude;

			this.Request.messages[ this.MessagesIndex ] = message;
			this.MessagesIndex++;

			return this;

		}

		/// <summary>
		/// 画像、タイトル、テキスト、複数のアクションボタンを組み合わせたテンプレートメッセージを送信リストに追加する
		/// 初回以外は配列を拡大させながら追加する
		/// 5通目以降は追加されない
		/// アクションボタンは最大4つ
		/// </summary>
		/// <param name="altText">代替テキスト</param>
		/// <param name="thumbnailImageUrl">画像のURL</param>
		/// <param name="title">タイトル</param>
		/// <param name="text">説明文</param>
		/// <param name="actions">ボタン押下時アクション</param>
		/// <returns>自身のオブジェクト</returns>
		public ReplyMessageService AddButtonsMessage( 
			string altText , 
			string thumbnailImageUrl ,
			string title ,
			string text ,
			RequestOfReplyMessage.Message.Template.Action[] actions 
		) {

			if( this.MessagesIndex == this.MaxIndex ) {
				Trace.TraceInformation( this.MaxIndex + "通以上送信できません" );
				return this;
			}
			else if( this.MessagesIndex != 0 ) {
				Array.Resize<RequestOfReplyMessage.Message>( ref this.Request.messages , this.MessagesIndex + 1 );
			}

			RequestOfReplyMessage.Message.Template template = new RequestOfReplyMessage.Message.Template();
			template.type = "buttons";
			template.thumbnailImageUrl = thumbnailImageUrl;
			template.title = title;
			template.text = text;
			template.actions = actions;

			RequestOfReplyMessage.Message message = new RequestOfReplyMessage.Message();
			message.type = "template";
			message.altText = altText;
			message.template = template;

			this.Request.messages[ this.MessagesIndex ] = message;
			this.MessagesIndex++;

			return this;

		}

		/// <summary>
		/// 2つのアクションボタンを提示するテンプレートメッセージを送信リストに追加する
		/// 初回以外は配列を拡大させながら追加する
		/// 5通目以降は追加されない
		/// アクションボタンは最大2つ
		/// </summary>
		/// <param name="altText">代替テキスト</param>
		/// <param name="text">説明文</param>
		/// <param name="actions">ボタン押下時アクション</param>
		/// <returns>自身のオブジェクト</returns>
		public ReplyMessageService AddConfirmMessage(
			string altText ,
			string text ,
			RequestOfReplyMessage.Message.Template.Action[] actions
		) {

			if( this.MessagesIndex == this.MaxIndex ) {
				Trace.TraceInformation( this.MaxIndex + "通以上送信できません" );
				return this;
			}
			else if( this.MessagesIndex != 0 ) {
				Array.Resize<RequestOfReplyMessage.Message>( ref this.Request.messages , this.MessagesIndex + 1 );
			}
			
			RequestOfReplyMessage.Message.Template template = new RequestOfReplyMessage.Message.Template();
			template.type = "confirm";
			template.text = text;
			template.actions = actions;

			RequestOfReplyMessage.Message message = new RequestOfReplyMessage.Message();
			message.type = "template";
			message.altText = altText;
			message.template = template;
			
			this.Request.messages[ this.MessagesIndex ] = message;
			this.MessagesIndex++;

			return this;

		}

		/// <summary>
		/// 複数の情報を並べて提示できるカルーセル型のテンプレートメッセージを送信リストに追加する
		/// 初回以外は配列を拡大させながら追加する
		/// 5通目以降は追加されない
		/// カラムは最大5つ
		/// </summary>
		/// <param name="altText">代替テキスト</param>
		/// <param name="columns">カラム</param>
		/// <returns>自身のオブジェクト</returns>
		public ReplyMessageService AddCarouselMessage(
			string altText ,
			RequestOfReplyMessage.Message.Template.Column[] columns
		) {

			if( this.MessagesIndex == this.MaxIndex ) {
				Trace.TraceInformation( this.MaxIndex + "通以上送信できません" );
				return this;
			}
			else if( this.MessagesIndex != 0 ) {
				Array.Resize<RequestOfReplyMessage.Message>( ref this.Request.messages , this.MessagesIndex + 1 );
			}

			RequestOfReplyMessage.Message.Template template = new RequestOfReplyMessage.Message.Template();
			template.type = "carousel";
			template.columns = columns;

			RequestOfReplyMessage.Message message = new RequestOfReplyMessage.Message();
			message.type = "template";
			message.altText = altText;
			message.template = template;
			
			this.Request.messages[ this.MessagesIndex ] = message;
			this.MessagesIndex++;

			return this;

		}
		
		/// <summary>
		/// メッセージの送信
		/// </summary>
		/// <returns></returns>
		public async Task Send() {
			
			string jsonRequest = JsonConvert.SerializeObject( this.Request );
			Trace.TraceInformation( "Reply Message Request is : " + jsonRequest );
			StringContent content = new StringContent( jsonRequest );
			content.Headers.ContentType = new MediaTypeHeaderValue( "application/json" );

			HttpClient client = new HttpClient();
			client.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );
			client.DefaultRequestHeaders.Add( "Authorization" , "Bearer {" + LineBotConfig.ChannelAccessToken + "}" );

			HttpResponseMessage response = await client.PostAsync( LineBotConfig.ReplyMessageUrl , content ).ConfigureAwait( false );
			string result = await response.Content.ReadAsStringAsync().ConfigureAwait( false );
			Trace.TraceInformation( "Reply Message Status Code is : " + response.StatusCode );

		}
		
	}

}