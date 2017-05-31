using LineBotCompanyTrip.Common;
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
		/// コンストラクタ
		/// リプライトークンを保持する
		/// </summary>
		/// <param name="replyToken">リプライトークン</param>
		public ReplyMessageService( string replyToken ) {

			Trace.TraceInformation( "Reply Message Service Start" );
			Trace.TraceInformation( "Reply Token is : " + replyToken );
			
			this.Request = new RequestOfReplyMessage() {
				replyToken = replyToken ,
				messages = new Message[ 1 ]
			};
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

			Trace.TraceInformation( "Add Text Message" );

			if( this.MessagesIndex == this.MaxIndex ) {
				Trace.TraceWarning( "Message Index == Max Index" );
				return this;
			}
			else if( this.MessagesIndex != 0 ) {
				Array.Resize( ref this.Request.messages , this.MessagesIndex + 1 );
			}

			Trace.TraceInformation( "Messages Size is : " + this.Request.messages.Length );

			Message message = new Message() {
				type = CommonEnum.MessageType.text.ToString() ,
				text = messageText
			};

			Trace.TraceInformation( "Message Type is : " + message.type );
			Trace.TraceInformation( "Message Text is : " + message.text );
			
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

			Trace.TraceInformation( "Add Image Message" );

			if( this.MessagesIndex == this.MaxIndex ) {
				Trace.TraceWarning( "Message Index == Max Index" );
				return this;
			}
			else if( this.MessagesIndex != 0 ) {
				Array.Resize( ref this.Request.messages , this.MessagesIndex + 1 );
			}

			Trace.TraceInformation( "Messages Size is : " + this.Request.messages.Length );

			Message message = new Message() {
				type = CommonEnum.MessageType.image.ToString() ,
				originalContentUrl = originalContentUrl ,
				previewImageUrl = previewImageUrl
			};
			
			Trace.TraceInformation( "Message Type is : " + message.type );
			Trace.TraceInformation( "Message Original Content Url is : " + message.originalContentUrl );
			Trace.TraceInformation( "Message Preview Content Url is : " + message.previewImageUrl );

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

			Trace.TraceInformation( "Add Location Message" );

			if( this.MessagesIndex == this.MaxIndex ) {
				Trace.TraceWarning( "Message Index == Max Index" );
				return this;
			}
			else if( this.MessagesIndex != 0 ) {
				Array.Resize( ref this.Request.messages , this.MessagesIndex + 1 );
			}
			
			Trace.TraceInformation( "Messages Size is : " + this.Request.messages.Length );

			Message message = new Message() {
				type = CommonEnum.MessageType.location.ToString() ,
				title = title ,
				address = address ,
				latitude = latitude ,
				longitude = longitude
			};

			Trace.TraceInformation( "Message Type is : " + message.type );
			Trace.TraceInformation( "Message Title is : " + message.title );
			Trace.TraceInformation( "Message Address is : " + message.address );
			Trace.TraceInformation( "Message Latitude is : " + message.latitude );
			Trace.TraceInformation( "Message Longitude is : " + message.longitude );

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
			Models.LineBot.ReplyMessage.Action[] actions 
		) {
			
			Trace.TraceInformation( "Add Button Message" );

			if( this.MessagesIndex == this.MaxIndex ) {
				Trace.TraceWarning( "Message Index == Max Index" );
				return this;
			}
			else if( this.MessagesIndex != 0 ) {
				Array.Resize( ref this.Request.messages , this.MessagesIndex + 1 );
			}

			Trace.TraceInformation( "Messages Size is : " + this.Request.messages.Length );

			Template template = new Template() {
				type = CommonEnum.TemplateType.buttons.ToString() ,
				thumbnailImageUrl = thumbnailImageUrl ,
				title = title ,
				text = text ,
				actions = actions
			};
			
			Message message = new Message() {
				type = CommonEnum.MessageType.template.ToString() ,
				altText = altText ,
				template = template
			};
			
			Trace.TraceInformation( "Message Type is : " + message.type );
			Trace.TraceInformation( "Message Alt Text is : " + message.altText );
			Trace.TraceInformation( "Message Template Type is : " + message.template.type );
			Trace.TraceInformation( "Message Template Thumbnail Image Url is : " + message.template.thumbnailImageUrl );
			Trace.TraceInformation( "Message Template Title is : " + message.template.title );
			Trace.TraceInformation( "Message Template Text is : " + message.template.text );
			Trace.TraceInformation( "Message Template Actions Length is : " + message.template.actions.Length );

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
			Models.LineBot.ReplyMessage.Action[] actions
		) {

			Trace.TraceInformation( "Add Confirm Message" );

			if( this.MessagesIndex == this.MaxIndex ) {
				Trace.TraceWarning( "Message Index == Max Index" );
				return this;
			}
			else if( this.MessagesIndex != 0 ) {
				Array.Resize( ref this.Request.messages , this.MessagesIndex + 1 );
			}

			Trace.TraceInformation( "Messages Size is : " + this.Request.messages.Length );

			Template template = new Template() {
				type = CommonEnum.TemplateType.confirm.ToString() ,
				text = text ,
				actions = actions
			};

			Message message = new Message() {
				type = CommonEnum.MessageType.template.ToString() ,
				altText = altText ,
				template = template
			};

			Trace.TraceInformation( "Message Type is : " + message.type );
			Trace.TraceInformation( "Message Alt Text is : " + message.altText );
			Trace.TraceInformation( "Message Template Type is : " + message.template.type );
			Trace.TraceInformation( "Message Template Text is : " + message.template.text );
			Trace.TraceInformation( "Message Template Actions Length is : " + message.template.actions.Length );

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
			Column[] columns
		) {
			
			Trace.TraceInformation( "Add Carousel Message" );

			if( this.MessagesIndex == this.MaxIndex ) {
				Trace.TraceWarning( "Message Index == Max Index" );
				return this;
			}
			else if( this.MessagesIndex != 0 ) {
				Array.Resize( ref this.Request.messages , this.MessagesIndex + 1 );
			}

			Trace.TraceInformation( "Messages Size is : " + this.Request.messages.Length );

			Template template = new Template() {
				type = CommonEnum.TemplateType.carousel.ToString() ,
				columns = columns
			};

			Message message = new Message() {
				type = CommonEnum.MessageType.template.ToString() ,
				altText = altText ,
				template = template
			};
			
			Trace.TraceInformation( "Message Type is : " + message.type );
			Trace.TraceInformation( "Message Alt Text is : " + message.altText );
			Trace.TraceInformation( "Message Template Type is : " + message.template.type );
			Trace.TraceInformation( "Message Template Column Length is : " + message.template.columns.Length );

			this.Request.messages[ this.MessagesIndex ] = message;
			this.MessagesIndex++;

			return this;

		}
		
		/// <summary>
		/// メッセージの送信
		/// </summary>
		/// <returns></returns>
		public async Task Send() {
			
			Trace.TraceInformation( "Reply Message Send Start" );

			string jsonRequest = JsonConvert.SerializeObject( this.Request );
			Trace.TraceInformation( "Reply Message Request is : " + jsonRequest );
			StringContent content = new StringContent( jsonRequest );
			content.Headers.ContentType = new MediaTypeHeaderValue( "application/json" );

			HttpClient client = new HttpClient();
			client.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );
			client.DefaultRequestHeaders.Add( "Authorization" , "Bearer {" + LineBotConfig.ChannelAccessToken + "}" );

			try {
				HttpResponseMessage response = await client.PostAsync( LineBotConfig.ReplyMessageUrl , content );
				string result = await response.Content.ReadAsStringAsync();
				Trace.TraceInformation( "Reply Message Status Code is : " + response.StatusCode );
				response.Dispose();
			}
			catch( ArgumentNullException e ) {
				Trace.TraceError( "Reply Message Send Argument Null Exception " + e.Message );
			}
			catch( HttpRequestException e ) {
				Trace.TraceError( "Reply Message Send Http Request Exception " + e.Message );
			}
			catch( Exception e ) {
				Trace.TraceError( "Reply Message Send 予期せぬ例外 " + e.Message );
			}

			content.Dispose();
			client.Dispose();
			
			Trace.TraceInformation( "Reply Message Send End" );

		}
		
	}

}