using System;
using System.Configuration;
using System.Text;
using log4net;

namespace com.TheSilentGroup.Fluorine
{
	public class LineEndingConverter
	{

		public const char CR_CHAR = '\r';
		public const string CR_STRING = "\r";
		public const char LF_CHAR = '\n';
		public const string LF_STRING = "\n";
		public readonly static char[] CRLF_CHARS = new char[] { CR_CHAR, LF_CHAR };
		public const string CRLF_STRING = CR_STRING + LF_STRING;

		public readonly static bool ConversionEnabled = GetConversionEnabled();

		public static string Convert(string source, string newLineEnding)
		{
			if (ConversionEnabled && source.IndexOfAny(CRLF_CHARS) != -1)
			{
				return ConvertImpl(source, newLineEnding);
			}
			return source;
		}

		private static string ConvertImpl(string source, string newLineEnding)
		{
			
			int len = source.Length;
			StringBuilder result = new StringBuilder(len + 16);
			int substringStart = 0;
			
			for(int pos = 0; pos<len; pos++)
			{
				switch(source[pos])
				{
					case CR_CHAR:
						result.Append(source.Substring(substringStart, pos - substringStart) + newLineEnding);
						substringStart = pos + 1;
						break;

					case LF_CHAR:
						if (pos == 0 || source[pos-1] != CR_CHAR)
						{
							result.Append(source.Substring(substringStart, pos - substringStart) + newLineEnding);
						}
						substringStart = pos + 1;
						break;
				}					
			}

			if (substringStart < len)
			{
				result.Append(source.Substring(substringStart));
			}
			return result.ToString();
		}

		private static bool GetConversionEnabled()
		{
			string s = ConfigurationManager.AppSettings["FluorineConvertLineEndings"];
			if (s == null)
			{
				return false;
			}

			try
			{
				return System.Convert.ToBoolean(s);
			}
			catch(Exception ex)
			{
				ILog log = LogManager.GetLogger(typeof (LineEndingConverter));
				if (log != null)
				{
					log.Error("Error converting FluorineConvertLineEndings setting '" + s + "' to boolean.", ex);
				}
				return false;
			}
		}

		private LineEndingConverter()
		{
		}
	}
}
