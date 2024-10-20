using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Willow.ExpressionParser
{
    public partial class Parser
    {
        /// <summary>
        /// Characters that can be quoted inside a string literal by adding a backslash
        /// </summary>
        private static readonly char[] quotableCharacters = new[] { '\"', '\'', '\\' };

        /// <summary>
        /// Chars allowed to create an Identifier Token. Includes special unit chars
        /// </summary>
        private static readonly char[] allowedIdentifierCharacters = new[] { '%', '$', '_', '°', '€' };

        private static IEnumerable<Token> Scan(string input, int index)
        {
            if (string.IsNullOrEmpty(input)) yield break;
            while (index < input.Length)
            {
                if (input[index] == ' ')
                {
                    index++;
                }
                else if (input[index] == '(')
                {
                    yield return Token.LParen;
                    index++;
                }
                else if (input[index] == ')')
                {
                    yield return Token.RParen;
                    index++;
                }

                // Using [] for SQL style variable names instead
                //else if (input[index] == '[')
                //{
                //    yield return Token.LSquareParen;
                //    index++;
                //}
                //else if (input[index] == ']')
                //{
                //    yield return Token.RSquareParen;
                //    index++;
                //}
                else if (input[index] == '{')
                {
                    yield return Token.LCurlyParen;
                    index++;
                }
                else if (input[index] == '}')
                {
                    yield return Token.RCurlyParen;
                    index++;
                }
                else if (input[index] == ',')
                {
                    yield return Token.Comma;
                    index++;
                }
                else if (input[index] == ';')
                {
                    yield return Token.SemiColon;
                    index++;
                }
                else if (input[index] == '&')
                {
                    // Allow double & too for the programmers out there
                    if (index + 1 < input.Length && input[index + 1] == '&')
                    {
                        yield return Token.And;
                        index++;
                    }
                    else
                    {
                        yield return Token.And;
                    }
                    index++;
                }
                else if (input[index] == '|')
                {
                    // Allow double || too for the programmers out there
                    if (index + 1 < input.Length && input[index + 1] == '|')
                    {
                        yield return Token.Or;
                        index++;
                    }
                    else
                    {
                        yield return Token.Or;
                    }
                    index++;
                }
                else if (input[index] == '\"' || input[index] == '\'')
                {
                    char quoteCharacter = input[index];
                    index++;
                    int startIndex = index;
                    bool foundEscapeCharacters = false;

                    bool isEscapeCharacter(int currentIndex, string value)
                    {
                        if (value[currentIndex] == '\\' && currentIndex + 1 < value.Length &&
                           quotableCharacters.Contains(value[currentIndex + 1]))
                        {
                            return true;
                        }

                        return false;
                    }

                    while (index < input.Length && input[index] != quoteCharacter)
                    {
                        if (isEscapeCharacter(index, input))
                        {
                            // we found excape characters, we will have to do more work
                            // after the substring call
                            foundEscapeCharacters = true;
                        }

                        index++;
                    }

                    string inputString = input.Substring(startIndex, index - startIndex);

                    if (foundEscapeCharacters)
                    {
                        int subIndex = 0;
                        //remove escape characters afterwards (which rarely happens)
                        //so we can still take advantage of a single string.Substring above
                        while (subIndex < inputString.Length)
                        {
                            if (isEscapeCharacter(subIndex, inputString))
                            {
                                inputString = inputString.Remove(subIndex, 1);
                            }

                            subIndex++;
                        }
                    }

                    if (index == input.Length)
                        throw new ParserException("Unterminated string constant");
                    index++;
                    yield return new QuotedStringToken(inputString);
                }

                // SQL style variable names instead of [ ] for arrays
                else if (input[index] == '[')
                {
                    char endCharacter = ']';
                    index++;
                    int startIndex = index;

                    while (index < input.Length && input[index] != endCharacter)
                    {
                        index++;
                    }

                    string inputString = input.Substring(startIndex, index - startIndex);

                    if (index == input.Length)
                        throw new ParserException("Unterminated SQL-style variable name");
                    index++;
                    yield return new IdentifierToken(inputString);
                }
                else if (input[index] == '+')
                {
                    yield return Token.Plus;
                    index++;
                }
                else if (input[index] == '.')
                {
                    yield return Token.Dot;
                    index++;
                }
                else if (input[index] == '-')
                {
                    yield return Token.Minus;
                    index++;
                }
                else if (input[index] == '*')
                {
                    yield return Token.Multiply;
                    index++;
                }
                else if (input[index] == '/')
                {
                    yield return Token.Divide;
                    index++;
                }
                else if (input[index] == '^')
                {
                    yield return Token.Power;
                    index++;
                }
                else if (input[index] == '=')
                {
                    // Allow double equals too for the programmers out there
                    if (index + 1 < input.Length && input[index + 1] == '=')
                    {
                        yield return Token.Equal;
                        index++;
                    }
                    else
                    {
                        yield return Token.Equal;
                    }
                    index++;
                }
                else if (input[index] == '!')
                {
                    if (index + 1 < input.Length && input[index + 1] == '=')
                    {
                        yield return Token.NotEqual;
                        index++;
                    }
                    else
                    {
                        yield return Token.UnaryNot;
                    }
                    index++;
                }
                else if (input[index] == '<')
                {
                    if (index + 1 < input.Length && input[index + 1] == '=')
                    {
                        yield return Token.LessEqual;
                        index++;
                    }
                    else
                    {
                        yield return Token.Less;
                    }

                    index++;
                }
                else if (input[index] == '>')
                {
                    if (index + 1 < input.Length && input[index + 1] == '=')
                    {
                        yield return Token.GreaterEqual;
                        index++;
                    }
                    else
                    {
                        yield return Token.Greater;
                    }

                    index++;
                }
                else if (input[index] == '∈')
                {
                    yield return Token.MemberOf;
                    index++;
                }
                else if (input[index] == '∩')
                {
                    yield return Token.Intersection;
                    index++;
                }
                else if (input[index] == '∪')
                {
                    yield return Token.Union;
                    index++;
                }
                else if (char.IsLetter(input[index]) || allowedIdentifierCharacters.Contains(input[index]))
                {
                    int startIndex = index;
                    while (index < input.Length &&
                           (char.IsLetterOrDigit(input[index]) || allowedIdentifierCharacters.Contains(input[index])))
                    {
                        index++;
                    }
                    string identifier = input.Substring(startIndex, index - startIndex);

                    if (identifier == "is")
                    {
                        yield return Token.Is;
                    }
                    else
                    {
                        yield return new IdentifierToken(identifier);
                    }
                }
                else if (char.IsDigit(input[index]))
                {
                    int startIndex = index;
                    while (index < input.Length && char.IsDigit(input[index]))
                    {
                        index++;
                    }

                    if (index < input.Length && input[index] == '.')
                    {
                        index++;
                        while (index < input.Length && char.IsDigit(input[index]))
                        {
                            index++;
                        }
                    }
                    double.TryParse(input.Substring(startIndex, index - startIndex),
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out double d);

                    yield return new NumberConstantToken(d);
                }

                // Unicode LEFT DOUBLE QUOTATION MARK and RIGHT DOUBLE QUOTATION MARK
                // get a slightly more friendly error message. Common mistake when copying
                // from word processors or web pages.
                else if (input[index] == '“' || input[index] == '”')
                {
                    throw new ParserException($"Unicode double quote character '{input[index]}' at position {index} in {input} use ASCII '\"' instead");
                }
                else
                {
                    throw new ParserException($"Did not recognize character '{input[index]}' at position {index} in {input}");
                }
            }
        }
    }
}
