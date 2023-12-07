
using System;
using System.Reflection.Metadata;
using System.Threading;

string strFileName = "TestData.txt";


// Card Pack
Dictionary<char, Card> dctCardPack = new Dictionary<char, Card>();
List<Hand> lstHands = new List<Hand>();


string strCardLabels = "A, K, Q, T, 9, 8, 7, 6, 5, 4, 3, 2, J";

// Split the string by commas and remove any leading or trailing spaces
string[] arrCardValuesArray = strCardLabels.Split(',').Select(s => s.Trim()).ToArray();

// Convert the array of strings to an array of chars
char[] charArray = arrCardValuesArray.Select(s => s[0]).ToArray();

int iValue = 13;
foreach (char cLabel in charArray)
{
    Card objCard = new Card(cLabel, iValue);
    dctCardPack.Add(cLabel, objCard);
    iValue--;

}

// Open the file with a StreamReader
using (StreamReader reader = new StreamReader(strFileName))
{
    string strLine;

    // Read lines one at a time until the end of the file
    while ((strLine = reader.ReadLine()) != null)
    {
        string strCards = strLine.Substring(0, Math.Min(5, strLine.Length));

        // Extract the remaining part as a number
        string strBid = strLine.Substring(5).Trim();
        int iBid = int.Parse(strBid);

        Helper.HandType enumHandType = Helper.AnalyzeHand(strCards);

        if(strCards.Contains("J"))
        {
            int iNumberOfJokers = strCards.Count(c => c == 'J');
            string[] arrResults = Helper.GenerateCombinations("AKQT98765432J", iNumberOfJokers);

            string strNewhand = strCards.Replace("J", "").Trim();

            for (int iLoop = 0; iLoop < arrResults.Length; iLoop++)
            {
                Helper.HandType enumNewHandType = Helper.AnalyzeHand(strNewhand + arrResults[iLoop]);

                if ((int)enumNewHandType > (int)enumHandType)
                {
                    enumHandType = enumNewHandType;
                }
            }
        }

        //32T3K is still the only one pair; it doesn't contain any jokers, so its strength doesn't increase.
        //KK677 is now the only two pair, making it the second-weakest hand.
        //T55J5, KTJJT, and QQQJA are now all four of a kind! T55J5 gets rank 3, QQQJA gets rank 4, and KTJJT gets rank 5.

        List<int> lstCardValues = new List<int>();
        foreach (char cCard in strCards)
        {
            lstCardValues.Add(dctCardPack[cCard].CardValue);
        }

        Hand objhand = new Hand(strCards, iBid, enumHandType, lstCardValues);
        lstHands.Add(objhand);
    }
}

List<Hand> lstSortedHands = lstHands.OrderBy(hand => hand.HandType)
    .ThenBy(hand => hand.ListCardValues[0])
    .ThenBy(hand => hand.ListCardValues[1])
    .ThenBy(hand => hand.ListCardValues[2])
    .ThenBy(hand => hand.ListCardValues[3])
    .ThenBy(hand => hand.ListCardValues[4])
    .ToList();


// Add the Rank
int iRank = 0;
foreach(Hand objHand in lstSortedHands)
{
    iRank++;
    objHand.Rank = iRank;
}


double iTotalWinnings = 0;

foreach (Hand objHand in lstSortedHands)
{
    iTotalWinnings += objHand.getWinnings();
}


Console.WriteLine("Part 1 Total winnings :" + iTotalWinnings);

// A hand consists of five cards labeled one of A, K, Q, J, T, 9, 8, 7, 6, 5, 4, 3, or 2
// A highest, 2 is lowest

//Five of a kind, where all five cards have the same label: AAAAA
//Four of a kind, where four cards have the same label and one card has a different label: AA8AA
//Full house, where three cards have the same label, and the remaining two cards share a different label: 23332
//Three of a kind, where three cards have the same label, and the remaining two cards are each different from any other card in the hand: TTT98
//Two pair, where two cards share one label, two other cards share a second label, and the remaining card has a third label: 23432
//One pair, where two cards share one label, and the other three cards have a different label from the pair and each other: A23A4
//High card, where all cards' labels are distinct: 23456

