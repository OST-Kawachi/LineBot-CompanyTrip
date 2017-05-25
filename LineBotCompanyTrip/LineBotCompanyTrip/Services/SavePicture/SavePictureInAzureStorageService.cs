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
		/// 保存する画像名
		/// pic_(本日日付).jpg
		/// 例：pic_20170525142715.jpg
		/// </summary>
		private static readonly string PictureName = "pic_";

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
		/// <returns>URL</returns>
		public string SaveImage( byte[] imageBytes ) {
			
			string pictureName =
				SavePictureInAzureStorageService.PictureName +
				DateTime.Now.Year.ToString() +
				DateTime.Now.Month.ToString() +
				DateTime.Now.Day.ToString() +
				DateTime.Now.Hour.ToString() +
				DateTime.Now.Minute.ToString() +
				DateTime.Now.Second.ToString() +
				DateTime.Now.Millisecond.ToString() + 
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