using LineBotCompanyTrip.Models.LineBot.ReplyMessage;
using System;
using System.Diagnostics;

namespace LineBotCompanyTrip.Services.LineBot {

	/// <summary>
	/// カルーセル型テンプレートに使用するカラム作成クラス
	/// </summary>
	public class ColumnCreator {

		/// <summary>
		/// カラム配列
		/// </summary>
		private Column[] columns;

		/// <summary>
		/// カラム配列の長さ
		/// </summary>
		private int ColumnIndex { set; get; }

		/// <summary>
		/// カラム配列の最大値
		/// </summary>
		private int MaxIndex { set; get; }

		/// <summary>
		/// カラム配列を作成する
		/// </summary>
		/// <returns>自身のオブジェクト</returns>
		public ColumnCreator CreateColumn() {
			
			Trace.TraceInformation( "CreateColumn" );

			this.columns = new Column[ 1 ];
			this.MaxIndex = 5;
			this.ColumnIndex = 0;
			
			Trace.TraceInformation( "Max Column Index is : " + this.MaxIndex );

			return this;

		}

		/// <summary>
		/// カラムを追加する
		/// 2つめ以降のカラムは配列を作成しながら追加する
		/// </summary>
		/// <param name="thumbnailImageUrl">画像のURL</param>
		/// <param name="title">タイトル</param>
		/// <param name="text">説明文</param>
		/// <param name="actions">ボタン</param>
		/// <returns></returns>
		public ColumnCreator AddColumn(
			string thumbnailImageUrl ,
			string title ,
			string text ,
			Models.LineBot.ReplyMessage.Action[] actions
		) {

			Trace.TraceInformation( "Add Column" );

			if( this.ColumnIndex == this.MaxIndex ) {
				Trace.TraceWarning( "Column Index == Max Index" );
				return this;
			}
			else if( this.ColumnIndex != 0 ) {
				Array.Resize( ref this.columns , this.ColumnIndex + 1 );
			}
			
			Trace.TraceInformation( "Actions Size is : " + this.columns.Length );

			Column column = new Column() {
				thumbnailImageUrl = thumbnailImageUrl ,
				title = title ,
				text = text ,
				actions = actions
			};
			
			Trace.TraceInformation( "Column Thumbnail Image Url is : " + column.thumbnailImageUrl );
			Trace.TraceInformation( "Column Title is : " + column.title );
			Trace.TraceInformation( "Column Text is : " + column.text );
			Trace.TraceInformation( "Column Actions Length is : " + column.actions.Length );

			this.columns[ this.ColumnIndex ] = column;
			this.ColumnIndex++;

			return this;

		}

		/// <summary>
		/// カラムの配列を返す
		/// </summary>
		/// <returns>カラムの配列を返す</returns>
		public Column[] GetColumns() => this.columns;

	}

}