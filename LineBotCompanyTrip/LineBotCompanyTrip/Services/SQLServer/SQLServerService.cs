using System.Collections.Generic;
using System.Diagnostics;
using LineBotCompanyTrip.Common;
using LineBotCompanyTrip.Models.AzureCognitiveServices.EmotionAPI;
using System.Data.SqlClient;
using LineBotCompanyTrip.Configurations;
using System;
using System.IO;
using System.Configuration;

namespace LineBotCompanyTrip.Services.SQLServer {

	/// <summary>
	/// SQLサーバーに関するサービスクラス
	/// </summary>
	public class SQLServerService {

		/// <summary>
		/// 接続文字列
		/// </summary>
		private string connectionString;

		/// <summary>
		/// コンストラクタ
		/// SQL Serverとのコネクションの設定を行う
		/// </summary>
		public SQLServerService() {

			SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder() {
				DataSource = SQLServerConfig.ServerName ,
				IntegratedSecurity = false ,
				UserID = SQLServerConfig.UserName ,
				Password = SQLServerConfig.Password ,
				InitialCatalog = SQLServerConfig.DatabaseName
			};

			this.connectionString = builder.ToString();

			Trace.TraceInformation( "Azure SQL Server Connection String Create : SUCCESS" );
			Trace.TraceInformation( "Connection String is : " + this.connectionString );
			
		}

		/// <summary>
		/// WHERE句に使用するLine.UserIdでの絞込みまたはLine.GroupIdでの絞込み分
		/// </summary>
		/// <param name="isUserId">ユーザIDかどうか</param>
		/// <param name="id">ユーザIDまたはグループID</param>
		/// <returns>Line.UserId = @UserId　または　Line.GroupId = @GroupId</returns>
		private string GetWhereUserIdOrGroupId( bool isUserId , string id ) => (
			isUserId
			? @" CONVERT( VARCHAR , Line.UserId ) = CONVERT( VARCHAR , @UserId ) "
			: @" CONVERT( VARCHAR , Line.GroupId ) = CONVERT( VARCHAR , @GroupId ) "
		);
		
		/// <summary>
		/// MostEmotionのメソッドで表情種別毎に何度も呼ばれるサブクエリを共通化
		/// </summary>
		/// <param name="typeString"></param>
		/// <param name="isUserId"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		private string GetSubQueryOfMostEmotion( string typeString , bool isUserId , string id ) => @"
			SELECT TOP( 3 ) Face.ProcessedPicturePath as Path , Face." + typeString + @" as Value , '" + typeString + @"' as Type
			FROM Face
			WHERE Face." + typeString + @" IN(
				SELECT TOP( 3 ) MAX( Face." + typeString + @" ) as Value
				FROM Face
				INNER JOIN Picture
				ON Face.PictureId = Picture.PictureId
				INNER JOIN Line
				ON Picture.LineId = Line.LineId
				WHERE " + this.GetWhereUserIdOrGroupId( isUserId , id ) + @"
				GROUP BY Face." + typeString + @"
				ORDER BY Face." + typeString + @" DESC
			) 
			ORDER BY Face." + typeString + @" DESC
			";

		/// <summary>
		/// 登録されているLineIDのうち、最大のものを取得する
		/// メソッド内でSqlConnection.Close()はしない
		/// </summary>
		/// <param name="alreadyOpenedConnection">既にSqlConnection.Open()が呼ばれているコネクション</param>
		/// <returns>取得できなかった場合は-1</returns>
		private int GetMaxLineId( SqlConnection alreadyOpenedConnection ) {

			Trace.TraceInformation( "Get Max Line Id Start" );

			int maxLineId = -1;

			string selectSql = @"
				SELECT TOP( 1 ) Line.LineId as LineId
				FROM Line
				ORDER BY Line.LineId DESC;
			";
			Trace.TraceInformation( "Get Max Line Id SQL is : " + selectSql );

			SqlCommand selectSqlCommand = new SqlCommand( selectSql , alreadyOpenedConnection );

			Trace.TraceInformation( "SQL Execute" );
			SqlDataReader reader = null;
			try {
				reader = selectSqlCommand.ExecuteReader();
				while( reader.Read() == true ) {
					int? readerLineId = reader[ "LineId" ] as int?;
					maxLineId = readerLineId.GetValueOrDefault( -1 );
					break;
				}
				reader.Close();
			}
			catch( InvalidCastException e ) {
				Trace.TraceError( "Get Max Line Id Invalid Cast Exception : " + e.Message );
				reader?.Close();
			}
			catch( SqlException e ) {
				Trace.TraceError( "Get Max Line Id Sql Exception : " + e.Message );
				reader?.Close();
			}
			catch( ObjectDisposedException e ) {
				Trace.TraceError( "Get Max Line Id Object Disposed Exception : " + e.Message );
				reader?.Close();
			}
			catch( InvalidOperationException e ) {
				Trace.TraceError( "Get Max Line Id Invalid Operation Exception : " + e.Message );
				reader?.Close();
			}
			catch( IOException e ) {
				Trace.TraceError( "Get Max Line Id IOException : " + e.Message );
				reader?.Close();
			}
			catch( Exception e ) {
				Trace.TraceError( "Get Max Line Id 予期せぬ例外 : " + e.Message );
				reader?.Close();
			}
			
			Trace.TraceInformation( "Max Line Id is : " + maxLineId );

			Trace.TraceInformation( "Get Max Line Id End" );

			return maxLineId;

		}

		/// <summary>
		/// LineIdを取得する
		/// メソッド内でSqlConnection.Close()はしない
		/// </summary>
		/// <param name="alreadyOpenedConnection">既にSqlConnection.Open()が呼ばれているコネクション</param>
		/// <param name="lineId">LINEのid</param>
		/// <param name="isUserId"><see cref="id"/>がユーザIDかどうか</param>
		/// <returns>LineId</returns>
		private int GetLineId( SqlConnection alreadyOpenedConnection , bool isUserId , string id ) {

			Trace.TraceInformation( "Get Line Id Start" );

			Trace.TraceInformation( "Get Line Id Is User Id is : " + isUserId );
			Trace.TraceInformation( "Get Line Id Id is : " + id );

			int lineId = -1;

			string selectLineIdSql = @"
				SELECT Line.LineId as LineId
				FROM Line
				WHERE " + this.GetWhereUserIdOrGroupId( isUserId , id ) + ";";
			Trace.TraceInformation( "Get Line Id SQL is : " + selectLineIdSql );
			
			SqlCommand selectSqlCommand = new SqlCommand( selectLineIdSql , alreadyOpenedConnection );
			if( isUserId )
				selectSqlCommand.Parameters.AddWithValue( "@UserId" , id );
			else 
				selectSqlCommand.Parameters.AddWithValue( "@GroupId" , id );


			Trace.TraceInformation( "SQL Execute" );
			SqlDataReader reader = null;
			try {
				reader = selectSqlCommand.ExecuteReader();
				while( reader.Read() == true ) {
					int? readerLineId = reader[ "LineId" ] as int?;
					lineId = readerLineId.GetValueOrDefault( -1 );
					break;
				}
				reader.Close();
			}
			catch( InvalidCastException e ) {
				Trace.TraceError( "Get Line Id Invalid Cast Exception : " + e.Message );
				reader?.Close();
			}
			catch( SqlException e ) {
				Trace.TraceError( "Get Line Id Sql Exception : " + e.Message );
				reader?.Close();
			}
			catch( ObjectDisposedException e ) {
				Trace.TraceError( "Get Line Id Object Disposed Exception : " + e.Message );
				reader?.Close();
			}
			catch( InvalidOperationException e ) {
				Trace.TraceError( "Get Line Id Invalid Operation Exception : " + e.Message );
				reader?.Close();
			}
			catch( IOException e ) {
				Trace.TraceError( "Get Line Id IOException : " + e.Message );
				reader?.Close();
			}
			catch( Exception e ) {
				Trace.TraceError( "Get Line Id 予期せぬ例外 : " + e.Message );
				reader?.Close();
			}
			
			Trace.TraceInformation( "Line Id is : " + lineId );
			
			Trace.TraceInformation( "Get Line Id End" );

			return lineId;

		}

