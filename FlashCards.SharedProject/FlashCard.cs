using System;
using System.Collections.Generic;
using System.Xml;
using XmlBuddy;

namespace FlashCards
{
	/// <summary>
	/// a single flash card that represents one word and all the available translations for that word.
	/// </summary>
	public class FlashCard
	{
		#region Properties

		public List<Translation> Translations { get; private set; }

		public string Language1 { get; set; }
		public string Language2 { get; set; }

		#endregion //Properties

		#region Methods

		public FlashCard()
		{
			Translations = new List<Translation>();
		}

		/// <summary>
		/// Read in all each individual peice of data for this content entry
		/// </summary>
		/// <param name="xmlNode"></param>
		public void ParseChildNode(XmlNode xmlNode)
		{
			string name = xmlNode.Name;
			string value = xmlNode.InnerText;

			switch (name.ToLower())
			{
				case "words":
					{
						XmlFileBuddy.ReadChildNodes(xmlNode, ParseCardXmlNodes);
					}
					break;
				case "word":
					{
						//add a translation with language1
						Translations.Add(new Translation
						{
							Language = Language1,
							Word = value
						}
						);
					}
					break;
				case "translation":
					{
						//add a translation with language1
						Translations.Add(new Translation
						{
							Language = Language2,
							Word = value
						}
						);
					}
					break;
				default:
				{
					throw new Exception(string.Format("bad xml name passed to flash card: {0}", name));
				}
			}
		}

		public void ParseCardXmlNodes(XmlNode xmlNode)
		{
			//create a new translation
			var translation = new Translation();

			//read it in 
			XmlFileBuddy.ReadChildNodes(xmlNode, translation.ParseChildNode);

			//If there was no translation in the card, don't store it
			if (!string.IsNullOrEmpty(translation.Language) && !string.IsNullOrEmpty(translation.Word))
			{
				//store the card
				Translations.Add(translation);
			}
		}

		public void WriteXmlNodes(XmlTextWriter xmlFile)
		{
			//write out all the cards
			xmlFile.WriteStartElement("Words");
			foreach (var translation in Translations)
			{
				translation.WriteXmlNodes(xmlFile);
			}
			xmlFile.WriteEndElement();
		}

		#endregion //Methods
	}
}