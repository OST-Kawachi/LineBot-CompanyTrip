using System.Collections.Generic;
using System.Diagnostics;
using LineBotCompanyTrip.Common;
using LineBotCompanyTrip.Models.AzureCognitiveServices.EmotionAPI;
using System.Data.SqlClient;
using LineBotCompanyTrip.Configurations;
using System;

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
			
		}

		/// <summary>
		/// LINE情報の登録
		/// </summary>
		/// <param name="id">ユーザIDもしくはグループID</param>
		/// <param name="isUserId">ユーザIDかどうか</param>
		/// <returns>LINEID</returns>
		public void RegistLineInfo( string id , bool isUserId ) {

			Trace.TraceInformation( "LINE情報の登録" );

			SqlConnection connection = null;
			try {

				//SQL発行
				{
					connection = new SqlConnection( this.connectionString );
					connection.Open();
					Trace.TraceInformation( "Connection Open" );

					//IDの最大値を取得する
					int maxLineId = -1;
					{

						string selectSql =
							@"SELECT TOP( 1 ) Line.LineId as LineId " +
							"FROM Line " +
							"ORDER BY Line.LineId DESC " +
							";";
						Trace.TraceInformation( "Max Line Id SQL is : " + selectSql );

						SqlCommand selectSqlCommand = new SqlCommand( selectSql , connection );

						Trace.TraceInformation( "SQL Execute" );
						using( SqlDataReader reader = selectSqlCommand.ExecuteReader() ) {
							while( reader.Read() == true ) {
								int? readerLineId = reader[ "LineId" ] as int?;
								maxLineId = readerLineId.HasValue ? readerLineId.Value : -1;
								break;
							}
						}

						Trace.TraceInformation( "Max Line Id is : " + maxLineId );

					}

					//Line情報の登録
					{

						int lineId = maxLineId + 1;

						string insertSql =
							@"INSERT INTO Line ( LineId , Type , UserId , GroupId , PostbackStatus ) VALUES ( @LineId , @Type , @UserId , @GroupId , @PostbackStatus );";

						Trace.TraceInformation( "Regist Line Info SQL is : " + insertSql );

						SqlCommand insertSqlCommand = new SqlCommand( insertSql , connection );
						insertSqlCommand.Parameters.AddWithValue( "@LineId" , lineId );
						insertSqlCommand.Parameters.AddWithValue( "@Type" , ( isUserId ? 0 : 1 ) );
						insertSqlCommand.Parameters.AddWithValue( "@UserId" , ( isUserId ? id : "" ) );
						insertSqlCommand.Parameters.AddWithValue( "@GroupId" , ( isUserId ? "" : id ) );
						insertSqlCommand.Parameters.AddWithValue( "@PostbackStatus" , 0 );
						
						int result = insertSqlCommand.ExecuteNonQuery();

						if( result < 1 )
							Trace.TraceInformation( "Insert Error" );

					}

					connection.Close();

				}

			}
			catch( Exception e ) {
				connection?.Close();
				Trace.TraceInformation( "DB Error : " + e.Message );
			}

		}

		/// <summary>
		/// LINE情報の削除
		/// </summary>
		/// <param name="id">ユーザIDもしくはグループID</param>
		/// <param name="isUserId">ユーザIDかどうか</param>
		/// <returns>LINEID</returns>
		public void LeaveLineInfo( string id , bool isUserId ) {

			Trace.TraceInformation( "LINE情報の削除" );

			Trace.TraceInformation( "今回は削除しない" );

		}

		/// <summary>
		/// Postbackが初期状態かどうか判断
		/// </summary>
		/// <param name="userId">ユーザID</param>
		/// <param name="groupId">グループID</param>
		/// <returns>初期化状態かどうか</returns>
		public bool IsPostbackInitialization( string userId , string groupId ) {

			Trace.TraceInformation( "Postbackの判断" );

			// 初期状態かどうか
			bool isInitialization = true;
			{

				SqlConnection connection = null;
				try {

					connection = new SqlConnection( this.connectionString );
					connection.Open();
					Trace.TraceInformation( "Connection Open" );

					string selectSql =
						"SELECT Line.PostbackStatus as Status " +
						"FROM Line " +
						"WHERE " + ( !string.IsNullOrEmpty( userId ) ? "CONVERT( VARCHAR , Line.UserId ) = CONVERT( VARCHAR , @UserId ) " : "CONVERT( VARCHAR , Line.GroupId ) = CONVERT( VARCHAR , @GroupId ) " ) +
						";";
					Trace.TraceInformation( "Get Postback SQL is : " + selectSql );

					SqlCommand selectSqlCommand = new SqlCommand( selectSql , connection );
					if( !string.IsNullOrEmpty( userId ) )
						selectSqlCommand.Parameters.AddWithValue( "@UserId" , userId );
					else
						selectSqlCommand.Parameters.AddWithValue( "@GroupId" , groupId );

					Trace.TraceInformation( "SQL Execute" );
					using( SqlDataReader reader = selectSqlCommand.ExecuteReader() ) {
						while( reader.Read() == true ) {
							int? status = reader[ "Status" ] as int?;
							isInitialization = status.HasValue ? status.Value == 0 : true;
							break;
						}
					}

					Trace.TraceInformation( "Initialization is : " + isInitialization );
					
					connection.Close();
					
				}
				catch( Exception e ) {
					connection?.Close();
					Trace.TraceInformation( "DB Error : " + e.Message );
				}
				
			}
			
			return isInitialization;

		}

		/// <summary>
		/// Postbackを更新する
		/// </summary>
		/// <param name="userId">ユーザID</param>
		/// <param name="groupId">グループID</param>
		/// <param name="isInitialization">初期化するかどうか</param>
		public void UpdatePostback( string userId , string groupId , bool isInitialization ) {

			Trace.TraceInformation( "Postbackの更新" );
			
			SqlConnection connection = null;
			try {

				connection = new SqlConnection( this.connectionString );
				connection.Open();
				Trace.TraceInformation( "Connection Open" );

				string updateSql =
					"UPDATE Line " +
					"SET Line.PostbackStatus = @Status " +
					"WHERE " + ( !string.IsNullOrEmpty( userId ) ? "CONVERT( VARCHAR , Line.UserId ) = CONVERT( VARCHAR , @UserId ) " : "CONVERT( VARCHAR , Line.GroupId ) = CONVERT( VARCHAR , @GroupId ) " ) +
					";";
				Trace.TraceInformation( "Update Postback SQL is : " + updateSql );
				
				SqlCommand selectSqlCommand = new SqlCommand( updateSql , connection );
				selectSqlCommand.Parameters.AddWithValue( "@Status" , isInitialization ? 0 : 1 );
				if( !string.IsNullOrEmpty( userId ) ) {
					selectSqlCommand.Parameters.AddWithValue( "@UserId" , userId );
					Trace.TraceInformation( "UserId is : " + userId );
				}
				else {
					selectSqlCommand.Parameters.AddWithValue( "@GroupId" , groupId );
					Trace.TraceInformation( "GroupId is : " + groupId );
				}
				Trace.TraceInformation( "SQL Execute" );
				int resultRowCount = selectSqlCommand.ExecuteNonQuery();

				Trace.TraceInformation( "Update Count is : " + resultRowCount );

				connection.Close();

			}
			catch( Exception e ) {
				connection?.Close();
				Trace.TraceInformation( "DB Error : " + e.Message );
			}

		}
		
		/// <summary>
		/// 最もよく撮られた人の画像と回数を返す
		/// </summary>
		/// <param name="userId">ユーザID</param>
		/// <param name="groupId">グループID</param>
		/// <param name="url1">URL</param>
		/// <param name="count1">回数</param>
		/// <param name="url2">URL</param>
		/// <param name="count2">回数</param>
		/// <param name="url3">URL</param>
		/// <param name="count3">回数</param>
		public void GetMostPhotographed(
			string userId ,
			string groupId ,
			ref string url1 ,
			ref int count1 ,
			ref string url2 ,
			ref int count2 ,
			ref string url3 ,
			ref int count3
		) {

			Trace.TraceInformation( "最もよく撮られた人取得" );

			SqlConnection connection = null;
			try {

				// TODO Face.Idだと一人ずつしか取得できない
				// Face APIを使ってグルーピングしてそれに当てはまる人を数えないと意味ない

				connection = new SqlConnection( this.connectionString );
				connection.Open();
				Trace.TraceInformation( "Connection Open" );

				string mostPhotographedSql =
					"SELECT TOP( 3 ) COUNT( Face.Id ) as Count , Face.ProcessedPicturePath as Path " +
					"FROM Face " +
					"INNER JOIN Picture " +
					"ON Face.PictureId = Picture.PictureId " +
					"INNER JOIN Line " +
					"ON Picture.LineId = Line.LineId " +
					"WHERE " + ( !string.IsNullOrEmpty( userId ) ? "CONVERT( VARCHAR , Line.UserId ) = CONVERT( VARCHAR , @UserId ) " : "CONVERT( VARCHAR , Line.GroupId ) = CONVERT( VARCHAR , @GroupId ) " ) +
					"GROUP BY Face.Id , Face.ProcessedPicturePath " +
					";";
				Trace.TraceInformation( "Most Photographed Sql is : " + mostPhotographedSql );

				SqlCommand selectSqlCommand = new SqlCommand( mostPhotographedSql , connection );
				if( !string.IsNullOrEmpty( userId ) ) {
					selectSqlCommand.Parameters.AddWithValue( "@UserId" , userId );
					Trace.TraceInformation( "User Id is : " + userId );
				}
				else {
					selectSqlCommand.Parameters.AddWithValue( "@GroupId" , groupId );
					Trace.TraceInformation( "Group Id is : " + groupId );
				}

				Trace.TraceInformation( "SQL Execute" );
				using( SqlDataReader reader = selectSqlCommand.ExecuteReader() ) {
					int index = 0;
					while( reader.Read() == true ) {

						if( index == 0 ) {
							url1 = reader[ "Path" ] as string;
							int? count = reader[ "Count" ] as int?;
							count1 = count.HasValue ? count.Value : 0;
						}
						else if( index == 1 ) {
							url2 = reader[ "Path" ] as string;
							int? count = reader[ "Count" ] as int?;
							count2 = count.HasValue ? count.Value : 0;
						}
						else if( index == 2 ) {
							url3 = reader[ "Path" ] as string;
							int? count = reader[ "Count" ] as int?;
							count3 = count.HasValue ? count.Value : 0;
							break;
						}

						index++;

					}

				}

				Trace.TraceInformation( "Url1 is : " + url1 );
				Trace.TraceInformation( "Url2 is : " + url2 );
				Trace.TraceInformation( "Url3 is : " + url3 );
				Trace.TraceInformation( "Count1 is : " + count1 );
				Trace.TraceInformation( "Count2 is : " + count2 );
				Trace.TraceInformation( "Count3 is : " + count3 );

				connection.Close();

			}
			catch( Exception e ) {
				connection?.Close();
				Trace.TraceInformation( "DB Error : " + e.Message );
			}
			
		}

		/// <summary>
		/// 最も笑顔の画像と笑顔度を返す
		/// </summary>
		/// <param name="userId">ユーザID</param>
		/// <param name="groupId">グループID</param>
		/// <param name="url1">URL</param>
		/// <param name="value1">笑顔度</param>
		/// <param name="url2">URL</param>
		/// <param name="value2">笑顔度</param>
		/// <param name="url3">URL</param>
		/// <param name="value3">笑顔度</param>
		public void GetMostHappiness(
			string userId ,
			string groupId ,
			ref string url1 ,
			ref double value1 ,
			ref string url2 ,
			ref double value2 ,
			ref string url3 ,
			ref double value3
		) {

			Trace.TraceInformation( "最も笑顔の人取得" );
			
			SqlConnection connection = null;
			try {

				connection = new SqlConnection( this.connectionString );
				connection.Open();
				Trace.TraceInformation( "Connection Open" );

				string mostHappinessSql =
					"SELECT TOP( 3 ) Face.Happiness as Value , Face.ProcessedPicturePath as Path "+
					"FROM Face " +
					"Where Face.Happiness IN " +
					"( " +
					"	SELECT TOP( 3 ) MAX( Face.Happiness ) as SubValue " +
					"	FROM Face " +
					"	INNER JOIN Picture " +
					"	ON Face.PictureId = Picture.PictureId " +
					"	INNER JOIN Line " +
					"	ON Picture.LineId = Line.LineId " +
					"	WHERE " + ( !string.IsNullOrEmpty( userId ) ? "CONVERT( VARCHAR , Line.UserId ) = CONVERT( VARCHAR , @UserId ) " : "CONVERT( VARCHAR , Line.GroupId ) = CONVERT( VARCHAR , @GroupId ) " ) +
					"	GROUP BY Face.Happiness " +
					"	ORDER BY Face.Happiness DESC " +
					") " +
					"ORDER BY Face.Happiness DESC; ";
				
				Trace.TraceInformation( "Most Photographed Sql is : " + mostHappinessSql );

				SqlCommand selectSqlCommand = new SqlCommand( mostHappinessSql , connection );
				if( !string.IsNullOrEmpty( userId ) ) {
					selectSqlCommand.Parameters.AddWithValue( "@UserId" , userId );
					Trace.TraceInformation( "User Id is : " + userId );
				}
				else {
					selectSqlCommand.Parameters.AddWithValue( "@GroupId" , groupId );
					Trace.TraceInformation( "Group Id is : " + groupId );
				}

				Trace.TraceInformation( "SQL Execute" );
				using( SqlDataReader reader = selectSqlCommand.ExecuteReader() ) {
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
				
				connection.Close();

			}
			catch( Exception e ) {
				connection?.Close();
				Trace.TraceInformation( "DB Error : " + e.Message );
			}

		}

		/// <summary>
		/// 最も表情豊かな画像と表情種別と表情度を返す
		/// </summary>
		/// <param name="userId">ユーザID</param>
		/// <param name="groupId">グループID</param>
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
			string userId ,
			string groupId ,
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

			Trace.TraceInformation( "最も表情豊かな人取得" );

			SqlConnection connection = null;
			try {

				connection = new SqlConnection( this.connectionString );
				connection.Open();
				Trace.TraceInformation( "Connection Open" );

				string mostEmotionSql =
					@"SELECT TOP( 3 ) SubQuery.Path as Path , SubQuery.Value as Value , SubQuery.Type as Type " +
					"FROM( " +
					"	( " +
					"		SELECT TOP( 3 ) Face.ProcessedPicturePath as Path , Face.Anger as Value , 'Anger' as Type " +
					"		FROM Face " +
					"		WHERE Face.Anger IN( " +
					"			SELECT TOP( 3 ) MAX( Face.Anger ) as Value " +
					"			FROM Face " +
					"			INNER JOIN Picture " +
					"			ON Face.PictureId = Picture.PictureId " +
					"			INNER JOIN Line " +
					"			ON Picture.LineId = Line.LineId " +
					"			WHERE " + ( !string.IsNullOrEmpty( userId ) ? "CONVERT( VARCHAR , Line.UserId ) = CONVERT( VARCHAR , @UserId ) " : "CONVERT( VARCHAR , Line.GroupId ) = CONVERT( VARCHAR , @GroupId ) " ) +
					"			GROUP BY Face.Anger " +
					"			ORDER BY Face.Anger DESC " +
					"		) " +
					"		ORDER BY Face.Anger DESC " +
					"	) " +
					"	UNION ALL " +
					"	( " +
					"		SELECT TOP( 3 ) Face.ProcessedPicturePath as Path , Face.Contempt as Value , 'Contempt' as Type " +
					"		FROM Face " +
					"		WHERE Face.Contempt IN( " +
					"			SELECT TOP( 3 ) MAX( Face.Contempt ) as Value " +
					"			FROM Face " +
					"			INNER JOIN Picture " +
					"			ON Face.PictureId = Picture.PictureId " +
					"			INNER JOIN Line " +
					"			ON Picture.LineId = Line.LineId " +
					"			WHERE " + ( !string.IsNullOrEmpty( userId ) ? "CONVERT( VARCHAR , Line.UserId ) = CONVERT( VARCHAR , @UserId ) " : "CONVERT( VARCHAR , Line.GroupId ) = CONVERT( VARCHAR , @GroupId ) " ) +
					"			GROUP BY Face.Contempt " +
					"			ORDER BY Face.Contempt DESC " +
					"		) " +
					"		ORDER BY Face.Contempt DESC " +
					"	) " +
					"	UNION ALL " +
					"	( " +
					"		SELECT TOP( 3 ) Face.ProcessedPicturePath as Path , Face.Disgust as Value , 'Disgust' as Type " +
					"		FROM Face " +
					"		WHERE Face.Disgust IN( " +
					"			SELECT TOP( 3 ) MAX( Face.Disgust ) as Value " +
					"			FROM Face " +
					"			INNER JOIN Picture " +
					"			ON Face.PictureId = Picture.PictureId " +
					"			INNER JOIN Line " +
					"			ON Picture.LineId = Line.LineId " +
					"			WHERE " + ( !string.IsNullOrEmpty( userId ) ? "CONVERT( VARCHAR , Line.UserId ) = CONVERT( VARCHAR , @UserId ) " : "CONVERT( VARCHAR , Line.GroupId ) = CONVERT( VARCHAR , @GroupId ) " ) +
					"			GROUP BY Face.Disgust " +
					"			ORDER BY Face.Disgust DESC " +
					"		) " +
					"		ORDER BY Face.Disgust DESC " +
					"	) " +
					"	UNION ALL " +
					"	( " +
					"		SELECT TOP( 3 ) Face.ProcessedPicturePath as Path , Face.Fear as Value , 'Fear' as Type " +
					"		FROM Face " +
					"		WHERE Face.Fear IN( " +
					"			SELECT TOP( 3 ) MAX( Face.Fear ) as Value " +
					"			FROM Face " +
					"			INNER JOIN Picture " +
					"			ON Face.PictureId = Picture.PictureId " +
					"			INNER JOIN Line " +
					"			ON Picture.LineId = Line.LineId " +
					"			WHERE " + ( !string.IsNullOrEmpty( userId ) ? "CONVERT( VARCHAR , Line.UserId ) = CONVERT( VARCHAR , @UserId ) " : "CONVERT( VARCHAR , Line.GroupId ) = CONVERT( VARCHAR , @GroupId ) " ) +
					"			GROUP BY Face.Fear " +
					"			ORDER BY Face.Fear DESC " +
					"		) " +
					"		ORDER BY Face.Fear DESC " +
					"	) " +
					"	UNION ALL " +
					"	( " +
					"		SELECT TOP( 3 ) Face.ProcessedPicturePath as Path , Face.Happiness as Value , 'Happiness' as Type " +
					"		FROM Face " +
					"		WHERE Face.Happiness IN( " +
					"			SELECT TOP( 3 ) MAX( Face.Happiness ) as Value " +
					"			FROM Face " +
					"			INNER JOIN Picture " +
					"			ON Face.PictureId = Picture.PictureId " +
					"			INNER JOIN Line " +
					"			ON Picture.LineId = Line.LineId " +
					"			WHERE " + ( !string.IsNullOrEmpty( userId ) ? "CONVERT( VARCHAR , Line.UserId ) = CONVERT( VARCHAR , @UserId ) " : "CONVERT( VARCHAR , Line.GroupId ) = CONVERT( VARCHAR , @GroupId ) " ) +
					"			GROUP BY Face.Happiness " +
					"			ORDER BY Face.Happiness DESC " +
					"		) " +
					"		ORDER BY Face.Happiness DESC " +
					"	) " +
					"	UNION ALL " +
					"	( " +
					"		SELECT TOP( 3 ) Face.ProcessedPicturePath as Path , Face.Sadness as Value , 'Sadness' as Type " +
					"		FROM Face " +
					"		WHERE Face.Sadness IN( " +
					"			SELECT TOP( 3 ) MAX( Face.Sadness ) as Value " +
					"			FROM Face " +
					"			INNER JOIN Picture " +
					"			ON Face.PictureId = Picture.PictureId " +
					"			INNER JOIN Line " +
					"			ON Picture.LineId = Line.LineId " +
					"			WHERE " + ( !string.IsNullOrEmpty( userId ) ? "CONVERT( VARCHAR , Line.UserId ) = CONVERT( VARCHAR , @UserId ) " : "CONVERT( VARCHAR , Line.GroupId ) = CONVERT( VARCHAR , @GroupId ) " ) +
					"			GROUP BY Face.Sadness " +
					"			ORDER BY Face.Sadness DESC " +
					"		) " +
					"		ORDER BY Face.Sadness DESC " +
					"	) " +
					"	UNION ALL " +
					"	( " +
					"		SELECT TOP( 3 ) Face.ProcessedPicturePath as Path , Face.Surprise as Value , 'Surprise' as Type " +
					"		FROM Face " +
					"		WHERE Face.Surprise IN( " +
					"			SELECT TOP( 3 ) MAX( Face.Surprise ) as Value " +
					"			FROM Face " +
					"			INNER JOIN Picture " +
					"			ON Face.PictureId = Picture.PictureId " +
					"			INNER JOIN Line " +
					"			ON Picture.LineId = Line.LineId " +
					"			WHERE " + ( !string.IsNullOrEmpty( userId ) ? "CONVERT( VARCHAR , Line.UserId ) = CONVERT( VARCHAR , @UserId ) " : "CONVERT( VARCHAR , Line.GroupId ) = CONVERT( VARCHAR , @GroupId ) " ) +
					"			GROUP BY Face.Surprise " +
					"			ORDER BY Face.Surprise DESC " +
					"		) " +
					"		ORDER BY Face.Surprise DESC " +
					"	) " +
					") SubQuery " +
					"ORDER BY Value DESC " +
					";";

				Trace.TraceInformation( "Most Emotion Sql is : " + mostEmotionSql );

				SqlCommand selectSqlCommand = new SqlCommand( mostEmotionSql , connection );
				if( !string.IsNullOrEmpty( userId ) ) {
					selectSqlCommand.Parameters.AddWithValue( "@UserId" , userId );
					Trace.TraceInformation( "User Id is : " + userId );
				}
				else {
					selectSqlCommand.Parameters.AddWithValue( "@GroupId" , groupId );
					Trace.TraceInformation( "Group Id is : " + groupId );
				}

				Trace.TraceInformation( "SQL Execute" );
				using( SqlDataReader reader = selectSqlCommand.ExecuteReader() ) {
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

				connection.Close();

			}
			catch( Exception e ) {
				connection?.Close();
				Trace.TraceInformation( "DB Error : " + e.Message );
			}
			
		}

		/// <summary>
		/// 画像の登録
		/// </summary>
		/// <param name="userId">ユーザID</param>
		/// <param name="groupId">グループID</param>
		/// <param name="path">画像URL</param>
		/// <returns>管理番号</returns>
		public int RegistPicture( string userId , string groupId , string path ) {

			Trace.TraceInformation( "画像の登録" );
			
			SqlConnection connection = null;
			int registedPictureId = -1;
			try {

				//SQL発行
				{
					connection = new SqlConnection( this.connectionString );
					connection.Open();
					Trace.TraceInformation( "Connection Open" );

					//LineIDの取得
					int lineId = -1;
					{

						string selectLineIdSql =
							"SELECT Line.LineId as LineId " +
							"FROM Line " +
							"WHERE " + ( !string.IsNullOrEmpty( userId ) ? "CONVERT( VARCHAR , Line.UserId ) = CONVERT( VARCHAR , @UserId ) " : "CONVERT( VARCHAR , Line.GroupId ) = CONVERT( VARCHAR , @GroupId ) " ) +
							";";
						Trace.TraceInformation( "Get Line Id SQL is : " + selectLineIdSql );

						SqlCommand selectSqlCommand = new SqlCommand( selectLineIdSql , connection );
						if( !string.IsNullOrEmpty( userId ) ) {
							selectSqlCommand.Parameters.AddWithValue( "@UserId" , userId );
							Trace.TraceInformation( "User Id is : " + userId );
						}
						else {
							selectSqlCommand.Parameters.AddWithValue( "@GroupId" , groupId );
							Trace.TraceInformation( "Group Id s : " + groupId );
						}

						Trace.TraceInformation( "SQL Execute" );
						using( SqlDataReader reader = selectSqlCommand.ExecuteReader() ) {
							while( reader.Read() == true ) {
								int? readerLineId = reader[ "LineId" ] as int?;
								lineId = readerLineId.HasValue ? readerLineId.Value : -1;
								break;
							}
						}

						Trace.TraceInformation( "Line Id is : " + lineId );

					}

					//PictureIdの最大値を取得
					int maxPictureId = -1;
					{

						string selectMaxPictureIdSql =
							"SELECT TOP( 1 ) Picture.PictureId as PictureId " +
							"FROM Picture " +
							"ORDER BY Picture.PictureId DESC " +
							";";
						Trace.TraceInformation( "Get Max Picture Id SQL is : " + selectMaxPictureIdSql );

						SqlCommand selectSqlCommand = new SqlCommand( selectMaxPictureIdSql , connection );
						Trace.TraceInformation( "SQL Execute" );
						using( SqlDataReader reader = selectSqlCommand.ExecuteReader() ) {
							while( reader.Read() == true ) {
								int? readerPictureId = reader[ "PictureId" ] as int?;
								maxPictureId = readerPictureId.HasValue ? readerPictureId.Value : -1;
								break;
							}
						}
						Trace.TraceInformation( "Max Picture Id is : " + maxPictureId );

					}


					//画像の登録
					{

						registedPictureId = maxPictureId + 1;

						string registPictureSql =
							@"INSERT INTO Picture ( PictureId , OriginalUrl , LineId ) VALUES ( @PictureId , @OriginalUrl , @LineId );";
						
						Trace.TraceInformation( "Regist Picture SQL is : " + registPictureSql );

						SqlCommand insertSqlCommand = new SqlCommand( registPictureSql , connection );
						insertSqlCommand.Parameters.AddWithValue( "@PictureId" , registedPictureId );
						insertSqlCommand.Parameters.AddWithValue( "@OriginalUrl" , path );
						insertSqlCommand.Parameters.AddWithValue( "@LineId" , lineId );
						
						int result = insertSqlCommand.ExecuteNonQuery();

						if( result < 1 )
							Trace.TraceInformation( "Insert Error" );
					
					}
					
					connection.Close();

				}

			}
			catch( Exception e ) {
				connection?.Close();
				Trace.TraceInformation( "DB Error : " + e.Message );
			}
			
			return registedPictureId;

		}

		/// <summary>
		/// 表情の登録
		/// </summary>
		/// <param name="pictureId">写真管理番号</param>
		/// <param name="response">表情情報</param>
		/// <param name="path">URL</param>
		public void RegistFace( int pictureId , ResponseOfEmotionAPI response , string path ) {

			Trace.TraceInformation( "表情の登録" );
			
			SqlConnection connection = null;
			try {
				
				connection = new SqlConnection( this.connectionString );
				connection.Open();
				Trace.TraceInformation( "Connection Open" );

				//FaceIDの最大値取得
				int maxFaceId = -1;
				{

					string selectMaxFaceIdSql =
						"SELECT Face.Id as Id " +
						"FROM Face " +
						"ORDER BY Face.Id DESC" +
						";";
					Trace.TraceInformation( "Get Max Face Id SQL is : " + selectMaxFaceIdSql );

					SqlCommand selectSqlCommand = new SqlCommand( selectMaxFaceIdSql , connection );

					Trace.TraceInformation( "SQL Execute" );
					using( SqlDataReader reader = selectSqlCommand.ExecuteReader() ) {
						while( reader.Read() == true ) {
							int? readerMaxId = reader[ "Id" ] as int?;
							maxFaceId = readerMaxId.HasValue ? readerMaxId.Value : -1;
							break;
						}
					}

					Trace.TraceInformation( "Max Face Id is : " + maxFaceId );

				}

				// 表情情報の登録
				{

					string registFaceSql =
						"INSERT INTO Face " +
						"( Id , FaceId , TopPos , LeftPos , Height , Width , Anger , Contempt , Disgust , Fear , Happiness , Neutral , Sadness , Surprise , PictureId , ProcessedPicturePath ) " +
						"VALUES ( @Id , @FaceId , @TopPos , @LeftPos , @Height , @Width , @Anger , @Contempt , @Disgust , @Fear , @Happiness , @Neutral , @Sadness , @Surprise , @PictureId , @ProcessedPicturePath );";
					Trace.TraceInformation( "Regist Face Sql is : " + registFaceSql );

					SqlCommand registSqlCommand = new SqlCommand( registFaceSql , connection );
					registSqlCommand.Parameters.AddWithValue( "@Id" , maxFaceId + 1 );
					registSqlCommand.Parameters.AddWithValue( "@FaceId" , "" );
					registSqlCommand.Parameters.AddWithValue( "@TopPos" , response.faceRectangle.top );
					registSqlCommand.Parameters.AddWithValue( "@LeftPos" , response.faceRectangle.left );
					registSqlCommand.Parameters.AddWithValue( "@Height" , response.faceRectangle.height );
					registSqlCommand.Parameters.AddWithValue( "@Width" , response.faceRectangle.width );
					registSqlCommand.Parameters.AddWithValue( "@Anger" , response.scores.anger );
					registSqlCommand.Parameters.AddWithValue( "@Contempt" , response.scores.contempt );
					registSqlCommand.Parameters.AddWithValue( "@Disgust" , response.scores.disgust );
					registSqlCommand.Parameters.AddWithValue( "@Fear" , response.scores.fear );
					registSqlCommand.Parameters.AddWithValue( "@Happiness" , response.scores.happiness );
					registSqlCommand.Parameters.AddWithValue( "@Neutral" , response.scores.neutral );
					registSqlCommand.Parameters.AddWithValue( "@Sadness" , response.scores.sadness );
					registSqlCommand.Parameters.AddWithValue( "@Surprise" , response.scores.surprise );
					registSqlCommand.Parameters.AddWithValue( "@PictureId" , pictureId );
					registSqlCommand.Parameters.AddWithValue( "@ProcessedPicturePath" , path );
					Trace.TraceInformation( "Id is : " + maxFaceId + 1 );
					Trace.TraceInformation( "FaceId is : " + "" );
					Trace.TraceInformation( "Top Pos is : " + response.faceRectangle.top );
					Trace.TraceInformation( "Left Pos is : " + response.faceRectangle.left );
					Trace.TraceInformation( "Height is : " + response.faceRectangle.height );
					Trace.TraceInformation( "Width is : " + response.faceRectangle.width );
					Trace.TraceInformation( "Anger is : " + response.scores.anger );
					Trace.TraceInformation( "Contempt is : " + response.scores.contempt );
					Trace.TraceInformation( "Disgust is : " + response.scores.disgust );
					Trace.TraceInformation( "Fear is : " + response.scores.fear );
					Trace.TraceInformation( "Happiness is : " + response.scores.happiness );
					Trace.TraceInformation( "Neutral is : " + response.scores.neutral );
					Trace.TraceInformation( "Sadness is : " + response.scores.sadness );
					Trace.TraceInformation( "Surprise is : " + response.scores.surprise );
					Trace.TraceInformation( "Picture Id is : " + pictureId );
					Trace.TraceInformation( "Path is : " + path );

					int result = registSqlCommand.ExecuteNonQuery();

					if( result < 1 )
						Trace.TraceInformation( "Insert Error" );

				}

				connection.Close();

			}
			catch( Exception e ) {
				connection?.Close();
				Trace.TraceInformation( "Error : " + e.Message );
			}

		}
		
	}

}