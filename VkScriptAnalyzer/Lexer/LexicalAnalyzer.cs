using System;
using System.Linq;
using VkScriptAnalyzer.Lexer.Mashines;

namespace VkScriptAnalyzer.Lexer
{       
    public class LexicalAnalyzer
    {
        /// <summary>
        /// Была ли проверка автоматов
        /// </summary>
        private bool wasChecked = false;
        /// <summary>
        /// Была ли лексема-разделитель
        /// </summary>
        private bool wasDividingLexem = false;

        private string input;
        /// <summary>
        /// Токен, содержащий лексему-разделитель
        /// </summary>
        private Token fastToken = null;

        private readonly char[] DIVIDING_CHARS   = { '+', '-', '/', '*', ';', ',', '(', ')', '{', '}', '<', '>', '!', '=', '.', ':' };
        private readonly char[] WHITESPACE_CHARS = { ' ', '\t', '\n', '\r' };

        private readonly MashineNumber     mashineNumber     = new MashineNumber();
        private readonly MashineIdentifier mashineIdentifier = new MashineIdentifier();
        private readonly MashineString     mashineString     = new MashineString();


        private readonly string[] KEY_WORDS =
         {
            "var",
            "if",
            "else",
            "while",
            "and",
            "or",
            "return",
        };

        private readonly string[] BOOL_DATA_TYPES =
        {
            "true",
            "false"
        };

        private readonly Machine[] PARSERS;

        public int PosNumber { get; private set; }

        public LexicalAnalyzer(string text)
        {
            input = text.TrimStart().TrimEnd();

            PARSERS = new Machine[] {
                mashineNumber,
                mashineIdentifier,
                mashineString
            };

            PosNumber = 1;
        }

        private char ParseSymbol()
        {
            char symbol = input[0];
            input = input.Remove(0, 1);

            return symbol;
        }

        private char? CheckNextSymbol()
        {
            if (input.Length == 0)
                return null;
            else
                return input[0];
        }

        private Token CheckParsers()
        {
            Token token = new Token();
            bool find = false;
            string value = null;

            var tempParsers = PARSERS
                        .Where(p => p.lexValue != string.Empty && p.lexValue != null)
                        .OrderByDescending(p => p.lexValue.Length)
                        .ToArray();

            foreach (Machine parser in tempParsers)
            {
                value = parser.lexValue;

                if (parser.IsEnd())
                {
                    if (parser.type == TokenType.Identifier && KEY_WORDS.Contains(value))
                        token.Type = TokenType.KeyWord;
                    else if (parser.type == TokenType.Identifier && BOOL_DATA_TYPES.Contains(value))
                        token.Type = TokenType.BoolDataType;
                    else
                        token.Type = parser.type;

                    token.Value = value;
                    token.PosNumber = PosNumber;

                    find = true;

                    break;
                }
            }

            ResetParsers();

            if (!find)
            {
                token.Type = TokenType.Unknown;
                token.Value = value;
            }

            if(token.Type == TokenType.String)
            {
                token.Value = token.Value.Remove(0, 1);
                token.Value = token.Value.Remove(token.Value.Length - 1, 1);
            }

            return token;
        }

        private Token CheckDoubleDividingChars(char firstDividingChar, bool parseNotDividingLexem)
        {
            var secondDividingChar = CheckNextSymbol();

            if (firstDividingChar == '=')
            {
                if (secondDividingChar == '=')
                {
                    ParseSymbol();
                    var token = new Token()
                    {
                        Type = TokenType.Equal,
                        Value = "==",
                        PosNumber = PosNumber
                    };

                    if (parseNotDividingLexem)
                    {
                        fastToken = token;
                        return CheckParsers();
                    }
                    else
                    {
                        return token;
                    }
                }
            }
            else if (firstDividingChar == '!')
            {
                if (secondDividingChar == '=')
                {
                    ParseSymbol();
                    var token = new Token()
                    {
                        Type = TokenType.NonEqual,
                        Value = "!=",
                        PosNumber = PosNumber
                    };

                    if (parseNotDividingLexem)
                    {
                        fastToken = token;

                        return CheckParsers();
                    }
                    else
                    {
                        return token;
                    }
                }
            }

            return null;
        }

        private void ResetParsers()
        {
            foreach (Machine parser in PARSERS)
            {
                parser.Reset();
            }
        }

        public Token GetToken()
        {
            if (fastToken != null)
            {
                var tmp = fastToken;
                fastToken = null;
                return tmp;
            }

            if (input.Length == 0)
            {
                if (fastToken != null)
                    return fastToken;
                else
                    return null;
            }

            if (wasChecked)
                wasChecked = false;

            bool parseNotDividingLexem = false;
            bool isWhiteSpace = false;
            while (input.Length > 0)
            {
                char symbol = ParseSymbol();

                if (WHITESPACE_CHARS.Contains(symbol))
                {
                    if(symbol == '\n')
                        PosNumber++;

                    isWhiteSpace = true;
                }

                bool dividingLexem = false;
                var type = TokenType.Unknown;

                if (DIVIDING_CHARS.Contains(symbol))
                {
                    var doubleDividingToken = CheckDoubleDividingChars(symbol, parseNotDividingLexem);
                    if (doubleDividingToken != null)
                        return doubleDividingToken;

                    dividingLexem = true;

                    type = (TokenType)symbol;
                }

                if (dividingLexem)
                {
                    if(symbol == '.')
                    {
                        // если начали разбирать число, то точка не будет символом-"разделителем"
                        bool isError = mashineNumber.InError();

                        if (!isError)
                        {
                            foreach (Machine parser in PARSERS)
                            {
                                parser.Parse(symbol);
                            }

                            continue;
                        }
                    }

                    if (wasDividingLexem || !parseNotDividingLexem)
                    {
                        return new Token()
                        {
                            Type = type,
                            Value = Convert.ToString(symbol),
                            PosNumber = PosNumber
                        };
                    }

                    wasDividingLexem = true;

                    fastToken = new Token()
                    {
                        Type = type,
                        Value = Convert.ToString(symbol),
                        PosNumber = PosNumber
                    };

                    if (parseNotDividingLexem)
                    {
                        return CheckParsers();
                    }
                }
                else if (isWhiteSpace)
                {
                    isWhiteSpace = false; 

                    if (parseNotDividingLexem)
                    {
                        wasChecked = true;
                        return CheckParsers();
                    }
                }
                else
                {
                    wasDividingLexem = false;
                    parseNotDividingLexem = true;

                    foreach (Machine parser in PARSERS)
                    {
                        parser.Parse(symbol);
                    }
                }
            }

            if(!isWhiteSpace)
                return CheckParsers();

            return null;
        }
    }
}