		/// <summary>
		/// ライン情報を登録する
		/// メソッド内でSqlConnection.Close()はしない
		/// </summary>
		/// <param name="alreadyOpenedConnection">既にSqlConnection.Open()が呼ばれているコネクション</param>
		/// <param name="lineId">LINEのid</param>
		/// <param name="isUserId"><see cref="id"/>がユーザIDかどうか</param>
		/// <param name="id">ユーザIDまたはグループID</param>
		private void InsertLine( SqlConnection alreadyOpenedConnection , int lineId , bool isUserId , string id ) {

			Trace.TraceInformation( "Insert Line Start" );

			Trace.TraceInformation( "Insert Line Id is : " + lineId );
			Trace.TraceInformation( "Insert Is User Id is :" + isUserId );
			Trace.TraceInformation( "Insert Id is :" + id );
			
			string insertSql = @"
				INSERT INTO Line 
				( LineId , Type , UserId , GroupId , PostbackStatus ) 
				VALUES 
				( @LineId , @Type , @UserId , @GroupId , @PostbackStatus );
			";

			Trace.TraceInformation( "Insert Line SQL is : " + insertSql );

			SqlCommand insertSqlCommand = new SqlCommand( insertSql , alreadyOpenedConnection );
			insertSqlCommand.Parameters.AddWithValue( "@LineId" , lineId );
			insertSqlCommand.Parameters.AddWithValue( "@Type" , ( isUserId ? 0 : 1 ) );
			insertSqlCommand.Parameters.AddWithValue( "@UserId" , ( isUserId ? id : "" ) );
			insertSqlCommand.Parameters.AddWithValue( "@GroupId" , ( isUserId ? "" : id ) );
			insertSqlCommand.Parameters.AddWithValue( "@PostbackStatus" , 0 );

			int result = 0;
			try {
				Trace.TraceInformation( "SQL Execute" );
				result = insertSqlCommand.ExecuteNonQuery();
			}
			catch( InvalidCastException e ) {
				Trace.TraceError( "Insert Line Invalid Cast Exception : " + e.Message );
			}
			catch( SqlException e ) {
				Trace.TraceError( "Insert Line Sql Exception : " + e.Message );
			}
			catch( IOException e ) {
				Trace.TraceError( "Insert Line IOException : " + e.Message );
			}
			catch( ObjectDisposedException e ) {
				Trace.TraceError( "Insert Line Object Disposed Exception : " + e.Message );
			}
			catch( InvalidOperationException e ) {
				Trace.TraceError( "Insert Line Invalid Operation Exception : " + e.Message );
			}
			catch( Exception e ) {
				Trace.TraceError( "Insert Line 予期せぬ例外 : " + e.Message );
			}
			
			Trace.TraceInformation( "Inser Result is : " + result );

			Trace.TraceInformation( "Insert Line End" );

		}

		/// <summary>
		/// Postbackを取得する
		/// メソッド内でSqlConnection.Close()はしない
		/// </summary>
		/// <param name="alreadyOpenedConnection">既にSqlConnection.Open()が呼ばれているコネクション</param>
		/// <param name="isUserId">idがユーザIDかどうか</param>
		/// <param name="id">ユーザIDまたはグループID</param>
		/// <returns>PostbackStatus</returns>
		private bool GetPostback( SqlConnection alreadyOpenedConnection , bool isUserId , string id ) {

			Trace.TraceInformation( "Get Postback Start" );

			Trace.TraceInformation( "Get Postback Id is : " + id );
			Trace.TraceInformation( "Get Postback Is User Id is :" + isUserId );

			string selectSql = @"
				SELECT Line.PostbackStatus as Status
				FROM Line
				WHERE " + this.GetWhereUserIdOrGroupId( isUserId , id ) + ";";

			Trace.TraceInformation( "Get Postback SQL is : " + selectSql );

			SqlCommand selectSqlCommand = new SqlCommand( selectSql , alreadyOpenedConnection );
			if( isUserId )
				selectSqlCommand.Parameters.AddWithValue( "@UserId" , id );
			else
				selectSqlCommand.Parameters.AddWithValue( "@GroupId" , id );
			
			Trace.TraceInformation( "SQL Execute" );
			SqlDataReader reader = null;
			bool postbackStatus = false;
			try {
				reader = selectSqlCommand.ExecuteReader();
				while( reader.Read() == true ) {
					int? status = reader[ "Status" ] as int?;
					postbackStatus = status.HasValue ? status.Value == 1 : false;
					break;
				}
			}
			catch( InvalidCastException e ) {
				Trace.TraceError( "Get Max Line Id Invalid Cast Exception : " + e.Message );
				reader?.Close();
			}
			catch( SqlException e ) {
				Trace.TraceError( "Get Max Line Id Sql Exception : " + e.Message );
				reader?.Close();
			}
			catch( ObjectDisposedException e ) {
				Trace.TraceError( "Get Max Line Id Object Disposed Exception : " + e.Message );
				reader?.Close();
			}
			catch( InvalidOperationException e ) {
				Trace.TraceError( "Get Max Line Id Invalid Operation Exception : " + e.Message );
				reader?.Close();
			}
			catch( IOException e ) {
				Trace.TraceError( "Get Max Line Id IOException : " + e.Message );
				reader?.Close();
			}
			catch( Exception e ) {
				Trace.TraceError( "Get Max Line Id 予期せぬ例外 : " + e.Message );
				reader?.Close();
			}
			
			Trace.TraceInformation( "Postback is : " + postbackStatus );

			Trace.TraceInformation( "Get Postback End" );

			return postbackStatus;

		}

