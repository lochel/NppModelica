using System;
using System.Collections.Generic;
using System.Text;

namespace Modelica
{
    namespace Parser
    {
        public enum Version
        {
            Modelica,
            MetaModelica
        }

        public class Lexer
        {
            public List<Token> tokenList;
            public int numberOfErrors;

            private bool isWhitespace(char c)
            {
                return c == ' ' || c == '\t' || c == '\n';
            }
            private bool isSChar(char c)
            {
                return !(c == '\"' || c == '\\');
            }
            private bool isDigit(char c)
            {
                return c >= '0' && c <= '9';
            }
            private bool isNonDigit(char c)
            {
                return c == '_' ||
                       (c >= 'a' && c <= 'z') ||
                       (c >= 'A' && c <= 'Z');
            }
            private bool isQChar(char c)
            {
                return isNonDigit(c) ||
                       isDigit(c) ||
                       c == '!' ||
                       c == '#' ||
                       c == '$' ||
                       c == '%' ||
                       c == '&' ||
                       c == '(' ||
                       c == ')' ||
                       c == '*' ||
                       c == '+' ||
                       c == ',' ||
                       c == '-' ||
                       c == '.' ||
                       c == '/' ||
                       c == ':' ||
                       c == ';' ||
                       c == '<' ||
                       c == '>' ||
                       c == '=' ||
                       c == '?' ||
                       c == '@' ||
                       c == '[' ||
                       c == ']' ||
                       c == '^' ||
                       c == '{' ||
                       c == '}' ||
                       c == '|' ||
                       c == '~' ||
                       c == ' ';
            }
            private bool isSEscape(char c1, char c2)
            {
                return (c1 == '\\' && c2 == '\'') ||
                       (c1 == '\\' && c2 == '\"') ||
                       (c1 == '\\' && c2 == '?') ||
                       (c1 == '\\' && c2 == '\\') ||
                       (c1 == '\\' && c2 == 'a') ||
                       (c1 == '\\' && c2 == 'b') ||
                       (c1 == '\\' && c2 == 'f') ||
                       (c1 == '\\' && c2 == 'n') ||
                       (c1 == '\\' && c2 == 'r') ||
                       (c1 == '\\' && c2 == 't') ||
                       (c1 == '\\' && c2 == 'v');
            }

