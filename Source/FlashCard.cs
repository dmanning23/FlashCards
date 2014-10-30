using System;
using System.Xml;

namespace FlashCards
{
	/// <summary>
	/// a single flash card with a word and a translation
	/// </summary>
	public class FlashCard
	{
		#region Properties

		/// <summary>
		/// the word of the source language being translated
		/// </summary>
		public string Word { get; set; }

		/// <summary>
		/// the translation of that word in the target language
		/// </summary>
		public string Translation { get; set; }

		#endregion //Properties

		#region Methods

		public FlashCard()
		{
		}

		/// <summary>
		/// Read in all each individual peice of data for this content entry
		/// </summary>
		/// <param name="xmlNode"></param>
		public void ParseChildNode(XmlNode xmlNode)
		{
			string name = xmlNode.Name;
			string value = xmlNode.InnerText;

			switch (name)
			{
				case "Word":
				{
					Word = value;
				}
				break;
				case "Translation":
				{
					Translation = value;
				}
				break;
				default:
				{
					throw new Exception(string.Format("bad xml name passed to flash card: {0}", name));
				}
			}
		}

		public void WriteXmlNodes(XmlTextWriter xmlFile)
		{
			xmlFile.WriteStartElement("Item");

			xmlFile.WriteStartElement("Word");
			xmlFile.WriteString(Word);
			xmlFile.WriteEndElement();

			xmlFile.WriteStartElement("Translation");
			xmlFile.WriteString(Translation);
			xmlFile.WriteEndElement();

			xmlFile.WriteEndElement();
		}

		#endregion //Methods
	}
}