//HandType objFiveOfAKind =new HandType ("Five of a kind", 5);
//HandType objFourOfAKind = new HandType("Four of a kind", 4);

public class Hand
{
    public string Cards;
    public Helper.HandType HandType;
    public List<int> ListCardValues = new List<int>();
    public int Bid = 0;
    public int Rank = 0;

    public Hand(string strHand, int iBid, Helper.HandType iHandType, List<int> lstCardValues)
    {
        Cards = strHand;
        HandType = iHandType;
        ListCardValues = lstCardValues;
        Bid = iBid;
    }

    public double getWinnings()
    {
        return Rank * Bid;
    }
}


public static class Helper
{
    public static string[] GenerateCombinations(string strValues, int numberOfJs)
    {
        // Calculate the total number of combinations
        int iTotalCombinations = (int)Math.Pow(strValues.Length, numberOfJs);

        // Initialize the result array
        string[] combinations = new string[iTotalCombinations];

        // Generate all combinations
        for (int i = 0; i < iTotalCombinations; i++)
        {
            char[] combinationChars = new char[numberOfJs];

            // Calculate indices for each 'J'
            for (int j = 0; j < numberOfJs; j++)
            {
                int index = (i / (int)Math.Pow(strValues.Length, j)) % strValues.Length;
                combinationChars[j] = strValues[index];
            }

            combinations[i] = new string(combinationChars);
        }

        return combinations;
    }


    public enum HandType
    {
        HighCard,
        OnePair,
        TwoPair,
        ThreeOfAKind,
        FullHouse,
        FourOfAKind,
        FiveOfAKind        
    }

    public static HandType AnalyzeHand(string hand)
    {
        string sortedHand = new string(hand.Where(char.IsLetterOrDigit).OrderBy(c => c).ToArray());

        if (IsFiveOfAKind(sortedHand))
            return HandType.FiveOfAKind;
        if (IsFourOfAKind(sortedHand))
            return HandType.FourOfAKind;
        if (IsFullHouse(sortedHand))
            return HandType.FullHouse;
        if (IsThreeOfAKind(sortedHand))
            return HandType.ThreeOfAKind;
        if (IsTwoPair(sortedHand))
            return HandType.TwoPair;
        if (IsOnePair(sortedHand))
            return HandType.OnePair;

        return HandType.HighCard;
    }

    private static bool IsFiveOfAKind(string hand)
    {
        var groups = hand.GroupBy(c => c);
        return hand.Length == 5 && groups.Any(g => g.Count() == 5);
    }

    private static bool IsFourOfAKind(string hand)
    {
        var groups = hand.GroupBy(c => c);
        return hand.Length == 5 && groups.Any(g => g.Count() == 4);
    }

    private static bool IsFullHouse(string hand) => hand.Length == 5 && hand.Distinct().Count() == 2 && hand.GroupBy(c => c).Any(g => g.Count() == 3);

    private static bool IsThreeOfAKind(string hand)
    {
        var groups = hand.GroupBy(c => c);
        return hand.Length == 5 && groups.Any(g => g.Count() == 3);
    }

    private static bool IsTwoPair(string hand)
    {
        var groups = hand.GroupBy(c => c);
        int pairCount = groups.Count(g => g.Count() == 2);

        return hand.Length == 5 && pairCount == 2;
    }

    private static bool IsOnePair(string hand)
    {
        var groups = hand.GroupBy(c => c);
        int pairCount = groups.Count(g => g.Count() == 2);

        return hand.Length == 5 && pairCount == 1;
    }
}


//public class HandType
//{
//    public string Description = string.Empty;
//    public int Value;

//    public HandType(string strDescription, int iValue)
//    {
//        Description = strDescription;
//        Value = iValue;
//    }
//}

public class Card
{
    public char CardLabel;
    public int CardValue = 0;

    public Card(char cLabel, int iValue)
    {
        CardLabel = cLabel;
        CardValue = iValue;
    }
}

