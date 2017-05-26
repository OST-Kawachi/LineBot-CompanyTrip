using LineBotCompanyTrip.Configurations;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Diagnostics;
using System.IO;

namespace LineBotCompanyTrip.Services.SavePicture {

	/// <summary>
	/// Azure Storageに画像を保存するサービス
	/// </summary>
	public class SavePictureInAzureStorageService {

		/// <summary>
		/// BLOBを保存するコンテナ
		/// </summary>
		private CloudBlobContainer cloudBlobContainer;

		/// <summary>
		/// Azure StorageのURL
		/// </summary>
		private static readonly string AzureStorageUrl = "https://linebotcompanytrip.blob.core.windows.net/";

		/// <summary>
		/// 画像を保存するコンテナ名
		/// </summary>
		private static readonly string ContainerName = "pictures";
		
		/// <summary>
		/// コンストラクタ
		/// Azure Storageに接続し、picturesコンテナを取得
		/// </summary>
		public SavePictureInAzureStorageService() {

			StorageCredentials storageCredentials = new StorageCredentials( AzureStorageConfig.AccountName , AzureStorageConfig.AccessKey );
			CloudStorageAccount cloudStorageAccount = new CloudStorageAccount( storageCredentials , true );

			CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
			this.cloudBlobContainer = cloudBlobClient.GetContainerReference( SavePictureInAzureStorageService.ContainerName );

			Trace.TraceInformation( "Azure Storage と接続開始" );

		}

		/// <summary>
		/// 画像を保存する
		/// </summary>
		/// <param name="imageBytes">画像のバイナリデータ</param>
		/// <param name="timestamp">Webhook受信日時</param>
		/// <param name="isOriginal">オリジナル画像かどうか</param>
		/// <returns>URL</returns>
		public string SaveImage( byte[] imageBytes , string timestamp , bool isOriginal ) {
			
			string pictureName = 
				( isOriginal ? "original_" : "processed_" ) +
				timestamp + 
				".jpg";

			Trace.TraceInformation( "Picture Name is : " + pictureName );
			
			CloudBlockBlob cloudBlockBlob = this.cloudBlobContainer.GetBlockBlobReference( pictureName );
			cloudBlockBlob.UploadFromStream( new MemoryStream( imageBytes ) );

			Trace.TraceInformation( "Picture Upload In Azure Storage : SUCCESS" );

			string imagePath = 
				SavePictureInAzureStorageService.AzureStorageUrl +
				SavePictureInAzureStorageService.ContainerName +
				"/" +
				pictureName;

			Trace.TraceInformation( "Picture Path is : " + imagePath );

			return imagePath;

		}

	}

}