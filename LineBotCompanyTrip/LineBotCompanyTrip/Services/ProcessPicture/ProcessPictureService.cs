using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using LineBotCompanyTrip.Common;
using LineBotCompanyTrip.Models.AzureCognitiveServices.EmotionAPI;
using System.Diagnostics;

namespace LineBotCompanyTrip.Services.ProcessPicture {

	/// <summary>
	/// 画像加工サービス
	/// </summary>
	public class ProcessPictureService {

		/// <summary>
		/// 解析画像に必要なデータを帰る
		/// </summary>
		/// <param name="type">表情タイプ</param>
		/// <param name="value">表情値</param>
		/// <param name="penColor">ペン色</param>
		/// <param name="brushColor">ブラシ色</param>
		/// <param name="text">描画文字列</param>
		private void GetAnalysisDataForDrawing( CommonEnum.EmotionType type , double value , ref Pen penColor , ref Brush brushColor , ref string text ) {

			Trace.TraceInformation( "Get Analysis Data For Drawing Start" );
			
			switch( type ) {
				case CommonEnum.EmotionType.happiness:
					penColor = Pens.Pink;
					brushColor = Brushes.Pink;
					text = "Happy:" + CommonUtil.ConvertDecimalIntoPercentage( value );
					break;
				case CommonEnum.EmotionType.anger:
					penColor = Pens.Red;
					brushColor = Brushes.Red;
					text = "Anger:" + CommonUtil.ConvertDecimalIntoPercentage( value );
					break;
				case CommonEnum.EmotionType.contempt:
					penColor = Pens.Orange;
					brushColor = Brushes.Orange;
					text = "Contempt:" + CommonUtil.ConvertDecimalIntoPercentage( value );
					break;
				case CommonEnum.EmotionType.sadness:
					penColor = Pens.Blue;
					brushColor = Brushes.Blue;
					text = "Sadness:" + CommonUtil.ConvertDecimalIntoPercentage( value );
					break;
				case CommonEnum.EmotionType.disgust:
					penColor = Pens.Aqua;
					brushColor = Brushes.Aqua;
					text = "Disgust:" + CommonUtil.ConvertDecimalIntoPercentage( value );
					break;
				case CommonEnum.EmotionType.fear:
					penColor = Pens.Yellow;
					brushColor = Brushes.Yellow;
					text = "Fear:" + CommonUtil.ConvertDecimalIntoPercentage( value );
					break;
				case CommonEnum.EmotionType.surprise:
					penColor = Pens.Green;
					brushColor = Brushes.Green;
					text = "Surprise:" + CommonUtil.ConvertDecimalIntoPercentage( value );
					break;
				default:
					penColor = Pens.Gray;
					brushColor = Brushes.Gray;
					text = "Neutral:" + CommonUtil.ConvertDecimalIntoPercentage( value );
					break;
			}

			Trace.TraceInformation( "Analysis Data For Drawing Pen Color is : " + penColor.Color.ToString() );
			Trace.TraceInformation( "Analysis Data For Drawing Brush Color is : " + brushColor.ToString() );
			Trace.TraceInformation( "Analysis Data For Drawing Text is : " + text );
			
			Trace.TraceInformation( "Get Analysis Data For Drawing End" );

		}

		/// <summary>
		/// 枠線を指定色で囲う
		/// </summary>
		/// <param name="pictureBytes">画像のバイナリデータ</param>
		/// <param name="posX">枠左上座標（左）</param>
		/// <param name="posY">枠左上座標（上）</param>
		/// <param name="width">枠幅</param>
		/// <param name="height">枠高さ</param>
		/// <param name="pen">Pensで取得するペンの色</param>
		/// <returns>加工済み画像のバイナリデータ</returns>
		private byte[] DrawFrameOnPicture( byte[] pictureBytes , int posX , int posY , int width , int height , Pen pen ) {
			
			Trace.TraceInformation( "Draw Frame On Picture Start" );

			Trace.TraceInformation( "Frame Pos is : ( " + posX + " , " + posY + " )" );
			Trace.TraceInformation( "Frame Width is : " + width );
			Trace.TraceInformation( "Frame Height is : " + height );
			Trace.TraceInformation( "Frame Color is : " + pen.Color.ToString() );

			MemoryStream pictureStream = new MemoryStream( pictureBytes );

			Bitmap bitmap = new Bitmap( pictureStream );
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

			bitmap.Dispose();
			graphics.Dispose();
			pictureStream.Dispose();
			savedStream.Dispose();

			Trace.TraceInformation( "Draw Frame On Picture End" );
			
			return bytes;
			
		}

