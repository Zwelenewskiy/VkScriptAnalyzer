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
        private bool was_checked = false;
        /// <summary>
        /// Была ли лексема-разделитель
        /// </summary>
        private bool was_dividing_lexem = false;

        private string input;
        /// <summary>
        /// Токен, содержащий лексему-разделитель
        /// </summary>
        private Token fast_token = null;

        private readonly char[] DIVIDING_CHARS   = { '+', '-', '/', '*', ';', ',', '(', ')', '{', '}', '<', '>', '!', '=', '.', ':' };
        private readonly char[] WHITESPACE_CHARS = { ' ', '\t', '\n', '\r' };

        private readonly MashineNumber     MashineNumber     = new MashineNumber();
        private readonly MashineIdentifier MashineIdentifier = new MashineIdentifier();
        private readonly MashineString     MashineString     = new MashineString();


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

        private readonly string[] DATA_TYPES =
        {
            "true",
            "false",
            "null",
        };

        private readonly Machine[] PARSERS;

        public int PosNumber { get; private set; }

        public LexicalAnalyzer(string text)
        {
            input = text.TrimStart().TrimEnd();

            PARSERS = new Machine[] {
                MashineNumber,
                MashineIdentifier,
                MashineString
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

            var temp_parsers = PARSERS
                        .Where(p => p.lex_value != string.Empty && p.lex_value != null)
                        .OrderByDescending(p => p.lex_value.Length)
                        .ToArray();

            foreach (Machine parser in temp_parsers)
            {
                value = parser.lex_value;

                if (parser.IsEnd())
                {
                    if (parser.type == TokenType.Identifier && KEY_WORDS.Contains(value))
                        token.type = TokenType.KeyWord;
                    else if (parser.type == TokenType.Identifier && DATA_TYPES.Contains(value))
                        token.type = TokenType.BoolDataType;
                    else
                        token.type = parser.type;

                    token.value = value;
                    token.pos = PosNumber;

                    find = true;

                    break;
                }
            }

            ResetParsers();

            if (!find)
            {
                token.type = TokenType.Unknown;
                token.value = value;
            }

            return token;
        }

        private Token CheckDoubleDividingChars(char first_dividing_char, bool parse_not_dividing_lexem)
        {
            var second_dividing_char = CheckNextSymbol();

            if (first_dividing_char == '=')
            {
                if (second_dividing_char == '=')
                {
                    ParseSymbol();
                    var token = new Token()
                    {
                        type = TokenType.Equal,
                        value = "==",
                        pos = PosNumber
                    };

                    if (parse_not_dividing_lexem)
                    {
                        fast_token = token;
                        return CheckParsers();
                    }
                    else
                    {
                        return token;
                    }
                }
            }
            else if (first_dividing_char == '!')
            {
                if (second_dividing_char == '=')
                {
                    ParseSymbol();
                    var token = new Token()
                    {
                        type = TokenType.NonEqual,
                        value = "!=",
                        pos = PosNumber
                    };

                    if (parse_not_dividing_lexem)
                    {
                        fast_token = token;

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
            if (fast_token != null)
            {
                var tmp = fast_token;
                fast_token = null;
                return tmp;
            }

            if (input.Length == 0)
            {
                if (fast_token != null)
                    return fast_token;
                else
                    return null;
            }

            if (was_checked)
                was_checked = false;

            bool parse_not_dividing_lexem = false;
            bool is_white_space = false;
            while (input.Length > 0)
            {
                char symbol = ParseSymbol();

                if (WHITESPACE_CHARS.Contains(symbol))
                {
                    if(symbol == '\n')
                        PosNumber++;

                    is_white_space = true;
                }

                bool dividing_lexem = false;
                var type = TokenType.Unknown;

                if (DIVIDING_CHARS.Contains(symbol))
                {
                    var double_dividing_token = CheckDoubleDividingChars(symbol, parse_not_dividing_lexem);
                    if (double_dividing_token != null)
                        return double_dividing_token;

                    dividing_lexem = true;

                    type = (TokenType)symbol;
                }

                if (dividing_lexem)
                {
                    if(symbol == '.')
                    {
                        // если начали разбирать число, то точка не будет символом-"разделителем"
                        bool is_error = MashineNumber.InError();

                        if (!is_error)
                        {
                            foreach (Machine parser in PARSERS)
                            {
                                parser.Parse(symbol);
                            }

                            continue;
                        }
                    }

                    if (was_dividing_lexem || !parse_not_dividing_lexem)
                    {
                        return new Token()
                        {
                            type = type,
                            value = Convert.ToString(symbol),
                            pos = PosNumber
                        };
                    }

                    was_dividing_lexem = true;

                    fast_token = new Token()
                    {
                        type = type,
                        value = Convert.ToString(symbol),
                        pos = PosNumber
                    };

                    if (parse_not_dividing_lexem)
                    {
                        return CheckParsers();
                    }
                }
                else if (is_white_space)
                {
                    is_white_space = false; 

                    if (parse_not_dividing_lexem)
                    {
                        was_checked = true;
                        return CheckParsers();
                    }
                }
                else
                {
                    was_dividing_lexem = false;
                    parse_not_dividing_lexem = true;

                    foreach (Machine parser in PARSERS)
                    {
                        parser.Parse(symbol);
                    }
                }
            }

            if(!is_white_space)
                return CheckParsers();

            return null;
        }
    }
}