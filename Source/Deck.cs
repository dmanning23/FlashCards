using FilenameBuddy;
using System;
using System.Collections.Generic;
using System.Xml;
using XmlBuddy;

namespace FlashCards
{
	/// <summary>
	/// A pile of flash cards to run through.
	/// </summary>
	public class Deck : XmlFileBuddy
	{
		#region Properties

		/// <summary>
		/// what category of words is covered by this deck
		/// </summary>
		public string Category { get; set; }

		/// <summary>
		/// list of all the flash cards in this deck
		/// </summary>
		public List<FlashCard> Cards { get; private set; }

		#endregion //Properties

		#region Methods

		/// <summary>
		/// default construictor
		/// </summary>
		public Deck() : base("FlashCards.Deck")
		{
			Cards = new List<FlashCard>();
		}

		/// <summary>
		/// construcgtor with filename
		/// </summary>
		/// <param name="filename"></param>
		public Deck(string filename)
			: this()
		{
			XmlFilename = new Filename(filename);
		}

		/// <summary>
		/// Get a question and answers, with a list of possible incorrect answers.
		/// </summary>
		/// <param name="correctAnswer"></param>
		/// <param name="wrongAnswers"></param>
		public void GetQuestion(Random rand, out string question, out string correctAnswer, out List<string> wrongAnswers)
		{
			//grab a random flash card to be the question
			int index = rand.Next(Cards.Count);
			question = Cards[index].Word;
			correctAnswer = Cards[index].Translation;

			//add all the possible incorrect answers
			wrongAnswers = new List<string>();
			for (int i = 0; i < Cards.Count; i++)
			{
				if (index != i)
				{
					wrongAnswers.Add(Cards[i].Translation);
				}
			}
		}

		public override void ParseXmlNode(System.Xml.XmlNode xmlNode)
		{
			string name = xmlNode.Name;
			string value = xmlNode.InnerText;

			switch (name)
			{
				case "Category":
				{
					Category = value;
				}
				break;
				case "Cards":
				{
					ReadChildNodes(xmlNode, ParseCardXmlNodes);
				}
				break;
				default:
				{
					throw new Exception("unknown XML node: " + name);
				}
			}
		}

		private void ParseCardXmlNodes(XmlNode xmlNode)
		{
			//create a new flash card
			var card = new FlashCard();

			//read it in 
			XmlFileBuddy.ReadChildNodes(xmlNode, card.ParseChildNode);

			//store the card
			Cards.Add(card);
		}

		public override void WriteXmlNodes(System.Xml.XmlTextWriter xmlFile)
		{
			xmlFile.WriteStartElement("Category");
			xmlFile.WriteString(Category);
			xmlFile.WriteEndElement();

			//write out all the cards
			xmlFile.WriteStartElement("Cards");
			foreach (var card in Cards)
			{
				card.WriteXmlNodes(xmlFile);
			}
			xmlFile.WriteEndElement();
		}

		#endregion //Methods
	}
}