		/// <summary>
		/// 指定位置にメッセージを描画する
		/// </summary>
		/// <param name="pictureBytes">画像のバイナリデータ</param>
		/// <param name="posX">描画位置（左）</param>
		/// <param name="posY">描画位置（上）</param>
		/// <param name="pen">Pensで取得するペンの色</param>
		/// <param name="brush">Brushesで取得するブラシの色</param>
		/// <param name="message">メッセージ</param>
		/// <returns>加工済み画像のバイナリデータ</returns>
		private byte[] DrawMessageOnPicture( byte[] pictureBytes , int posX , int posY , Pen pen , Brush brush , string message ) {

			Trace.TraceInformation( "Draw Message On Picture Start" );

			Trace.TraceInformation( "Message Pos is : ( " + posX + " , " + posY + " )" );
			Trace.TraceInformation( "Message Pen Color is : " + pen.Color.ToString() );
			Trace.TraceInformation( "Message Brush Color is : " + brush.ToString() );
			Trace.TraceInformation( "Message is : " + message );

			MemoryStream pictureStream = new MemoryStream( pictureBytes );
			Bitmap bitmap = new Bitmap( pictureStream );
			Graphics graphics = Graphics.FromImage( bitmap );
			Font font = new Font( "MS UI Gothic" , 10 );

			graphics.FillRectangle( brush , posX - 2 , posY - 12 , message.Length * 5 + 20 , 12 );
			graphics.DrawString( 
				message , 
				font , 
				( 
					pen == Pens.Aqua || pen == Pens.Pink || pen == Pens.Yellow || pen == Pens.Orange
					? Brushes.Black 
					: Brushes.White 
				) , 
				posX - 2 , 
				posY - 12
			);


			MemoryStream savedStream = new MemoryStream();
			bitmap.Save( savedStream , ImageFormat.Jpeg );
			byte[] bytes = savedStream.GetBuffer();

			font.Dispose();
			graphics.Dispose();
			bitmap.Dispose();
			pictureStream.Dispose();
			savedStream.Dispose();

			Trace.TraceInformation( "Draw Message On Picture End" );

			return bytes;

		}
		
		/// <summary>
		/// 解析結果を画像のバイナリデータに描画する
		/// </summary>
		/// <param name="pictureBytes">画像のバイナリデータ</param>
		/// <param name="response">Emotion APIで取得した結果</param>
		/// <returns>描画後画像のバイナリデータ</returns>
		public byte[] DrawAnalysisOnPicture( byte[] pictureBytes , ResponseOfEmotionRecognitionAPI response ) {

			Trace.TraceInformation( "Draw Analysis On Picture Start" );
			
			CommonEnum.EmotionType type = CommonEnum.EmotionType.neutral;
			double value = 0.0;
			CommonUtil.GetMostEmotion( response , ref type , ref value );

			Pen penColor = null;
			Brush brushColor = null;
			string text = "";
			this.GetAnalysisDataForDrawing( type , value , ref penColor , ref brushColor , ref text );

			pictureBytes = this.DrawFrameOnPicture(
				pictureBytes ,
				response.faceRectangle.left ,
				response.faceRectangle.top ,
				response.faceRectangle.width ,
				response.faceRectangle.height ,
				penColor
			);

			pictureBytes = this.DrawMessageOnPicture(
				pictureBytes ,
				response.faceRectangle.left ,
				response.faceRectangle.top ,
				penColor ,
				brushColor ,
				text
			);

			Trace.TraceInformation( "Draw Analysis On Picture End" );

			return pictureBytes;

		}

	}

}