		/// <summary>
		/// Postbackを更新する
		/// </summary>
		/// <param name="alreadyOpenedConnection">既にSqlConnection.Open()が呼ばれているコネクション</param>
		/// <param name="isUserId">idがユーザIDかどうか</param>
		/// <param name="id">ユーザIDまたはグループID</param>
		/// <param name="isInitialization">初期化するかどうか</param>
		private void UpdatePostback( SqlConnection alreadyOpenedConnection , bool isUserId , string id , bool isInitialization ) {
			
			Trace.TraceInformation( "Update Postback Start" );

			Trace.TraceInformation( "Update Postback Is User Id : " + isUserId );
			Trace.TraceInformation( "Update Postback Id is : " + id );
			Trace.TraceInformation( "Update Postback Is Initialization is : " + isInitialization );

			string updateSql = @"
				UPDATE Line
				SET Line.PostbackStatus = @Status
				WHERE " + this.GetWhereUserIdOrGroupId( isUserId , id ) + ";";

			Trace.TraceInformation( "Update Postback SQL is : " + updateSql );
			
			SqlCommand updateSqlCommand = new SqlCommand( updateSql , alreadyOpenedConnection );
			updateSqlCommand.Parameters.AddWithValue( "@Status" , isInitialization ? 0 : 1 );
			if( isUserId )
				updateSqlCommand.Parameters.AddWithValue( "@UserId" , id );
			else
				updateSqlCommand.Parameters.AddWithValue( "@GroupId" , id );

			Trace.TraceInformation( "SQL Execute" );

			int resultRowCount = 0;
			try {
				resultRowCount = updateSqlCommand.ExecuteNonQuery();
			}
			catch( InvalidCastException e ) {
				Trace.TraceError( "Update Postback Invalid Cast Exception : " + e.Message );
			}
			catch( SqlException e ) {
				Trace.TraceError( "Update Postback Sql Exception : " + e.Message );
			}
			catch( IOException e ) {
				Trace.TraceError( "Update Postback IOException : " + e.Message );
			}
			catch( ObjectDisposedException e ) {
				Trace.TraceError( "Update Postback Object Disposed Exception : " + e.Message );
			}
			catch( InvalidOperationException e ) {
				Trace.TraceError( "Update Postback Invalid Operation Exception : " + e.Message );
			}
			catch( Exception e ) {
				Trace.TraceError( "Update Postback 予期せぬ例外 : " + e.Message );
			}
			
			Trace.TraceInformation( "Update Postback Count is : " + resultRowCount );
			
			Trace.TraceInformation( "Update Postback End" );

		}

		/// <summary>
		/// PictureIdの最大値を取得する
		/// </summary>
		/// <param name="alreadyOpenedConnection">既にSqlConnection.Open()が呼ばれているコネクション</param>
		/// <returns>PictureIdの最大値</returns>
		private int GetMaxPictureId( SqlConnection alreadyOpenedConnection ) {

			Trace.TraceInformation( "Get Max Picture Id Start" );

			int maxPictureId = -1;

			string selectSql = @"
				SELECT TOP( 1 ) Picture.PictureId as PictureId
				FROM Picture
				ORDER BY Picture.PictureId DESC;";					
			Trace.TraceInformation( "Get Max Picture Id SQL is : " + selectSql );

			SqlCommand selectSqlCommand = new SqlCommand( selectSql , alreadyOpenedConnection );

			Trace.TraceInformation( "SQL Execute" );
			SqlDataReader reader = null;
			try {
				reader = selectSqlCommand.ExecuteReader();
				while( reader.Read() == true ) {
					int? readerLineId = reader[ "PictureId" ] as int?;
					maxPictureId = readerLineId.GetValueOrDefault( -1 );
					break;
				}
				reader.Close();
			}
			catch( InvalidCastException e ) {
				Trace.TraceError( "Get Max Picture Id Invalid Cast Exception : " + e.Message );
				reader?.Close();
			}
			catch( SqlException e ) {
				Trace.TraceError( "Get Max Picture Id Sql Exception : " + e.Message );
				reader?.Close();
			}
			catch( ObjectDisposedException e ) {
				Trace.TraceError( "Get Max Picture Id Object Disposed Exception : " + e.Message );
				reader?.Close();
			}
			catch( InvalidOperationException e ) {
				Trace.TraceError( "Get Max Picture Id Invalid Operation Exception : " + e.Message );
				reader?.Close();
			}
			catch( IOException e ) {
				Trace.TraceError( "Get Max Picture Id IOException : " + e.Message );
				reader?.Close();
			}
			catch( Exception e ) {
				Trace.TraceError( "Get Max Picture Id 予期せぬ例外 : " + e.Message );
				reader?.Close();
			}

			Trace.TraceInformation( "Max Picture Id is : " + maxPictureId );

			Trace.TraceInformation( "Get Max Picture Id End" );

			return maxPictureId;
			
		}

		/// <summary>
		/// 画像を登録する
		/// </summary>
		/// <param name="alreadyOpenedConnection">既にSqlConnection.Open()が呼ばれているコネクション</param>
		/// <param name="isUserId">idがユーザIDかどうか</param>
		/// <param name="pictureId">PictureID</param>
		/// <param name="path">画像URL</param>
		/// <param name="lineId">LineId</param>
		private void InsertPicture( SqlConnection alreadyOpenedConnection , int pictureId , string path , int lineId ) {

			Trace.TraceInformation( "Insert Picture Start" );

			Trace.TraceInformation( "Inser Picture Picture Id is : " + pictureId );
			Trace.TraceInformation( "Insert Picture Path is : " + path );

			string insertSql = @"
				INSERT INTO Picture 
				( PictureId , OriginalUrl , LineId ) 
				VALUES 
				( @PictureId , @OriginalUrl , @LineId );";
			Trace.TraceInformation( "Inser Picture SQL is : " + insertSql );

			SqlCommand insertSqlCommand = new SqlCommand( insertSql , alreadyOpenedConnection );
			insertSqlCommand.Parameters.AddWithValue( "@PictureId" , pictureId );
			insertSqlCommand.Parameters.AddWithValue( "@OriginalUrl" , path );
			insertSqlCommand.Parameters.AddWithValue( "@LineId" , lineId );

			Trace.TraceInformation( "SQL Execute" );
			try {
				insertSqlCommand.ExecuteNonQuery();
			}
			catch( InvalidCastException e ) {
				Trace.TraceError( "Insert Picture Invalid Cast Exception : " + e.Message );
			}
			catch( SqlException e ) {
				Trace.TraceError( "Insert Picture Sql Exception : " + e.Message );
			}
			catch( IOException e ) {
				Trace.TraceError( "Insert Picture IOException : " + e.Message );
			}
			catch( ObjectDisposedException e ) {
				Trace.TraceError( "Insert Picture Object Disposed Exception : " + e.Message );
			}
			catch( InvalidOperationException e ) {
				Trace.TraceError( "Insert Picture Invalid Operation Exception : " + e.Message );
			}
			catch( Exception e ) {
				Trace.TraceError( "Insert Picture 予期せぬ例外 : " + e.Message );
			}
			
			Trace.TraceInformation( "Insert Picture End" );
			
		}

