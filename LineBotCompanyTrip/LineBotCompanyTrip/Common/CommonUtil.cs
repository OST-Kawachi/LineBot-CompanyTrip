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
		public static string ConvertDecimalIntoPercentage( double value ) {
			
			return ( Math.Truncate( value * 10000 ) / 100 ) + "%";
			
		}

	}

}