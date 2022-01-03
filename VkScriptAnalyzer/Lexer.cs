using System;
using System.Collections.Generic;
using System.Linq;
using VkScriptAnalyzer.Mashines;
using VkScriptAnalyzer.GlobalClasses;

namespace VkScriptAnalyzer
{       
    public class Lexer
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

        public Lexer(string text)
        {
            input = text;

            input = input.TrimStart().TrimEnd();
        }

        string[] keyWords =
        {
            "If",
            "Then",
            "Else",
            "var",
            "begin",
            "end",
            "read",
            "write",
            "round",
        };

        string[] dataTypes =
        {
            "integer",
            "double",
        };

        char[] dividing_chars = {'+', '-', '/', '*' };

        Machine[] parsers = {
            new MashineNumber(
                new Dictionary<Input_signal, Dictionary<State, State>>()
                {
                    { Input_signal.Digit,
                        new Dictionary<State, State>() {
                        {  State.S0, State.S2 },
                        {  State.S1, State.S2 },
                        {  State.S2, State.S2 },
                        {  State.S3, State.S4 },
                        {  State.S4, State.S4 }
                    } },
                    { Input_signal.Dot,
                        new Dictionary<State, State>() {
                        {  State.S0, State.S_error },
                        {  State.S1, State.S_error },
                        {  State.S2, State.S3 },
                        {  State.S3, State.S_error },
                        {  State.S4, State.S_error },
                    } },
                    { Input_signal.Comma,
                        new Dictionary<State, State>() {
                        {  State.S0, State.S_error },
                        {  State.S1, State.S_error },
                        {  State.S2, State.S3 },
                        {  State.S3, State.S_error },
                        {  State.S4, State.S_error },
                    } },
                    { Input_signal.Minus,
                        new Dictionary<State, State>() {
                        {  State.S0, State.S1 },
                        {  State.S1, State.S_error },
                        {  State.S2, State.S_error },
                        {  State.S3, State.S_error },
                        {  State.S4, State.S_error },
                    } },
                    { Input_signal.Other,
                        new Dictionary<State, State>() {
                        {  State.S0, State.S_error },
                        {  State.S1, State.S_error },
                        {  State.S2, State.S_error },
                        {  State.S3, State.S_error },
                        {  State.S4, State.S_error }
                    }
                },
            }),

            new MashineIdentifier(
                new Dictionary<Input_signal, Dictionary<State, State>>()
                {
                    { Input_signal.Digit,
                        new Dictionary<State, State>() {
                        {  State.S0, State.S_error },
                        {  State.S1, State.S1 }
                    } },
                    { Input_signal.Letter,
                        new Dictionary<State, State>() {
                        {  State.S0, State.S1 },
                        {  State.S1, State.S1 }
                    } },
                    { Input_signal.Other,
                        new Dictionary<State, State>() {
                        {  State.S0, State.S_error },
                        {  State.S1, State.S_error }
                    } },
                }),

            #region Not used machines
            /*new MashineAssign(
				new Dictionary<Input_signal, Dictionary<State, State>>()
				{
					{ Input_signal.Colon,
						new Dictionary<State, State>() {
						{  State.S0, State.S1 },
						{  State.S1, State.S_error },
						{  State.S2, State.S_error }
					} },
					{ Input_signal.Equals,
						new Dictionary<State, State>() {
						{  State.S0, State.S_error },
						{  State.S1, State.S2 },
						{  State.S2, State.S_error }
					} },
					{ Input_signal.Other,
						new Dictionary<State, State>() {
						{  State.S0, State.S_error },
						{  State.S1, State.S_error },
						{  State.S2, State.S_error }
					} },
				}
				),
			new MashineString(
				new Dictionary<Input_signal, Dictionary<State, State>>()
				{
					{ Input_signal.Quote,
						new Dictionary<State, State>() {
							{ State.S0, State.S1 },
							{ State.S1, State.S2 },
							{ State.S2, State.S_error }
					} },
					{ Input_signal.Letter,
						new Dictionary<State, State>() {
							{ State.S0, State.S_error },
							{ State.S1, State.S1 },
							{ State.S2, State.S_error }
					} },
					{ Input_signal.Other,
						new Dictionary<State, State>() {
							{ State.S0, State.S_error },
							{ State.S1, State.S_error },
							{ State.S2, State.S_error }
					} },
				}),*/
            /*new MashineOneSymbol(
				new Dictionary<Input_signal, Dictionary<State, State>>()
				{
					{ Input_signal.Letter,
						new Dictionary<State, State>() {
						{  State.S0, State.S1 },
						{  State.S1, State.S1 },
						{  State.S_error, State.S1 }
					} },
					{ Input_signal.Other,
						new Dictionary<State, State>() {
						{  State.S0, State.S_error },
						{  State.S1, State.S_error }
					} }
				})*/
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

            var temp_parsers = parsers
                        .Where(p => p.lex_value != string.Empty && p.lex_value != null)
                        .OrderByDescending(p => p.lex_value.Length)
                        .ToArray();

            foreach (Machine parser in temp_parsers)
            {
                value = parser.lex_value;

                if (parser.IsEnd())
                {
                    if (parser.type == TokenType.Identifier && keyWords.Contains(value))
                        token.type = TokenType.KeyWord;
                    else if (parser.type == TokenType.Identifier && dataTypes.Contains(value))
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
            foreach (Machine parser in parsers)
            {
                parser.Reset();
            }
        }

        public Token GetToken()
        {
            if (fast_token != null)
            {
                var tmp = fast_token;//РАЗРЕШИТЬ ЭТОТ КОСТЫЛЬ!
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

                if (symbol == ' '
                    || symbol == '\t'
                    || symbol == '\n')
                {
                    is_white_space = true;
                }

                bool dividing_lexem = false;
                var type = TokenType.Unknown;

                if (dividing_chars.Contains(symbol))
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
                        //parse_not_dividing_lexem = false;
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

                    foreach (Machine parser in parsers)
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