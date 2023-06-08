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
        private bool _wasChecked = false;
        /// <summary>
        /// Была ли лексема-разделитель
        /// </summary>
        private bool _wasDividingLexem = false;

        private string _input;
        /// <summary>
        /// Токен, содержащий лексему-разделитель
        /// </summary>
        private Token _fastToken = null;

        private readonly char[] _dividingChars   = { '+', '-', '/', '*', ';', ',', '(', ')', '{', '}', '<', '>', '!', '=', '.', ':' };
        private readonly char[] _whitespaceChars = { ' ', '\t', '\n', '\r' };

        private readonly MashineNumber     _mashineNumber     = new MashineNumber();
        private readonly MashineIdentifier _mashineIdentifier = new MashineIdentifier();
        private readonly MashineString     _mashineString     = new MashineString();


        private readonly string[] _keyWords =
         {
            "var",
            "if",
            "else",
            "while",
            "and",
            "or",
            "return",
        };

        private readonly string[] _boolDataTypes =
        {
            "true",
            "false"
        };

        private readonly Machine[] _parsers;

        public int PosNumber { get; private set; }

        public LexicalAnalyzer(string text)
        {
            _input = text.TrimStart().TrimEnd();

            _parsers = new Machine[] {
                _mashineNumber,
                _mashineIdentifier,
                _mashineString
            };

            PosNumber = 1;
        }

        private char ParseSymbol()
        {
            char symbol = _input[0];
            _input = _input.Remove(0, 1);

            return symbol;
        }

        private char? CheckNextSymbol()
        {
            if (_input.Length == 0)
            {
                return null;
            }

            return _input[0];
        }

        private Token CheckParsers()
        {
            Token token = new Token();
            bool find = false;
            string value = null;

            var temp_parsers = _parsers
                        .Where(p => p.LexValue != string.Empty && p.LexValue != null)
                        .OrderByDescending(p => p.LexValue.Length)
                        .ToArray();

            foreach (Machine parser in temp_parsers)
            {
                value = parser.LexValue;

                if (parser.IsEnd())
                {
                    if (parser.Type == TokenType.Identifier && _keyWords.Contains(value))
                    {
                        token.Type = TokenType.KeyWord;
                    } else if (parser.Type == TokenType.Identifier && _boolDataTypes.Contains(value))
                    {
                        token.Type = TokenType.BoolDataType;
                    } else
                    {
                        token.Type = parser.Type;
                    }

                    token.Value = value;
                    token.Pos = PosNumber;

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
            var second_dividing_char = CheckNextSymbol();

            if (firstDividingChar == '=')
            {
                if (second_dividing_char == '=')
                {
                    ParseSymbol();
                    var token = new Token()
                    {
                        Type = TokenType.Equal,
                        Value = "==",
                        Pos = PosNumber
                    };

                    if (parseNotDividingLexem)
                    {
                        _fastToken = token;
                        return CheckParsers();
                    }

                    return token;
                }
            }
            else if (firstDividingChar == '!')
            {
                if (second_dividing_char == '=')
                {
                    ParseSymbol();
                    var token = new Token()
                    {
                        Type = TokenType.NonEqual,
                        Value = "!=",
                        Pos = PosNumber
                    };

                    if (parseNotDividingLexem)
                    {
                        _fastToken = token;

                        return CheckParsers();
                    }

                    return token;
                }
            }

            return null;
        }

        private void ResetParsers()
        {
            foreach (Machine parser in _parsers)
            {
                parser.Reset();
            }
        }

        public Token GetToken()
        {
            if (_fastToken != null)
            {
                var tmp = _fastToken;
                _fastToken = null;
                return tmp;
            }

            if (_input.Length == 0)
            {
                if (_fastToken != null)
                {
                    return _fastToken;
                }

                return null;
            }

            if (_wasChecked)
            {
                _wasChecked = false;
            }

            bool parse_not_dividing_lexem = false;
            bool is_white_space = false;
            while (_input.Length > 0)
            {
                char symbol = ParseSymbol();

                if (_whitespaceChars.Contains(symbol))
                {
                    if(symbol == '\n')
                    {
                        PosNumber++;
                    }

                    is_white_space = true;
                }

                bool dividing_lexem = false;
                var type = TokenType.Unknown;

                if (_dividingChars.Contains(symbol))
                {
                    var double_dividing_token = CheckDoubleDividingChars(symbol, parse_not_dividing_lexem);
                    if (double_dividing_token != null)
                    {
                        return double_dividing_token;
                    }

                    dividing_lexem = true;

                    type = (TokenType)symbol;
                }

                if (dividing_lexem)
                {
                    if(symbol == '.')
                    {
                        // если начали разбирать число, то точка не будет символом-"разделителем"
                        bool is_error = _mashineNumber.InError();

                        if (!is_error)
                        {
                            foreach (Machine parser in _parsers)
                            {
                                parser.Parse(symbol);
                            }

                            continue;
                        }
                    }

                    if (_wasDividingLexem || !parse_not_dividing_lexem)
                    {
                        return new Token()
                        {
                            Type = type,
                            Value = Convert.ToString(symbol),
                            Pos = PosNumber
                        };
                    }

                    _wasDividingLexem = true;

                    _fastToken = new Token()
                    {
                        Type = type,
                        Value = Convert.ToString(symbol),
                        Pos = PosNumber
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
                        _wasChecked = true;
                        return CheckParsers();
                    }
                }
                else
                {
                    _wasDividingLexem = false;
                    parse_not_dividing_lexem = true;

                    foreach (Machine parser in _parsers)
                    {
                        parser.Parse(symbol);
                    }
                }
            }

            if(!is_white_space)
            {
                return CheckParsers();
            }

            return null;
        }
    }
}