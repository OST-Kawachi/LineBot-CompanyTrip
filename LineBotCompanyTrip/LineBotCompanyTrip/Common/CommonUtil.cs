using LineBotCompanyTrip.Models.AzureCognitiveServices.EmotionAPI;
using System;

namespace LineBotCompanyTrip.Common {

	/// <summary>
	/// 共通処理定義
	/// </summary>
	public class CommonUtil {

		/// <summary>
		/// 小数をパーセンテージ表示に変換する
		/// </summary>
		/// <param name="value">小数値</param>
		/// <returns>小数第二位までのパーセント表示文字列</returns>
		public static string ConvertDecimalIntoPercentage( double value ) => ( Math.Truncate( value * 10000 ) / 100 ) + "%";
			
		/// <summary>
		/// レスポンスから最も近い解析結果を返す
		/// </summary>
		/// <param name="response">レスポンス</param>
		/// <param name="type">表情種別</param>
		/// <param name="value">表情評価</param>
		public static void GetMostEmotion( ResponseOfEmotionRecognitionAPI response , ref CommonEnum.EmotionType type , ref double value ) {

			//幸せ度40%以上で幸せ認定
			double happiness = Math.Truncate( response.scores.happiness * 10000 ) / 10000;
			if( happiness > 0.4 ) {
				type = CommonEnum.EmotionType.happiness;
				value = happiness;
				return;
			}

			//悲しみ度40%以上で幸せ認定
			double sadness = Math.Truncate( response.scores.sadness * 10000 ) / 10000;
			if( sadness > 0.4 ) {
				type = CommonEnum.EmotionType.sadness;
				value = sadness;
				return;
			}

			//ビビり度40%以上で幸せ認定
			double fear = Math.Truncate( response.scores.fear * 10000 ) / 10000;
			if( fear > 0.4 ) {
				type = CommonEnum.EmotionType.fear;
				value = fear;
				return;
			}

			//怒り度40%以上で幸せ認定
			double anger = Math.Truncate( response.scores.anger * 10000 ) / 10000;
			if( anger > 0.4 ) {
				type = CommonEnum.EmotionType.anger;
				value = anger;
				return;
			}

			//軽蔑度40%以上で幸せ認定
			double contempt = Math.Truncate( response.scores.contempt * 10000 ) / 10000;
			if( contempt > 0.4 ) {
				type = CommonEnum.EmotionType.contempt;
				value = contempt;
				return;
			}

			//うんざり度40%以上で幸せ認定
			double disgust = Math.Truncate( response.scores.disgust * 10000 ) / 10000;
			if( disgust > 0.4 ) {
				type = CommonEnum.EmotionType.disgust;
				value = disgust;
				return;
			}

			//驚き度40%以上で幸せ認定
			double surprise = Math.Truncate( response.scores.surprise * 10000 ) / 10000;
			if( surprise > 0.4 ) {
				type = CommonEnum.EmotionType.surprise;
				value = surprise;
				return;
			}

			//どれも40%超えてなかった場合は真顔認定
			double neutral = Math.Truncate( response.scores.neutral * 10000 ) / 10000;
			type = CommonEnum.EmotionType.neutral;
			value = neutral;
			
		}

		/// <summary>
		/// 文字列から表情種別を返す
		/// </summary>
		/// <param name="emotionString">表情種別（文字列）</param>
		/// <returns>表情種別</returns>
		public static CommonEnum.EmotionType ConvertEmotionStringIntoType( string emotionString ) {

			if( emotionString == null || emotionString == "" )
				return CommonEnum.EmotionType.neutral;

			switch( emotionString ) {
				case "Anger":
					return CommonEnum.EmotionType.anger;
				case "Contempt":
					return CommonEnum.EmotionType.contempt;
				case "Disgust":
					return CommonEnum.EmotionType.disgust;
				case "Fear":
					return CommonEnum.EmotionType.fear;
				case "Happiness":
					return CommonEnum.EmotionType.happiness;
				case "Sadness":
					return CommonEnum.EmotionType.sadness;
				case "Surprise":
					return CommonEnum.EmotionType.surprise;
				default:
					return CommonEnum.EmotionType.neutral;
			}
			
		}
		
	}

}
