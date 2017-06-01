using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using LineBotCompanyTrip.Common;
using LineBotCompanyTrip.Models.AzureCognitiveServices.EmotionAPI;
using System.Diagnostics;
using System;

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

			pictureBytes = this.ResizePicture(
				pictureBytes ,
				response.faceRectangle.top ,
				response.faceRectangle.left ,
				response.faceRectangle.height ,
				response.faceRectangle.width
			);

			Trace.TraceInformation( "Draw Analysis On Picture End" );

			return pictureBytes;

		}
		
		/// <summary>
		/// 画像を縦横比1:1.51に合わせてサイズを変更する
		/// できるだけ認識された顔画像が中央にくるように調節
		/// </summary>
		/// <param name="pictureBytes">加工用画像</param>
		/// <param name="top">左上座標（上）</param>
		/// <param name="left">左上座標（左）</param>
		/// <param name="height">顔サイズ（高さ）</param>
		/// <param name="width">顔サイズ（横）</param>
		/// <returns>リサイズされた画像</returns>
		private byte[] ResizePicture( byte[] pictureBytes , int top , int left , int height , int width ) {

			Trace.TraceInformation( "Resize Picture Start" );

			Trace.TraceInformation( "Face Pos is : ( " + left + " , " + top + " )" );
			Trace.TraceInformation( "Face Size is : ( " + width + " , " + height + " )" );

			MemoryStream pictureStream = null;
			Bitmap bitmap = null;
			try {
				pictureStream = new MemoryStream( pictureBytes );
				bitmap = new Bitmap( pictureStream );
			}
			catch( ArgumentException e ) {

				Trace.TraceError( "Resize Picture Argument Exception : " + e.Message );

				pictureStream?.Dispose();
				bitmap?.Dispose();
				Trace.TraceInformation( "Resize Picture End" );

				return pictureBytes;

			}

			//キャンバスのサイズ取得
			int canvasSizeWidth = 0;
			int canvasSizeHeight = 0;
			{

				canvasSizeWidth = bitmap.Size.Width;
				canvasSizeHeight = bitmap.Size.Height;

				Trace.TraceInformation( "Picture Size is ( " + canvasSizeWidth + " , " + canvasSizeHeight + " )" );

				//キャンバスの比率を合わせる
				if( canvasSizeWidth < canvasSizeHeight * 1.51 )
					canvasSizeHeight = (int)( canvasSizeWidth / 1.51 );
				else
					canvasSizeWidth = (int)( canvasSizeHeight * 1.15 );
				
				Trace.TraceInformation( "Resized Picture Size is ( " + canvasSizeWidth + " , " + canvasSizeHeight + " )" );

				//顔サイズが小さかった場合、顔高さ：キャンバス高さが1:2になるまでキャンバスサイズを小さくする
				if( height * 2 < canvasSizeHeight ) {
					canvasSizeHeight = (int)( canvasSizeHeight * height * 2 / canvasSizeHeight );
					canvasSizeWidth = (int)( canvasSizeWidth * height * 2 / canvasSizeHeight );
				}
				
			}

			Bitmap canvas = null;
			try {
				canvas = new Bitmap( canvasSizeWidth , canvasSizeHeight );
			}
			catch( Exception e ) {

				Trace.TraceError( "Resize Picture Exception : " + e.Message );

				canvas?.Dispose();
				bitmap?.Dispose();
				pictureStream?.Dispose();

				Trace.TraceInformation( "Resize Picture End" );

				return pictureBytes;

			}

			Graphics graphics = Graphics.FromImage( canvas );

			//bitmapから切り取る部分を取得する
			int cutPosX = 0;
			int cutPosY = 0;
			{

				int faceCenterX = left + ( width / 2 );
				int faceCenterY = top + ( height / 2 );

				cutPosX = faceCenterX - ( canvasSizeWidth / 2 );
				cutPosY = faceCenterY - ( canvasSizeHeight / 2 );
				
				Trace.TraceInformation( "Cut Pos is ( " + cutPosX + " , " + cutPosY + " )" );

			}

			//キャンバスのサイズ分切り取る
			Rectangle srcRect = new Rectangle( cutPosX , cutPosY , canvasSizeWidth , canvasSizeHeight );

			//座標(0,0)からキャンバスのサイズ分だけ描画（キャンバスサイズ分トリミング）
			Rectangle destRect = new Rectangle( 0 , 0 , canvasSizeWidth , canvasSizeHeight );

			try {
				graphics.DrawImage( bitmap , destRect , srcRect , GraphicsUnit.Pixel );
			}
			catch( ArgumentException e ) {

				Trace.TraceError( "Resize Picture Argument Exception : " + e.Message );

				graphics.Dispose();
				canvas?.Dispose();
				bitmap?.Dispose();
				pictureStream?.Dispose();

				Trace.TraceInformation( "Resize Picture End" );

				return pictureBytes;

			}
			MemoryStream savedStream = new MemoryStream();
			canvas.Save( savedStream , ImageFormat.Jpeg );
			byte[] bytes = savedStream.GetBuffer();

			graphics.Dispose();
			bitmap.Dispose();
			canvas.Dispose();
			pictureStream.Dispose();
			savedStream.Dispose();
			
			Trace.TraceInformation( "Resize Picture End" );

			return bytes;

		}

	}

}