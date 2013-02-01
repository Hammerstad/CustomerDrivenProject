using System.Globalization;
using Microsoft.Speech.Recognition;

namespace WindowsGame3.Controller
{
    class CustomGrammar
    {
        /// <summary>
        /// Static function which returns our vocabulary/dictionary/grammar.
        /// </summary
        public static Grammar CreateGrammar(CultureInfo culture)
        {
            
            //TO ADD COMMANDS FOLLOW THE SAME PATTERN AS THE OTHERS:
            //dictionary.Add("command_to_be_recognized");
            // ENTEN ELLER: Choices selectPlaneChoices = new Choices(new GrammarBuilder[] { selectPhrase, digitsPhrase });

            #region selectPlane

            //NUMBERS
            Choices digits = new Choices();
            digits.Add("zero");
            digits.Add("one");
            digits.Add("two");
            digits.Add("three");
            digits.Add("four");
            digits.Add("five");
            digits.Add("six");
            digits.Add("seven");
            digits.Add("eight");
            digits.Add("nine");
            digits.Add("ten");
            digits.Add("eleven");
            digits.Add("twelve");
            digits.Add("thirteen");
            digits.Add("fourteen");
            digits.Add("fifteen");

            GrammarBuilder selectPlanes = new GrammarBuilder { Culture = culture };
            selectPlanes.Append("select");
            selectPlanes.Append("plane");
            selectPlanes.Append(digits);
            #endregion
            
            #region alterHeading and alterSpeed

            //NUMBERS

            //This choices contains the numbers 0,10,20,30,40,50,60,70,80,90
            Choices numbers = new Choices();
            numbers.Add("zero");
            numbers.Add("ten");
            numbers.Add("twenty");
            numbers.Add("thirty");
            numbers.Add("fourty");
            numbers.Add("fifty");
            numbers.Add("sixty");
            numbers.Add("seventy");
            numbers.Add("eighty");
            numbers.Add("ninety");
            numbers.Add("hundred");
            //Hundreds
            numbers.Add("hundred");
            numbers.Add("hundred and ten");
            numbers.Add("hundred and twenty");
            numbers.Add("hundred and twenty");
            numbers.Add("hundred and thirty");
            numbers.Add("hundred and fourty");
            numbers.Add("hundred and fifty");
            numbers.Add("hundred and sixty");
            numbers.Add("hundred and seventy");
            numbers.Add("hundred and eighty");
            numbers.Add("hundred and ninety");
            numbers.Add("hundred and twenty");
            //Because we have to
            numbers.Add("onehundred");
            numbers.Add("onehundred and ten");
            numbers.Add("onehundred and twenty");
            numbers.Add("onehundred and thirty");
            numbers.Add("onehundred and fourty");
            numbers.Add("onehundred and fifty");
            numbers.Add("onehundred and sixty");
            numbers.Add("onehundred and seventy");
            numbers.Add("onehundred and eighty");
            numbers.Add("onehundred and ninety");
            //Two hundreds
            numbers.Add("twohundred");
            numbers.Add("twohundred and ten");
            numbers.Add("twohundred and twenty");
            numbers.Add("twohundred and thirty");
            numbers.Add("twohundred and fourty");
            numbers.Add("twohundred and fifty");
            numbers.Add("twohundred and sixty");
            numbers.Add("twohundred and seventy");
            numbers.Add("twohundred and eighty");
            numbers.Add("twohundred and ninety");
            //Three hundreds
            numbers.Add("threehundred");
            numbers.Add("threehundred and ten");
            numbers.Add("threehundred and twenty");
            numbers.Add("threehundred and thirty");
            numbers.Add("threehundred and fourty");
            numbers.Add("threehundred and fifty");
            numbers.Add("threehundred and sixty");
            numbers.Add("threehundred and seventy");
            numbers.Add("threehundred and eighty");
            numbers.Add("threehundred and ninety");

            Choices heading = new Choices(new string[]{"south", "north", "east", "west"});
            Choices direction = new Choices(heading, numbers);

            GrammarBuilder alterHeading = new GrammarBuilder { Culture = culture };
            alterHeading.Append("alter");
            alterHeading.Append("heading");
            alterHeading.Append(direction);
            #endregion

            #region alterSpeed
            Choices hundreds = new Choices();
            hundreds.Add("twohundred");
            hundreds.Add("threehundred");
            hundreds.Add("fourhundred");
            hundreds.Add("fivehundred");
            hundreds.Add("sixhundred");
            hundreds.Add("sevenhundred");
            hundreds.Add("eighthundred");
            hundreds.Add("ninehundred");

            GrammarBuilder alterSpeed = new GrammarBuilder { Culture = culture };
            alterSpeed.Append("alter");
            alterSpeed.Append("speed");
            alterSpeed.Append(hundreds);
            #endregion

            #region alterPitch
            GrammarBuilder alterPitch = new GrammarBuilder { Culture = culture };
            alterPitch.Append("alter");
            alterPitch.Append("pitch");
            GrammarBuilder minusNumbers = new GrammarBuilder { Culture = culture };
            minusNumbers.Append("minus");
            minusNumbers.Append(digits);
            Choices minusChoices = new Choices(minusNumbers, digits);
            alterPitch.Append(minusChoices);
            #endregion

            #region toggleCommands
            GrammarBuilder toggleCommands = new GrammarBuilder { Culture = culture };
            toggleCommands.Append("toggle");
            Choices toggleChoices = new Choices(new string[] {"names", "coordinates", "lines", "voice", "help"});
            toggleCommands.Append(toggleChoices);
            #endregion

            #region oneWordCommands
            GrammarBuilder oneWordCommands = new GrammarBuilder { Culture = culture };
            Choices oneWordCommandChoices = new Choices(new string[] { "go too", "follow", "unfollow", "calibrate", "exit", "deselect" });
            oneWordCommands.Append(oneWordCommandChoices);
            #endregion

            Choices allTheCommands = new Choices(new GrammarBuilder[] { selectPlanes, alterHeading, alterSpeed, alterPitch, toggleCommands, oneWordCommands });

            GrammarBuilder grammarBuilder = new GrammarBuilder{Culture = culture};
            grammarBuilder.Append(allTheCommands);
            Grammar grammar = new Grammar(grammarBuilder);

            return grammar;
        }
    }
}