		/// <summary>
		/// FaceIdの最大値を取得する
		/// </summary>
		/// <param name="alreadyOpenedConnection">既にSqlConnection.Open()が呼ばれているコネクション</param>
		/// <returns>FaceIdの最大値</returns>
		private int GetMaxFaceId( SqlConnection alreadyOpenedConnection ) {

			Trace.TraceInformation( "Get Max Face Id Start" );

			int maxFaceId = -1;

			string selectSql = @"
				SELECT Face.Id as Id
				FROM Face
				ORDER BY Face.Id DESC;";

			Trace.TraceInformation( "Get Max Face Id SQL is : " + selectSql );

			SqlCommand selectSqlCommand = new SqlCommand( selectSql , alreadyOpenedConnection );

			Trace.TraceInformation( "SQL Execute" );
			SqlDataReader reader = null;
			try {
				reader = selectSqlCommand.ExecuteReader();
				while( reader.Read() == true ) {
					int? readerLineId = reader[ "Id" ] as int?;
					maxFaceId = readerLineId.GetValueOrDefault( -1 );
					break;
				}
				reader.Close();
			}
			catch( InvalidCastException e ) {
				Trace.TraceError( "Get Max Face Id Invalid Cast Exception : " + e.Message );
				reader?.Close();
			}
			catch( SqlException e ) {
				Trace.TraceError( "Get Max Face Id Sql Exception : " + e.Message );
				reader?.Close();
			}
			catch( ObjectDisposedException e ) {
				Trace.TraceError( "Get Max Face Id Object Disposed Exception : " + e.Message );
				reader?.Close();
			}
			catch( InvalidOperationException e ) {
				Trace.TraceError( "Get Max Face Id Invalid Operation Exception : " + e.Message );
				reader?.Close();
			}
			catch( IOException e ) {
				Trace.TraceError( "Get Max Face Id IOException : " + e.Message );
				reader?.Close();
			}
			catch( Exception e ) {
				Trace.TraceError( "Get Max Face Id 予期せぬ例外 : " + e.Message );
				reader?.Close();
			}

			Trace.TraceInformation( "Max Face Id is : " + maxFaceId );

			Trace.TraceInformation( "Get Max Face Id End" );

			return maxFaceId;
			
		}

		/// <summary>
		/// Face登録
		/// </summary>
		/// <param name="alreadyOpenedConnection">既にSqlConnection.Open()が呼ばれているコネクション</param>
		/// <param name="id">管理番号</param>
		/// <param name="faceId">FaceId</param>
		/// <param name="response">Emotion API - Recognitionのレスポンス</param>
		/// <param name="pictureId">PictureId</param>
		/// <param name="path">画像パス</param>
		private void InsertFace( SqlConnection alreadyOpenedConnection , int id , string faceId , ResponseOfEmotionRecognitionAPI response , int pictureId , string path ) {

			Trace.TraceInformation( "Insert Face Start" );
			string insertSql = @"
				INSERT INTO Face
				( Id , FaceId , TopPos , LeftPos , Height , Width , Anger , Contempt , Disgust , Fear , Happiness , Neutral , Sadness , Surprise , PictureId , ProcessedPicturePath )
				VALUES 
				( @Id , @FaceId , @TopPos , @LeftPos , @Height , @Width , @Anger , @Contempt , @Disgust , @Fear , @Happiness , @Neutral , @Sadness , @Surprise , @PictureId , @ProcessedPicturePath );";
			Trace.TraceInformation( "Insert Face Sql is : " + insertSql );

			SqlCommand insertSqlCommand = new SqlCommand( insertSql , alreadyOpenedConnection );
			insertSqlCommand.Parameters.AddWithValue( "@Id" , id );
			insertSqlCommand.Parameters.AddWithValue( "@FaceId" , faceId );
			insertSqlCommand.Parameters.AddWithValue( "@TopPos" , response.faceRectangle.top );
			insertSqlCommand.Parameters.AddWithValue( "@LeftPos" , response.faceRectangle.left );
			insertSqlCommand.Parameters.AddWithValue( "@Height" , response.faceRectangle.height );
			insertSqlCommand.Parameters.AddWithValue( "@Width" , response.faceRectangle.width );
			insertSqlCommand.Parameters.AddWithValue( "@Anger" , response.scores.anger );
			insertSqlCommand.Parameters.AddWithValue( "@Contempt" , response.scores.contempt );
			insertSqlCommand.Parameters.AddWithValue( "@Disgust" , response.scores.disgust );
			insertSqlCommand.Parameters.AddWithValue( "@Fear" , response.scores.fear );
			insertSqlCommand.Parameters.AddWithValue( "@Happiness" , response.scores.happiness );
			insertSqlCommand.Parameters.AddWithValue( "@Neutral" , response.scores.neutral );
			insertSqlCommand.Parameters.AddWithValue( "@Sadness" , response.scores.sadness );
			insertSqlCommand.Parameters.AddWithValue( "@Surprise" , response.scores.surprise );
			insertSqlCommand.Parameters.AddWithValue( "@PictureId" , pictureId );
			insertSqlCommand.Parameters.AddWithValue( "@ProcessedPicturePath" , path );

			Trace.TraceInformation( "SQL Execute" );
			try {
				insertSqlCommand.ExecuteNonQuery();
			}
			catch( InvalidCastException e ) {
				Trace.TraceError( "Insert Face Invalid Cast Exception : " + e.Message );
			}
			catch( SqlException e ) {
				Trace.TraceError( "Insert Face Sql Exception : " + e.Message );
			}
			catch( IOException e ) {
				Trace.TraceError( "Insert Face IOException : " + e.Message );
			}
			catch( ObjectDisposedException e ) {
				Trace.TraceError( "Insert Face Object Disposed Exception : " + e.Message );
			}
			catch( InvalidOperationException e ) {
				Trace.TraceError( "Insert Face Invalid Operation Exception : " + e.Message );
			}
			catch( Exception e ) {
				Trace.TraceError( "Insert Face 予期せぬ例外 : " + e.Message );
			}

			Trace.TraceInformation( "Insert Face End" );

		}

