using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PS_Field_Install.Scripts {

	public static class TextTools {

		public static string MyRelativePath(string file) {
			return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file);
		}

		public static int CountStringOccurrences(string text, string pattern) {
			int count = 0;
			int i = 0;

			while ((i = text.IndexOf(pattern, i)) != -1) {
				i += pattern.Length;
				count++;
			}

			return count;
		}

		private static string[] GetListItems(string text) {
			string[] itemList = text.Split(',');
			int count = itemList.Length;
			int currItem = 0;
			string[] cleanedItemList = new string[count];

			foreach (string str in itemList) {
				string newStr = str.Trim(' ');
				cleanedItemList[currItem] = newStr;
				currItem++;
			}

			return cleanedItemList;
		}

		private static string[] GetListItems(string text, string splitter) {
			string[] splitterArray = new string[1];
			splitterArray[0] = splitter;
			string[] itemList = text.Split(splitterArray, System.StringSplitOptions.RemoveEmptyEntries);
			int count = itemList.Length;
			int currItem = 0;
			string[] cleanedItemList = new string[count];

			foreach (string str in itemList) {
				string newStr = str.Trim(' ');
				cleanedItemList[currItem] = newStr;
				currItem++;
			}

			return cleanedItemList;
		}

		public static string GrammerifyList(string text, string finalSeperator) {
			string modifiedString = "";

			int numItemsInList = CountStringOccurrences(text, ",") + 1;
			string[] list = GetListItems(text);

			for (int i = 0; i < numItemsInList; i++) {
				if (list.Length == 2) {
					return list[0] + " " + finalSeperator + " " + list[1];
				} else if (numItemsInList - i == 2) {
					list[i] = list[i] + " " + finalSeperator + " ";
					break;
				} else {
					list[i] = list[i] + ", ";
				}
			}

			foreach (string str in list) {
				modifiedString += str;
			}

			return modifiedString;
		}

		public static string[] SplitToArray(string text, string splitter) {
			return GetListItems(text, splitter);
		}

		public static byte[] GetBytes(string str) {
			byte[] bytes = new byte[str.Length * sizeof(char)];
			Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
			return bytes;
		}

		public static string GetProductFamily(string description) {
			string family = "";
			string temp = "";

			if (description.Contains(" ")) {

				int firstSpace = description.IndexOf(" ");
				temp = description.Substring(0, description.IndexOf(" ", firstSpace));

				if (temp.Substring(firstSpace).Contains("L")) {
					temp = description.Substring(0, firstSpace);
				}

				family = temp.Trim(' ');
				family = family.Replace(" ", "");
			} else {
				family = description;
			}

			return family;
		}

		public static IEnumerable<T> GetValues<T>() {
			return Enum.GetValues(typeof(T)).Cast<T>();
		}

		/// <summary>
		/// Reads a text file and returns lines in a List
		/// </summary>
		/// <param name="filepath">The absolute path of the text file to read</param>
		/// <param name="removeSpaces">Replaces spaces with underscores</param>
		/// <returns></returns>
		public static List<string> ReadFileLines(string filepath, bool removeSpaces) {
			StreamReader sr = new StreamReader(filepath);
			string line;
			List<string> data = new List<string>();

			while ((line = sr.ReadLine()) != null) {
				if (removeSpaces) {
					line = line.Replace(" ", "_");
				}
				data.Add(line);
			}

			sr.Close();
			return data;
		}

	}

}