            public Lexer(string source, Version version, bool skipWhitespaces)
            {
                source = source.Replace("\r\n", "\n");
                int i = 0;
                int sourceLength = source.Length;
                int column = 1;
                int row = 1;
                int startI;
                Position startPosition;

                tokenList = new List<Token>();
                numberOfErrors = 0;

                while (i < sourceLength)
                {
                    startPosition = new Position(row, column);
                    startI = i;

                    #region COMMENT: //
                    if (i + 1 < sourceLength && (source[i] == '/' && source[i + 1] == '/'))
                    {
                        do
                        {
                            if (source[i] == '\n')
                            {
                                row++;
                                column = 1;
                            }
                            else
                                column++;
                            ++i;
                        } while (i < sourceLength && (source[i] != '\n'));

                        if (!skipWhitespaces)
                            tokenList.Add(new Token(TokenType.COMMENT, LexicalErrorType.None, source.Substring(startI, i - startI), startPosition, new Position(row, column), startI, i - startI));
                    }
                    #endregion
                    #region COMMENT: /* ... */
                    else if (i + 1 < sourceLength && (source[i] == '/' && source[i + 1] == '*'))
                    {
                        i += 2;
                        column += 2;
                        while (i + 1 < sourceLength && !(source[i] == '*' && source[i + 1] == '/')) 
                        {
                            if (source[i] == '\n')
                            {
                                row++;
                                column = 1;
                            }
                            else
                                column++;
                            ++i;
                        }

                        if (i + 1 < sourceLength)
                        {
                            i += 2;
                            column += 2;
                            if (!skipWhitespaces)
                                tokenList.Add(new Token(TokenType.COMMENT, LexicalErrorType.None, source.Substring(startI, i - startI), startPosition, new Position(row, column), startI, i - startI));
                        }
                        else
                        {
                            tokenList.Add(new Token(TokenType.COMMENT, LexicalErrorType.Unknown, source.Substring(startI, i - startI), startPosition, new Position(row, column), startI, i - startI));
                            numberOfErrors++;
                        }
                    }
                    #endregion
                    #region whitspaces
                    else if (isWhitespace(source[i]))
                    {
                        do
                        {
                            if (source[i] == '\n')
                            {
                                row++;
                                column = 1;
                            }
                            else
                                column++;
                            i++;
                        } while (i < sourceLength && isWhitespace(source[i]));

                        if (!skipWhitespaces)
                            tokenList.Add(new Token(TokenType.WHITESPACE, LexicalErrorType.None, source.Substring(startI, i - startI), startPosition, new Position(row, column), startI, i - startI));
                    }
                    #endregion
                    #region [MetaModelica] three symbol tokens
                    else if (i + 2 < sourceLength && version == Version.MetaModelica && ((source[i] == '=' && source[i + 1] == '=' && source[i + 2] == '&')))
                    {
                        i += 3;
                        column += 3;
                        tokenList.Add(new Token(TokenType.SYMBOL, LexicalErrorType.None, source.Substring(startI, i - startI), startPosition, new Position(row, column), startI, i - startI));
                    }
                    #endregion
                    #region [MetaModelica] two symbol tokens
                    else if (i + 1 < sourceLength && version == Version.MetaModelica && ((source[i] == '+' && source[i + 1] == '&')))
                    {
                        i += 2;
                        column += 2;
                        tokenList.Add(new Token(TokenType.SYMBOL, LexicalErrorType.None, source.Substring(startI, i - startI), startPosition, new Position(row, column), startI, i - startI));
                    }
                    #endregion
                    #region two symbol tokens
                    // := <= >= == <> .+ .- .* ./ .^
                    else if (i + 1 < sourceLength && ((source[i] == ':' && source[i + 1] == '=') ||
                                                      (source[i] == '<' && source[i + 1] == '=') ||
                                                      (source[i] == '>' && source[i + 1] == '=') ||
                                                      (source[i] == '=' && source[i + 1] == '=') ||
                                                      (source[i] == '<' && source[i + 1] == '>') ||
                                                      (source[i] == '.' && source[i + 1] == '+') ||
                                                      (source[i] == '.' && source[i + 1] == '-') ||
                                                      (source[i] == '.' && source[i + 1] == '*') ||
                                                      (source[i] == '.' && source[i + 1] == '/') ||
                                                      (source[i] == '.' && source[i + 1] == '^')))
                    {
                        i += 2;
                        column += 2;
                        tokenList.Add(new Token(TokenType.SYMBOL, LexicalErrorType.None, source.Substring(startI, i - startI), startPosition, new Position(row, column), startI, i - startI));
                    }
                    #endregion
                    #region single symbol tokens
                    // ; = ( : ) , { } . * < > + - * / ^ [ ]
                    else if (i < sourceLength && (source[i] == ';' ||
                                                  source[i] == '=' ||
                                                  source[i] == '(' ||
                                                  source[i] == ':' ||
                                                  source[i] == ')' ||
                                                  source[i] == ',' ||
                                                  source[i] == '{' ||
                                                  source[i] == '}' ||
                                                  source[i] == '.' ||
                                                  source[i] == '*' ||
                                                  source[i] == '<' ||
                                                  source[i] == '>' ||
                                                  source[i] == '+' ||
                                                  source[i] == '-' ||
                                                  source[i] == '*' ||
                                                  source[i] == '/' ||
                                                  source[i] == '^' ||
                                                  source[i] == '[' ||
                                                  source[i] == ']'))
                    {
                        i++;
                        column++;
                        tokenList.Add(new Token(TokenType.SYMBOL, LexicalErrorType.None, source.Substring(startI, i - startI), startPosition, new Position(row, column), startI, i - startI));
                    }
                    #endregion
                    #region IDENT
                    else if (isNonDigit(source[i]))
                    {
                        do
                        {
                            i++;
                            column++;
                        } while (i < sourceLength && (isDigit(source[i]) || isNonDigit(source[i])));
                        tokenList.Add(new Token(TokenType.IDENT, LexicalErrorType.None, source.Substring(startI, i - startI), startPosition, new Position(row, column), startI, i - startI));
                    }
                    #endregion
                    #region Q-IDENT
                    else if (i + 2 < sourceLength && source[i] == '\'' && (isQChar(source[i + 1]) || isSEscape(source[i + 1], source[i + 2])))
                    {
                        i++;
                        column++;
                        do
                        {
                            if (isQChar(source[i]))
                            {
                                i++;
                                column++;
                            }
                            else if (isSEscape(source[i], source[i + 1]))
                            {
                                i += 2;
                                column += 2;
                            }
                        } while (i + 1 < sourceLength && (isQChar(source[i]) || isSEscape(source[i], source[i + 1])));

                        if (source[i] == '\'')
                        {
                            i++;
                            column++;
                        }
                        tokenList.Add(new Token(TokenType.IDENT, LexicalErrorType.None, source.Substring(startI, i - startI), startPosition, new Position(row, column), startI, i - startI));
                    }
                    #endregion
                    #region STRING
                    else if (source[i] == '\"')
                    {
                        i++;
                        column++;
                        while (i + 1 < sourceLength && (isSChar(source[i]) || isSEscape(source[i], source[i + 1])))
                        {
                            if (isSChar(source[i]))
                            {
                                if (source[i] == '\n')
                                {
                                    i++;
                                    row++;
                                    column = 1;
                                }
                                else
                                {
                                    i++;
                                    column++;
                                }
                            }
                            else if (isSEscape(source[i], source[i + 1]))
                            {
                                i += 2;
                                column += 2;
                            }
                        }

                        if (i < sourceLength && source[i] == '\"')
                        {
                            i++;
                            column++;
                            tokenList.Add(new Token(TokenType.STRING, LexicalErrorType.None, source.Substring(startI + 1, i - startI - 2), startPosition, new Position(row, column), startI, i - startI));
                        }
                        else
                        {
                            tokenList.Add(new Token(TokenType.STRING, LexicalErrorType.Unknown, source.Substring(startI, i - startI), startPosition, new Position(row, column), startI, i - startI));
                            numberOfErrors++;
                        }
                    }
                    #endregion
                    #region UNSIGNED_NUBER
                    else if (isDigit(source[i]))
                    {
                        do { i++; } while (i < sourceLength && isDigit(source[i]));

                        if (i < sourceLength && source[i] == '.')
                        {
                            ++i;

                            if (i < sourceLength && isDigit(source[i]))
                            {
                                do { i++; } while (i < sourceLength && isDigit(source[i]));
                            }
                        }

                        if (i < sourceLength && (source[i] == 'e' || source[i] == 'E'))
                        {
                            ++i;

                            if (i < sourceLength && (source[i] == '+' || source[i] == '-'))
                                ++i;

                            if (i < sourceLength && isDigit(source[i]))
                            {
                                do { i++; } while (i < sourceLength && isDigit(source[i]));
                            }
                        }

                        column += i - startI;
                        tokenList.Add(new Token(TokenType.UNSIGNED_NUMBER, LexicalErrorType.None, source.Substring(startI, i - startI), startPosition, new Position(row, column), startI, i - startI));
                    }
                    #endregion
                    #region unknown token
                    else
                    {
                        i++;
                        column++;
                        tokenList.Add(new Token(TokenType.UNKNOWN, LexicalErrorType.Unknown, source.Substring(startI, i - startI), startPosition, new Position(row, column), startI, i - startI));
                        numberOfErrors++;
                    }
                    #endregion
                }
            }
        }
    }
}