		/// <summary>
		/// 顔IDリスト取得
		/// </summary>
		/// <param name="alreadyOpenedConnection">既にSqlConnection.Open()が呼ばれているコネクション</param>
		/// <param name="isUserId">idがユーザIDかどうか</param>
		/// <param name="id">ユーザIDまたはグループID</param>
		/// <returns>顔IDリスト</returns>
		public List<string> GetFaceIds( SqlConnection alreadyOpenedConnection , bool isUserId , string id ) {

			Trace.TraceInformation( "Get Face Ids Start" );
			string selectSql = @"
				SELECT Face.FaceId as Id
				FROM Face
				INNER JOIN Picture
				ON Face.PictureId = Picture.PictureId
				INNER JOIN Line
				ON Picture.LineId = Line.LineId
				WHERE " + this.GetWhereUserIdOrGroupId( isUserId , id ) + @" ;";
			Trace.TraceInformation( "Insert Face Sql is : " + selectSql );

			SqlCommand selectSqlCommand = new SqlCommand( selectSql , alreadyOpenedConnection );
			if( isUserId )
				selectSqlCommand.Parameters.AddWithValue( "@UserId" , id );
			else
				selectSqlCommand.Parameters.AddWithValue( "@GroupId" , id );

			Trace.TraceInformation( "SQL Execute" );
			SqlDataReader reader = null;
			List<string> ids = new List<string>();
			try {
				reader = selectSqlCommand.ExecuteReader();
				while( reader.Read() == true ) {
					string readerFaceId = reader[ "Id" ] as string;
					ids.Add( readerFaceId );
				}
				reader.Close();
			}
			catch( InvalidCastException e ) {
				Trace.TraceError( "Get Face Ids Invalid Cast Exception : " + e.Message );
				reader?.Close();
			}
			catch( SqlException e ) {
				Trace.TraceError( "Get Face Ids Sql Exception : " + e.Message );
				reader?.Close();
			}
			catch( ObjectDisposedException e ) {
				Trace.TraceError( "Get Face Ids Object Disposed Exception : " + e.Message );
				reader?.Close();
			}
			catch( InvalidOperationException e ) {
				Trace.TraceError( "Get Face Ids Invalid Operation Exception : " + e.Message );
				reader?.Close();
			}
			catch( IOException e ) {
				Trace.TraceError( "Get Face Ids IOException : " + e.Message );
				reader?.Close();
			}
			catch( Exception e ) {
				Trace.TraceError( "Get Face Ids 予期せぬ例外 : " + e.Message );
				reader?.Close();
			}
			
			Trace.TraceInformation( "Get Face Ids End" );

			return ids;

		}

		/// <summary>
		/// 最も笑顔の画像と笑顔度を返す
		/// </summary>
		/// <param name = "alreadyOpenedConnection" > 既にSqlConnection.Open()が呼ばれているコネクション</param>
		/// <param name="isUserId">ユーザIDかどうか</param>
		/// <param name="id">ユーザIDかグループID</param>
		/// <param name="url1">URL</param>
		/// <param name="value1">笑顔度</param>
		/// <param name="url2">URL</param>
		/// <param name="value2">笑顔度</param>
		/// <param name="url3">URL</param>
		/// <param name="value3">笑顔度</param>
		private void GetMostHappiness(
			SqlConnection alreadyOpenedConnection ,
			bool isUserId ,
			string id ,
			ref string url1 ,
			ref double value1 ,
			ref string url2 ,
			ref double value2 ,
			ref string url3 ,
			ref double value3
		) {

			Trace.TraceInformation( "Get Most Happiness Start" );

			Trace.TraceInformation( "Get Most Happiness Is User Id is : " + isUserId );
			Trace.TraceInformation( "Get Most Happiness Id is : " + id );

			string selectSql = @"
				SELECT TOP( 3 ) Face.Happiness as Value , Face.ProcessedPicturePath as Path
				FROM Face
				Where Face.Happiness IN (
					SELECT TOP( 3 ) MAX( Face.Happiness ) as SubValue
					FROM Face
					INNER JOIN Picture
					ON Face.PictureId = Picture.PictureId
					INNER JOIN Line
					ON Picture.LineId = Line.LineId
					WHERE " + this.GetWhereUserIdOrGroupId( isUserId , id ) + @"
					GROUP BY Face.Happiness
					ORDER BY Face.Happiness DESC
				)
				ORDER BY Face.Happiness DESC;";

			Trace.TraceInformation( "Get Most Happiness Sql is : " + selectSql );

			SqlCommand selectSqlCommand = new SqlCommand( selectSql , alreadyOpenedConnection );
			if( isUserId )
				selectSqlCommand.Parameters.AddWithValue( "@UserId" , id );
			else
				selectSqlCommand.Parameters.AddWithValue( "@GroupId" , id );

			Trace.TraceInformation( "SQL Execute" );
			SqlDataReader reader = null;
			try {

				reader = selectSqlCommand.ExecuteReader();

				int index = 0;
				while( reader.Read() == true ) {

					if( index == 0 ) {
						url1 = reader[ "Path" ] as string;
						decimal? value = reader[ "Value" ] as decimal?;
						value1 = value.HasValue ? (double)value.Value : 0.0;
						Trace.TraceInformation( "Url1 is : " + url1 );
						Trace.TraceInformation( "Value1 is : " + value1 );
					}
					else if( index == 1 ) {
						url2 = reader[ "Path" ] as string;
						decimal? value = reader[ "Value" ] as decimal?;
						value2 = value.HasValue ? (double)value.Value : 0.0;
						Trace.TraceInformation( "Url2 is : " + url2 );
						Trace.TraceInformation( "Value2 is : " + value2 );
					}
					else if( index == 2 ) {
						url3 = reader[ "Path" ] as string;
						decimal? value = reader[ "Value" ] as decimal?;
						value3 = value.HasValue ? (double)value.Value : 0.0;
						Trace.TraceInformation( "Url3 is : " + url3 );
						Trace.TraceInformation( "Value3 is : " + value3 );
						break;
					}

					index++;

				}

			}
			catch( InvalidCastException e ) {
				Trace.TraceError( "Get Most Happiness Invalid Cast Exception : " + e.Message );
				reader?.Close();
			}
			catch( SqlException e ) {
				Trace.TraceError( "Get Most Happiness Sql Exception : " + e.Message );
				reader?.Close();
			}
			catch( ObjectDisposedException e ) {
				Trace.TraceError( "Get Most Happiness Object Disposed Exception : " + e.Message );
				reader?.Close();
			}
			catch( InvalidOperationException e ) {
				Trace.TraceError( "Get Most Happiness Invalid Operation Exception : " + e.Message );
				reader?.Close();
			}
			catch( IOException e ) {
				Trace.TraceError( "Get Most Happiness IOException : " + e.Message );
				reader?.Close();
			}
			catch( Exception e ) {
				Trace.TraceError( "Get Most Happiness 予期せぬ例外 : " + e.Message );
				reader?.Close();
			}
			
			Trace.TraceInformation( "Get Most Happiness End" );

		}

