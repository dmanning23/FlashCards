using FilenameBuddy;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using TetrisRandomizer;
using XmlBuddy;

namespace FlashCards.Core
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

		/// <summary>
		/// the random bag we are gonna use to pull cards out
		/// </summary>
		private readonly RandomBag questionRand;

		private Random translationRand = new Random();

		public string Language1 { get; set; }
		public string Language2 { get; set; }

		#endregion //Properties

		#region Methods

		/// <summary>
		/// default construictor
		/// </summary>
		public Deck() : base("FlashCards.Deck")
		{
			Cards = new List<FlashCard>();
			questionRand = new RandomBag(10);
		}

		/// <summary>
		/// construcgtor with string
		/// </summary>
		/// <param name="filename"></param>
		public Deck(string filename)
			: this()
		{
			Filename = new Filename(filename);
		}

		/// <summary>
		/// construcgtor with filename
		/// </summary>
		/// <param name="filename"></param>
		public Deck(Filename filename)
			: this()
		{
			Filename = filename;
		}

		/// <summary>
		/// Get a question and answers, with a list of possible incorrect answers.
		/// </summary>
		/// <param name="question"></param>
		/// <param name="correctAnswer"></param>
		/// <param name="wrongAnswers"></param>
		public void GetQuestion(out FlashCard questionCard, out Translation correctTranslation, out List<FlashCard> wrongQuestionCards, out List<Translation> wrongTranslations)
		{
			//grab a random flash card to be the question
			var cardIndex = questionRand.Next();
			questionCard = Cards[cardIndex];

			//Get the correct answer
			var correctIndex = translationRand.Next(questionCard.Translations.Count);
			var correctAnswer = questionCard.Translations[correctIndex];
			correctTranslation = correctAnswer;

			//add all the possible incorrect answers
			wrongQuestionCards = new List<FlashCard>();
			wrongTranslations = new List<Translation>();
			for (int i = 0; i < Cards.Count; i++)
			{
				if (cardIndex != i)
				{
					//Get the translation from this card that matches the correct answer
					var wrongCard = Cards[i];
					var wrongAnswer = wrongCard.Translations.FirstOrDefault(x => x.Language == correctAnswer.Language);
					if (null != wrongAnswer)
					{
						wrongQuestionCards.Add(wrongCard);
						wrongTranslations.Add(wrongAnswer);
					}
				}
			}
		}

		/// <summary>
		/// You can read in multiple decks and add them together to do comprehension lessons.
		/// </summary>
		/// <param name="otherDeck">A deck of cards to add to this one</param>
		public void AddDeck(Deck otherDeck)
		{
			//Add the cards
			Cards.AddRange(otherDeck.Cards);

			//Add the category
			Category += ", " + otherDeck.Category;

			//make sure the random bag will pull the new cards too
			questionRand.MaxNum = Cards.Count;
		}

		#endregion //Methods

		#region File Parsing

		public override void ReadXmlFile(ContentManager content = null)
		{
			base.ReadXmlFile(content);
			questionRand.MaxNum = Cards.Count;
		}

		public override void ParseXmlNode(XmlNode xmlNode)
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
				case "FlashCards":
				{
					ReadChildNodes(xmlNode, ParseFlashCardXmlNodes);
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

		private void ParseFlashCardXmlNodes(XmlNode xmlNode)
		{
			//create a new flash card
			var card = new FlashCard()
			{
				Language1 = this.Language1,
				Language2 = this.Language2
			};

			//read it in 
			XmlFileBuddy.ReadChildNodes(xmlNode, card.ParseCardXmlNodes);

			//store the card
			Cards.Add(card);
		}

		private void ParseCardXmlNodes(XmlNode xmlNode)
		{
			//create a new flash card
			var card = new FlashCard()
			{
				Language1 = this.Language1,
				Language2 = this.Language2
			};

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

		#endregion //File Parsing
	}
}