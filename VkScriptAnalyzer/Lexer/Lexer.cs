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

        private readonly char[] DIVIDING_CHARS   = { '+', '-', '/', '*', ';', '(', ')', '{', '}', '<', '>' };
        private readonly char[] WHITESPACE_CHARS = { ' ', '\t', '\n', '\r' }; 
        
        private readonly string[] KEY_WORDS =
         {
            "var",
            "if",
            "else",
            "while",
            "return",
        };

        private readonly string[] DATA_TYPES =
        {
            "true",
            "false",
        };

        public LexicalAnalyzer(string text)
        {
            input = text.TrimStart().TrimEnd();
        }

        private readonly Machine[] PARSERS = {
            new MashineNumber(),
            new MashineIdentifier(),
            new MashineNotEqual(),
            new MashineEqual(),

            #region Not used machines
            /*
            new MashineAssign(),             
			new MashineString(),
            new MashineOneSymbol()*/
            #endregion
        };

        char Parse_Symbol()
        {
            char symbol = input[0];
            input = input.Remove(0, 1);

            return symbol;
        }

        Token Check_Parsers()
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
                        token.type = TokenType.DataType;
                    else
                        token.type = parser.type;

                    token.value = value;
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
                char symbol = Parse_Symbol();

                if (WHITESPACE_CHARS.Contains(symbol))
                {
                    is_white_space = true;
                }

                bool dividing_lexem = false;
                var type = TokenType.Unknown;

                if (DIVIDING_CHARS.Contains(symbol))
                {
                    dividing_lexem = true;

                    type = (TokenType)symbol;
                }

                if (dividing_lexem)
                {
                    if (was_dividing_lexem || !parse_not_dividing_lexem)
                    {
                        return new Token()
                        {
                            type = type,
                            value = Convert.ToString(symbol)
                        };
                    }

                    was_dividing_lexem = true;

                    fast_token = new Token()
                    {
                        type = type,
                        value = Convert.ToString(symbol)
                    };

                    if (parse_not_dividing_lexem)
                    {
                        return Check_Parsers();
                    }
                }
                else if (is_white_space)
                {
                    is_white_space = false; 

                    if (parse_not_dividing_lexem)
                    {
                        was_checked = true;
                        return Check_Parsers();
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
                return Check_Parsers();

            return null;
        }
    }
}