		/// <summary>
		/// 最も表情豊かな画像のURLと表情種別と表情度を返す
		/// </summary>
		/// <param name = "alreadyOpenedConnection" > 既にSqlConnection.Open()が呼ばれているコネクション</param>
		/// <param name="isUserId">ユーザIDかどうか</param>
		/// <param name="id">ユーザIDかグループID</param>
		/// <param name="url1">URL</param>
		/// <param name="type1">種別</param>
		/// <param name="value1">笑顔度</param>
		/// <param name="url2">URL</param>
		/// <param name="type2">種別</param>
		/// <param name="value2">笑顔度</param>
		/// <param name="url3">URL</param>
		/// <param name="type3">種別</param>
		/// <param name="value3">笑顔度</param>
		private void GetMostEmotion(
			SqlConnection alreadyOpenedConnection ,
			bool isUserId ,
			string id ,
			ref string url1 ,
			ref CommonEnum.EmotionType type1 ,
			ref double value1 ,
			ref string url2 ,
			ref CommonEnum.EmotionType type2 ,
			ref double value2 ,
			ref string url3 ,
			ref CommonEnum.EmotionType type3 ,
			ref double value3
		) {

			Trace.TraceInformation( "Get Most Emotion Start" );

			Trace.TraceInformation( "Get Most Emotion Is User Id is : " + isUserId );
			Trace.TraceInformation( "Get Most Emotion Id is : " + id );

			string selectSql = @"";
			string mostEmotionSql = @"
				SELECT TOP( 3 ) SubQuery.Path as Path , SubQuery.Value as Value , SubQuery.Type as Type
				FROM(
					( " +　this.GetSubQueryOfMostEmotion( "Anger" , isUserId , id ) + @" )
					UNION ALL
					( " + this.GetSubQueryOfMostEmotion( "Contempt" , isUserId , id ) + @" )
					UNION ALL
					( " + this.GetSubQueryOfMostEmotion( "Disgust" , isUserId , id ) + @" )
					UNION ALL
					( " + this.GetSubQueryOfMostEmotion( "Fear" , isUserId , id ) + @" )
					UNION ALL
					( " + this.GetSubQueryOfMostEmotion( "Happiness" , isUserId , id ) + @" )
					UNION ALL
					( " + this.GetSubQueryOfMostEmotion( "Sadness" , isUserId , id ) + @" )
					UNION ALL
					( " + this.GetSubQueryOfMostEmotion( "Surprise" , isUserId , id ) + @" )
				) SubQuery
				ORDER BY Value DESC
				;";

			Trace.TraceInformation( "Get Most Emotion Sql is : " + selectSql );

			SqlCommand selectSqlCommand = new SqlCommand( selectSql , alreadyOpenedConnection );
			if( isUserId )
				selectSqlCommand.Parameters.AddWithValue( "@UserId" , id );
			else
				selectSqlCommand.Parameters.AddWithValue( "@GroupId" , id );

			Trace.TraceInformation( "SQL Execute" );
			SqlDataReader reader = null;
			try {

				reader = selectSqlCommand.ExecuteReader();

				int index = 0;
				while( reader.Read() == true ) {

					if( index == 0 ) {
						url1 = reader[ "Path" ] as string;
						decimal? value = reader[ "Value" ] as decimal?;
						value1 = value.HasValue ? (double)value.Value : 0.0;
						type1 = CommonUtil.ConvertEmotionStringIntoType( reader[ "Type" ] as string );
						Trace.TraceInformation( "Url1 is : " + url1 );
						Trace.TraceInformation( "Value1 is : " + value1 );
						Trace.TraceInformation( "Type1 is : " + type1 );
					}
					else if( index == 1 ) {
						url2 = reader[ "Path" ] as string;
						decimal? value = reader[ "Value" ] as decimal?;
						value2 = value.HasValue ? (double)value.Value : 0.0;
						type2 = CommonUtil.ConvertEmotionStringIntoType( reader[ "Type" ] as string );
						Trace.TraceInformation( "Url1 is : " + url2 );
						Trace.TraceInformation( "Value1 is : " + value2 );
						Trace.TraceInformation( "Type1 is : " + type2 );
					}
					else if( index == 2 ) {
						url3 = reader[ "Path" ] as string;
						decimal? value = reader[ "Value" ] as decimal?;
						value3 = value.HasValue ? (double)value.Value : 0.0;
						type3 = CommonUtil.ConvertEmotionStringIntoType( reader[ "Type" ] as string );
						Trace.TraceInformation( "Url1 is : " + url3 );
						Trace.TraceInformation( "Value1 is : " + value3 );
						Trace.TraceInformation( "Type1 is : " + type3 );
						break;
					}

					index++;

				}

			}
			catch( InvalidCastException e ) {
				Trace.TraceError( "Get Most Emotion Invalid Cast Exception : " + e.Message );
				reader?.Close();
			}
			catch( SqlException e ) {
				Trace.TraceError( "Get Most Emotion Sql Exception : " + e.Message );
				reader?.Close();
			}
			catch( ObjectDisposedException e ) {
				Trace.TraceError( "Get Most Emotion Object Disposed Exception : " + e.Message );
				reader?.Close();
			}
			catch( InvalidOperationException e ) {
				Trace.TraceError( "Get Most Emotion Invalid Operation Exception : " + e.Message );
				reader?.Close();
			}
			catch( IOException e ) {
				Trace.TraceError( "Get Most Emotion IOException : " + e.Message );
				reader?.Close();
			}
			catch( Exception e ) {
				Trace.TraceError( "Get Most Emotion 予期せぬ例外 : " + e.Message );
				reader?.Close();
			}

			Trace.TraceInformation( "Get Most Emotion End" );

		}

		/// <summary>
		/// LINE情報の登録
		/// </summary>
		/// <param name="id">ユーザIDもしくはグループID</param>
		/// <param name="isUserId">ユーザIDかどうか</param>
		/// <returns>LINEID</returns>
		public void RegistLine( string id , bool isUserId ) {

			Trace.TraceInformation( "Regist Line Start" );
			
			Trace.TraceInformation( "Id is : " + id );
			Trace.TraceInformation( "Is User Id : " + isUserId );
			
			SqlConnection connection = null;
			try {

				connection = new SqlConnection( this.connectionString );
				connection.Open();
				Trace.TraceInformation( "Connection Open" );

				int maxLineId = this.GetMaxLineId( connection );

				this.InsertLine( connection , maxLineId + 1 , isUserId , id );

				connection.Close();
				Trace.TraceInformation( "Connection Close" );

			}
			catch( InvalidCastException e ) {
				Trace.TraceError( "Regist Line Invalid Cast Exception : " + e.Message );
				connection?.Close();
				Trace.TraceInformation( "Connection Close" );
			}
			catch( SqlException e ) {
				Trace.TraceError( "Regist Line Sql Exception : " + e.Message );
				connection?.Close();
				Trace.TraceInformation( "Connection Close" );
			}
			catch( ConfigurationErrorsException e ) {
				Trace.TraceError( "Regist Line Configuration Errors Exception : " + e.Message );
				connection?.Close();
				Trace.TraceInformation( "Connection Close" );
			}
			catch( Exception e ) {
				Trace.TraceInformation( "Regist Line 予期せぬ例外 : " + e.Message );
				connection?.Close();
				Trace.TraceInformation( "Connection Close" );
			}

			Trace.TraceInformation( "Regist Line End" );

		}

		/// <summary>
		/// LINE情報の削除
		/// </summary>
		/// <param name="id">ユーザIDもしくはグループID</param>
		/// <param name="isUserId">ユーザIDかどうか</param>
		/// <returns>LINEID</returns>
		public void LeaveLine( string id , bool isUserId ) {

			Trace.TraceInformation( "Leave Line Start" );

			Trace.TraceInformation( "Leave Line End" );

		}

