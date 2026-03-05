using System.Collections;
using System.Globalization;
using System.Text;

namespace Zlo4NET.Core.Helpers;

internal class PHPDeserializer : IPHPDeserializer
{
	public bool XMLSafe = true;

	public Encoding StringEncoding = new UTF8Encoding();

	private NumberFormatInfo nfi;

	public PHPDeserializer()
	{
		nfi = new NumberFormatInfo();
		nfi.NumberGroupSeparator = "";
		nfi.NumberDecimalSeparator = ".";
	}

	public object Deserialize(string content)
	{
		if (!string.IsNullOrEmpty(content))
		{
			return _deserializeObjectFromString(content);
		}
		return new object();
	}

	private object _deserializeObjectFromString(string content, int handlePosition = 0)
	{
		switch (content[handlePosition])
		{
		case 'N':
			handlePosition += 2;
			return null;
		case 'b':
		{
			char num4 = content[handlePosition + 2];
			handlePosition += 4;
			return num4 == '1';
		}
		case 'i':
		{
			int num = content.IndexOf(":", handlePosition) + 1;
			int num2 = content.IndexOf(";", num);
			string text3 = content.Substring(num, num2 - num);
			handlePosition += 3 + text3.Length;
			return int.Parse(text3, nfi);
		}
		case 'd':
		{
			int num = content.IndexOf(":", handlePosition) + 1;
			int num2 = content.IndexOf(";", num);
			string text2 = content.Substring(num, num2 - num);
			handlePosition += 3 + text2.Length;
			return double.Parse(text2, nfi);
		}
		case 's':
		{
			int num = content.IndexOf(":", handlePosition) + 1;
			int num2 = content.IndexOf(":", num);
			string text = content.Substring(num, num2 - num);
			int num5 = int.Parse(text);
			int num3 = num5;
			if (num2 + 2 + num3 >= content.Length)
			{
				num3 = content.Length - 2 - num2;
			}
			string text4 = content.Substring(num2 + 2, num3);
			while (StringEncoding.GetByteCount(text4) > num5)
			{
				num3--;
				text4 = content.Substring(num2 + 2, num3);
			}
			handlePosition += 6 + text.Length + num3;
			if (XMLSafe)
			{
				text4 = text4.Replace("\n", "\r\n");
			}
			return text4;
		}
		case 'a':
		{
			int num = content.IndexOf(":", handlePosition) + 1;
			int num2 = content.IndexOf(":", num);
			string text = content.Substring(num, num2 - num);
			int num3 = int.Parse(text);
			Hashtable hashtable = new Hashtable(num3);
			ArrayList arrayList = new ArrayList(num3);
			handlePosition += 4 + text.Length;
			for (int i = 0; i < num3; i++)
			{
				object obj = _deserializeObjectFromString(content, handlePosition);
				object value = _deserializeObjectFromString(content, handlePosition);
				if (arrayList != null)
				{
					if (obj is int && (int)obj == arrayList.Count)
					{
						arrayList.Add(value);
					}
					else
					{
						arrayList = null;
					}
				}
				hashtable[obj] = value;
			}
			handlePosition++;
			if (handlePosition < content.Length && content[handlePosition] == ';')
			{
				handlePosition++;
			}
			if (arrayList != null)
			{
				return arrayList;
			}
			return hashtable;
		}
		default:
			return "";
		}
	}
}
