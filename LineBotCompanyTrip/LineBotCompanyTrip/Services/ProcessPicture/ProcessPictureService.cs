using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace LineBotCompanyTrip.Services.ProcessPicture {

	/// <summary>
	/// 画像加工サービス
	/// </summary>
	public class ProcessPictureService {

		/// <summary>
		/// 枠線を指定色で囲う
		/// </summary>
		/// <param name="imageBytes">画像のバイナリデータ</param>
		/// <param name="posX">枠左上座標（左）</param>
		/// <param name="posY">枠左上座標（上）</param>
		/// <param name="width">枠幅</param>
		/// <param name="height">枠高さ</param>
		/// <param name="pen">Pensで取得するペンの色</param>
		/// <returns>加工済み画像のバイナリデータ</returns>
		public byte[] DrawFrame( byte[] imageBytes , int posX , int posY , int width , int height , Pen pen ) {

			Bitmap bitmap = new Bitmap( new MemoryStream( imageBytes ) );
			Graphics graphics = Graphics.FromImage( bitmap );
			
			graphics.DrawLine( pen , posX , posY , posX , posY + height );
			graphics.DrawLine( pen , posX , posY , posX + width , posY );
			graphics.DrawLine( pen , posX + width , posY , posX + width , posY + height );
			graphics.DrawLine( pen , posX , posY + height , posX + width , posY + height );

			//一回り大きいサイズで描画
			graphics.DrawLine( pen , posX - 1 , posY - 1 , posX - 1 , posY + height + 1 );
			graphics.DrawLine( pen , posX - 1 , posY - 1 , posX + width + 1 , posY - 1 );
			graphics.DrawLine( pen , posX + width + 1 , posY - 1 , posX + width + 1 , posY + height + 1 );
			graphics.DrawLine( pen , posX - 1 , posY + height + 1 , posX + width + 1 , posY + height + 1 );

			//もう一回り大きいサイズで描画
			graphics.DrawLine( pen , posX - 2 , posY - 2 , posX - 2 , posY + height + 2 );
			graphics.DrawLine( pen , posX - 2 , posY - 2 , posX + width + 2 , posY - 2 );
			graphics.DrawLine( pen , posX + width + 2 , posY - 2 , posX + width + 2 , posY + height + 2 );
			graphics.DrawLine( pen , posX - 2 , posY + height + 2 , posX + width + 2 , posY + height + 2 );
			
			graphics.Dispose();

			MemoryStream savedStream = new MemoryStream();
			bitmap.Save( savedStream , ImageFormat.Jpeg );
			byte[] bytes = savedStream.GetBuffer();

			savedStream.Close();

			return savedStream.GetBuffer();
			
		}

		/// <summary>
		/// 指定位置にメッセージを描画する
		/// </summary>
		/// <param name="imageBytes">画像のバイナリデータ</param>
		/// <param name="posX">描画位置（左）</param>
		/// <param name="posY">描画位置（上）</param>
		/// <param name="pen">Pensで取得するペンの色</param>
		/// <param name="brush">Brushesで取得するブラシの色</param>
		/// <param name="message">メッセージ</param>
		/// <returns>加工済み画像のバイナリデータ</returns>
		public byte[] DrawMessage( byte[] imageBytes , int posX , int posY , Pen pen , Brush brush , string message ) {

			Bitmap bitmap = new Bitmap( new MemoryStream( imageBytes ) );
			Graphics graphics = Graphics.FromImage( bitmap );
			Font font = new Font( "MS UI Gothic" , 20 );

			graphics.FillRectangle( brush , posX , posY , message.Length * 5 , 20 );
			graphics.DrawString( message , font , Brushes.White , posX , posY );

			font.Dispose();

			MemoryStream savedStream = new MemoryStream();
			bitmap.Save( savedStream , ImageFormat.Jpeg );
			byte[] bytes = savedStream.GetBuffer();

			graphics.Dispose();
			
			return bytes;

		}
		
	}

}