		/// <summary>
		/// Postbackが初期状態かどうか判断
		/// </summary>
		/// <param name="isUserId">idがユーザIDかどうか</param>
		/// <param name="id">ユーザIDまたはグループID</param>
		/// <returns>初期化状態かどうか</returns>
		public bool IsPostbackInitialization( bool isUserId , string id ) {

			Trace.TraceInformation( "Is Postback Initialization Start" );

			Trace.TraceInformation( "Is Postback Initialization Is User Id : " + isUserId );
			Trace.TraceInformation( "Is Postback Initialization Id is : " + id );

			bool isInitialization = true;
			SqlConnection connection = null;
			try {

				connection = new SqlConnection( this.connectionString );
				connection.Open();
				Trace.TraceInformation( "Connection Open" );

				isInitialization = this.GetPostback( connection , isUserId , id );

				connection.Close();
				Trace.TraceInformation( "Connection Close" );

			}
			catch( InvalidCastException e ) {
				Trace.TraceError( "Is Postback Initialization Invalid Cast Exception : " + e.Message );
				connection?.Close();
				Trace.TraceInformation( "Connection Close" );
			}
			catch( SqlException e ) {
				Trace.TraceError( "Is Postback Initialization Sql Exception : " + e.Message );
				connection?.Close();
				Trace.TraceInformation( "Connection Close" );
			}
			catch( ConfigurationErrorsException e ) {
				Trace.TraceError( "Is Postback Initialization Configuration Errors Exception : " + e.Message );
				connection?.Close();
				Trace.TraceInformation( "Connection Close" );
			}
			catch( Exception e ) {
				Trace.TraceInformation( "Is Postback Initialization 予期せぬ例外 : " + e.Message );
				connection?.Close();
				Trace.TraceInformation( "Connection Close" );
			}
			
			return isInitialization;

		}
		
		/// <summary>
		/// Postbackを更新する
		/// </summary>
		/// <param name="userId">ユーザIDかどうか</param>
		/// <param name="groupId">ユーザIDまたはグループID</param>
		/// <param name="isInitialization">初期化するかどうか</param>
		public void UpdatePostback( bool isUserId , string id , bool isInitialization ) {
			
			Trace.TraceInformation( "Update Postback Start" );

			Trace.TraceInformation( "Update Postback Is User Id : " + isUserId );
			Trace.TraceInformation( "Update Postback Id is : " + id );
			Trace.TraceInformation( "Update Postback Is Initialization is : " + isInitialization );

			SqlConnection connection = null;
			try {

				connection = new SqlConnection( this.connectionString );
				connection.Open();
				Trace.TraceInformation( "Connection Open" );

				this.UpdatePostback( connection , isUserId , id , isInitialization );

				connection.Close();
				Trace.TraceInformation( "Connection Close" );
				
			}
			catch( InvalidCastException e ) {
				Trace.TraceError( "Update Postback Invalid Cast Exception : " + e.Message );
				connection?.Close();
				Trace.TraceInformation( "Connection Close" );
			}
			catch( SqlException e ) {
				Trace.TraceError( "Update Postback Sql Exception : " + e.Message );
				connection?.Close();
				Trace.TraceInformation( "Connection Close" );
			}
			catch( ConfigurationErrorsException e ) {
				Trace.TraceError( "Update Postback Configuration Errors Exception : " + e.Message );
				connection?.Close();
				Trace.TraceInformation( "Connection Close" );
			}
			catch( Exception e ) {
				Trace.TraceInformation( "Update Postback 予期せぬ例外 : " + e.Message );
				connection?.Close();
				Trace.TraceInformation( "Connection Close" );
			}

			Trace.TraceInformation( "Update Postback End" );

		}

		/// <summary>
		/// 画像の登録
		/// </summary>
		/// <param name="userId">ユーザIDかどうか</param>
		/// <param name="groupId">ユーザIDまたはグループID</param>
		/// <param name="path">画像URL</param>
		/// <returns>管理番号</returns>
		public int RegistPicture( bool isUserId , string id , string path ) {

			Trace.TraceInformation( "Regist Picture Start" );

			Trace.TraceInformation( "Regist Picture Is User Id : " + isUserId );
			Trace.TraceInformation( "Regist Picture Id is : " + id );
			Trace.TraceInformation( "Regist Picture Path is : " + path );

			SqlConnection connection = null;
			int pictureId = -1;
			try {

				connection = new SqlConnection( this.connectionString );
				connection.Open();
				Trace.TraceInformation( "Connection Open" );

				int lineId = this.GetLineId( connection , isUserId , id );

				pictureId = this.GetMaxLineId( connection ) + 1;

				this.InsertPicture( connection , pictureId , path , lineId );

				connection.Close();
				Trace.TraceInformation( "Connection Close" );

			}
			catch( InvalidCastException e ) {
				Trace.TraceError( "Regist Picture Invalid Cast Exception : " + e.Message );
				connection?.Close();
				Trace.TraceInformation( "Connection Close" );
			}
			catch( SqlException e ) {
				Trace.TraceError( "Regist Picture Sql Exception : " + e.Message );
				connection?.Close();
				Trace.TraceInformation( "Connection Close" );
			}
			catch( ConfigurationErrorsException e ) {
				Trace.TraceError( "Regist Picture Configuration Errors Exception : " + e.Message );
				connection?.Close();
				Trace.TraceInformation( "Connection Close" );
			}
			catch( Exception e ) {
				Trace.TraceInformation( "Regist Picture 予期せぬ例外 : " + e.Message );
				connection?.Close();
				Trace.TraceInformation( "Connection Close" );
			}

			Trace.TraceInformation( "Picture Id is : " + pictureId );

			Trace.TraceInformation( "Regist Picture End" );

			return pictureId;

		}

		/// <summary>
		/// 表情の登録
		/// </summary>
		/// <param name="pictureId">写真管理番号</param>
		/// <param name="response">表情情報</param>
		/// <param name="path">URL</param>
		/// <param name="faceId">Face API - Detectで取得するFaceId</param>
		public void RegistFace( int pictureId , ResponseOfEmotionRecognitionAPI response , string path , string faceId ) {

			Trace.TraceInformation( "Regist Face Start" );

			Trace.TraceInformation( "Regist Face Picture Id is : " + pictureId );
			Trace.TraceInformation( "Regist Face Path is : " + path );
			Trace.TraceInformation( "Regist Face Id is : " + faceId );

			SqlConnection connection = null;
			try {

				connection = new SqlConnection( this.connectionString );
				connection.Open();
				Trace.TraceInformation( "Connection Open" );

				int id = this.GetMaxFaceId( connection ) + 1;

				this.InsertFace( connection , id , faceId , response , pictureId , path );

				connection.Close();
				Trace.TraceInformation( "Connection Close" );

			}
			catch( InvalidCastException e ) {
				Trace.TraceError( "Regist Face Invalid Cast Exception : " + e.Message );
				connection?.Close();
				Trace.TraceInformation( "Connection Close" );
			}
			catch( SqlException e ) {
				Trace.TraceError( "Regist Face Sql Exception : " + e.Message );
				connection?.Close();
				Trace.TraceInformation( "Connection Close" );
			}
			catch( ConfigurationErrorsException e ) {
				Trace.TraceError( "Regist Face Configuration Errors Exception : " + e.Message );
				connection?.Close();
				Trace.TraceInformation( "Connection Close" );
			}
			catch( Exception e ) {
				Trace.TraceInformation( "Regist Face 予期せぬ例外 : " + e.Message );
				connection?.Close();
				Trace.TraceInformation( "Connection Close" );
			}

			Trace.TraceInformation( "Regist Face End" );

		}
		
