using System;
using System.Xml;

namespace FlashCards
{
	/// <summary>
	/// This is a translation of a word into one language
	/// </summary>
	public class Translation
	{
		#region Properties

		/// <summary>
		/// The language of this translation
		/// </summary>
		public string Language { get; set; }

		/// <summary>
		/// The translated word of this language
		/// </summary>
		public string Word { get; set; }

		#endregion //Properties

		#region Methods

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
				case "Language":
					{
						Language = value;
					}
					break;
				default:
					{
						throw new Exception(string.Format("bad xml name passed to Translation.ParseChildNode: {0}", name));
					}
			}
		}

		public void WriteXmlNodes(XmlTextWriter xmlFile)
		{
			xmlFile.WriteStartElement("Translation");

			xmlFile.WriteAttributeString("Language", Language);
			xmlFile.WriteAttributeString("Word", Word);

			xmlFile.WriteEndElement();
		}

		#endregion //Methods
	}
}