		/// <summary>
		/// 顔IDを取得する
		/// </summary>
		/// <param name="isUserId">ユーザIDかどうか</param>
		/// <param name="id">ユーザIDまたはグループID</param>
		/// <returns>顔IDリスト</returns>
		public List<string> GetFaceIds( bool isUserId , string id ) {

			Trace.TraceInformation( "Get Face Ids Start" );

			Trace.TraceInformation( "Get Face Ids Is User Id is : " + isUserId );
			Trace.TraceInformation( "Get Face Ids Id is : " + id );

			List<string> ids = null;
			SqlConnection connection = null;
			try {

				connection = new SqlConnection( this.connectionString );
				connection.Open();
				Trace.TraceInformation( "Connection Open" );

				ids = this.GetFaceIds( connection , isUserId , id );

				connection.Close();
				Trace.TraceInformation( "Connection Close" );

			}
			catch( InvalidCastException e ) {
				Trace.TraceError( "Get Most Emotion Invalid Cast Exception : " + e.Message );
				connection?.Close();
				Trace.TraceInformation( "Connection Close" );
			}
			catch( SqlException e ) {
				Trace.TraceError( "Get Most Emotion Sql Exception : " + e.Message );
				connection?.Close();
				Trace.TraceInformation( "Connection Close" );
			}
			catch( ConfigurationErrorsException e ) {
				Trace.TraceError( "Get Most Emotion Configuration Errors Exception : " + e.Message );
				connection?.Close();
				Trace.TraceInformation( "Connection Close" );
			}
			catch( Exception e ) {
				Trace.TraceInformation( "Get Most Emotion 予期せぬ例外 : " + e.Message );
				connection?.Close();
				Trace.TraceInformation( "Connection Close" );
			}
			
			Trace.TraceInformation( "Get Face Ids End" );

			return null;

		}

		/// <summary>
		/// 最も笑顔の画像と笑顔度を返す
		/// </summary>
		/// <param name="isUserId">ユーザIDかどうか</param>
		/// <param name="id">ユーザIDまたはグループID</param>
		/// <param name="url1">URL</param>
		/// <param name="value1">笑顔度</param>
		/// <param name="url2">URL</param>
		/// <param name="value2">笑顔度</param>
		/// <param name="url3">URL</param>
		/// <param name="value3">笑顔度</param>
		public void GetMostHappiness(
			bool isUserId ,
			string id ,
			ref string url1 ,
			ref double value1 ,
			ref string url2 ,
			ref double value2 ,
			ref string url3 ,
			ref double value3
		) {

			Trace.TraceInformation( "Get Most Happiness Start" );

			Trace.TraceInformation( "Get Most Happiness Is User Id is : " + isUserId );
			Trace.TraceInformation( "Get Most Happiness Id is : " + id );
			
			SqlConnection connection = null;
			try {

				connection = new SqlConnection( this.connectionString );
				connection.Open();
				Trace.TraceInformation( "Connection Open" );
				
				this.GetMostHappiness( connection , isUserId , id , ref url1 , ref value1 , ref url2 , ref value2 , ref url3 , ref value3 );
				
				connection.Close();
				Trace.TraceInformation( "Connection Close" );

			}
			catch( InvalidCastException e ) {
				Trace.TraceError( "Get Most Happiness Invalid Cast Exception : " + e.Message );
				connection?.Close();
				Trace.TraceInformation( "Connection Close" );
			}
			catch( SqlException e ) {
				Trace.TraceError( "Get Most Happiness Sql Exception : " + e.Message );
				connection?.Close();
				Trace.TraceInformation( "Connection Close" );
			}
			catch( ConfigurationErrorsException e ) {
				Trace.TraceError( "Get Most Happiness Configuration Errors Exception : " + e.Message );
				connection?.Close();
				Trace.TraceInformation( "Connection Close" );
			}
			catch( Exception e ) {
				Trace.TraceInformation( "Get Most Happiness 予期せぬ例外 : " + e.Message );
				connection?.Close();
				Trace.TraceInformation( "Connection Close" );
			}

			Trace.TraceInformation( "Get Most Happiness End" );

		}

		/// <summary>
		/// 最も表情豊かな画像と表情種別と表情度を返す
		/// </summary>
		/// <param name="isUserId">ユーザIDかどうか</param>
		/// <param name="id">ユーザIDまたはグループID</param>
		/// <param name="url1">URL</param>
		/// <param name="type1">表情種別</param>
		/// <param name="value1">表情度</param>
		/// <param name="url2">URL</param>
		/// <param name="type2">表情種別</param>
		/// <param name="value2">表情度</param>
		/// <param name="url3">URL</param>
		/// <param name="type3">表情種別</param>
		/// <param name="value3">表情度</param>
		public void GetMostEmotion(
			bool isUserId ,
			string id ,
			ref string url1 ,
			ref CommonEnum.EmotionType type1 ,
			ref double value1 ,
			ref string url2 ,
			ref CommonEnum.EmotionType type2 ,
			ref double value2 ,
			ref string url3 ,
			ref CommonEnum.EmotionType type3 ,
			ref double value3
		) {

			Trace.TraceInformation( "Get Most Emotion Start" );

			Trace.TraceInformation( "Get Most Emotion Is User Id is : " + isUserId );
			Trace.TraceInformation( "Get Most Emotion Id is : " + id );
			
			SqlConnection connection = null;
			try {

				connection = new SqlConnection( this.connectionString );
				connection.Open();
				Trace.TraceInformation( "Connection Open" );

				this.GetMostEmotion( connection , isUserId , id , ref url1 , ref type1 , ref value1 , ref url2 , ref type2 , ref value2 , ref url3 , ref type3 , ref value3 );

				connection.Close();
				Trace.TraceInformation( "Connection Close" );

			}
			catch( InvalidCastException e ) {
				Trace.TraceError( "Get Most Emotion Invalid Cast Exception : " + e.Message );
				connection?.Close();
				Trace.TraceInformation( "Connection Close" );
			}
			catch( SqlException e ) {
				Trace.TraceError( "Get Most Emotion Sql Exception : " + e.Message );
				connection?.Close();
				Trace.TraceInformation( "Connection Close" );
			}
			catch( ConfigurationErrorsException e ) {
				Trace.TraceError( "Get Most Emotion Configuration Errors Exception : " + e.Message );
				connection?.Close();
				Trace.TraceInformation( "Connection Close" );
			}
			catch( Exception e ) {
				Trace.TraceInformation( "Get Most Emotion 予期せぬ例外 : " + e.Message );
				connection?.Close();
				Trace.TraceInformation( "Connection Close" );
			}

			Trace.TraceInformation( "Get Most Emotion End" );
			

		}
		
	}

}