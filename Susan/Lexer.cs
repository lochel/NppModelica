using System.Collections.Generic;

namespace Susan
{
    namespace Parser
    {
        public class Lexer
        {
            public List<Token> tokenList;
            public int numberOfErrors;

            public override string ToString()
            {
                string str = "";
                foreach(Token t in tokenList)
                    str += t + "\n";
                return str;
            }

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

            bool checkContext(Stack<string> contextStack, string context)
            {
                if (contextStack.Count > 0 && contextStack.Peek() == context)
                    return true;
                return false;
            }

            bool isSourceContext(Stack<string> contextStack)
            {
                return contextStack.Count == 0 || checkContext(contextStack, "<%");
            }

            public Lexer(string source, bool skipWhitespaces)
            {
                source = source.Replace("\r\n", "\n");
                int i = 0;
                int sourceLength = source.Length;
                int column = 1;
                int row = 1;
                int startI;
                Position startPosition;

                Stack<string> contextStack = new Stack<string>();

                tokenList = new List<Token>();
                numberOfErrors = 0;

                while (i < sourceLength)
                {
                    startPosition = new Position(row, column);
                    startI = i;

                    if (isSourceContext(contextStack))
                    {
                        #region COMMENT: // ...
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
                        #region three symbol tokens (::=)
                        else if (i + 2 < sourceLength && ((source[i] == ':' && source[i + 1] == ':' && source[i + 2] == '=')))
                        {
                            i += 3;
                            column += 3;
                            tokenList.Add(new Token(TokenType.SYMBOL, LexicalErrorType.None, source.Substring(startI, i - startI), startPosition, new Position(row, column), startI, i - startI));
                        }
                        #endregion
                        #region two symbol tokens (:= <= >= == <> .+ .- .* ./ .^ |>)
                        else if (i + 1 < sourceLength && ((source[i] == ':' && source[i + 1] == '=') ||
                                                          (source[i] == '<' && source[i + 1] == '=') ||
                                                          (source[i] == '>' && source[i + 1] == '=') ||
                                                          (source[i] == '=' && source[i + 1] == '=') ||
                                                          (source[i] == '<' && source[i + 1] == '>') ||
                                                          (source[i] == '.' && source[i + 1] == '+') ||
                                                          (source[i] == '.' && source[i + 1] == '-') ||
                                                          (source[i] == '.' && source[i + 1] == '*') ||
                                                          (source[i] == '.' && source[i + 1] == '/') ||
                                                          (source[i] == '.' && source[i + 1] == '^') ||
                                                          (source[i] == '|' && source[i + 1] == '>')))
                        {
                            i += 2;
                            column += 2;
                            tokenList.Add(new Token(TokenType.SYMBOL, LexicalErrorType.None, source.Substring(startI, i - startI), startPosition, new Position(row, column), startI, i - startI));
                        }
                        #endregion
                        #region special symbol token (<<)
                        else if (i + 1 < sourceLength && ((source[i] == '<' && source[i + 1] == '<')))
                        {
                            i += 2;
                            column += 2;
                            tokenList.Add(new Token(TokenType.SYMBOL, LexicalErrorType.None, source.Substring(startI, i - startI), startPosition, new Position(row, column), startI, i - startI));

                            contextStack.Push("<<");

                        }
                        #endregion
                        #region special symbol token (%>)
                        else if (i + 1 < sourceLength && source[i] == '%' && source[i + 1] == '>')
                        {
                            i += 2;
                            column += 2;

                            if (contextStack.Count > 0)
                            {
                                tokenList.Add(new Token(TokenType.SYMBOL, LexicalErrorType.None, source.Substring(startI, i - startI), startPosition, new Position(row, column), startI, i - startI));
                                contextStack.Pop();
                            }
                            else
                            {
                                tokenList.Add(new Token(TokenType.SYMBOL, LexicalErrorType.Unknown, source.Substring(startI, i - startI), startPosition, new Position(row, column), startI, i - startI));
                                numberOfErrors++;
                            }
                        }
                        #endregion
                        #region single symbol tokens (; = ( : ) , { } . * < > + - * / ^ [ ] & \)
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
                                                      source[i] == ']' ||
                                                      source[i] == '&' ||
                                                      source[i] == '\\'))
                        {
                            i++;
                            column++;
                            tokenList.Add(new Token(TokenType.SYMBOL, LexicalErrorType.None, source.Substring(startI, i - startI), startPosition, new Position(row, column), startI, i - startI));
                        }
                        #endregion
                        #region special symbol token (')
                        else if (i < sourceLength && (source[i] == '\''))
                        {
                            i++;
                            column++;
                            tokenList.Add(new Token(TokenType.SYMBOL, LexicalErrorType.None, source.Substring(startI, i - startI), startPosition, new Position(row, column), startI, i - startI));

                            contextStack.Push(source.Substring(startI, i - startI));
                        }
                        #endregion
                        #region STRING
                        else if (source[i] == '\"')
                        {
                            i++;
                            column++;
                            while (i + 1 < sourceLength && (source[i] == '\\' || isSChar(source[i]) || isSEscape(source[i], source[i + 1])))
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
                                else
                                {
                                    i++;
                                    column++;
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
                    else if (checkContext(contextStack, "\'"))
                    {
                        while ((i + 0 < sourceLength && !(source[i] == '\'')) &&
                               (i + 1 < sourceLength && !(source[i] == '<' && source[i + 1] == '%')))
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

                        if (i + 1 < sourceLength && source[i] == '<' && source[i + 1] == '%')
                        {
                            if (startI < i)
                            {
                                i += 2;
                                column += 2;
                                tokenList.Add(new Token(TokenType.TEXT, LexicalErrorType.None, source.Substring(startI, i - startI - 2), startPosition, new Position(row, column - 2), startI, i - startI - 2));
                                tokenList.Add(new Token(TokenType.SYMBOL, LexicalErrorType.None, "<%", new Position(row, column - 2), new Position(row, column), i - 2, 2));
                            }
                            else
                            {
                                i += 2;
                                column += 2;
                                tokenList.Add(new Token(TokenType.SYMBOL, LexicalErrorType.None, "<%", new Position(row, column - 2), new Position(row, column), i - 2, 2));
                            }
                            contextStack.Push("<%");
                        }
                        else if (i < sourceLength && source[i] == '\'')
                        {
                            i++;
                            column++;
                            if (contextStack.Count > 0)
                            {
                                tokenList.Add(new Token(TokenType.TEXT, LexicalErrorType.None, source.Substring(startI, i - startI - 1), startPosition, new Position(row, column - 1), startI, i - startI - 1));
                                tokenList.Add(new Token(TokenType.SYMBOL, LexicalErrorType.None, source.Substring(i - 1, 1), new Position(row, column - 1), new Position(row, column), i - 1, 1));
                                contextStack.Pop();
                            }
                            else
                            {
                                tokenList.Add(new Token(TokenType.TEXT, LexicalErrorType.Unknown, source.Substring(startI, i - startI), startPosition, new Position(row, column), startI, i - startI));
                                numberOfErrors++;
                            }
                        }
                        else
                        {
                            tokenList.Add(new Token(TokenType.TEXT, LexicalErrorType.Unknown, source.Substring(startI, i - startI), startPosition, new Position(row, column), startI, i - startI));
                            numberOfErrors++;
                            if (contextStack.Count > 0)
                                contextStack.Pop();
                        }
                    }
                    else if (checkContext(contextStack, "<<"))
                    {
                        while ((i + 1 < sourceLength && !(source[i] == '>' && source[i + 1] == '>')) &&
                               (i + 1 < sourceLength && !(source[i] == '<' && source[i + 1] == '%')))
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

                        if (i + 1 < sourceLength && source[i] == '<' && source[i + 1] == '%')
                        {
                            if (startI < i)
                            {
                                i += 2;
                                column += 2;
                                tokenList.Add(new Token(TokenType.TEXT, LexicalErrorType.None, source.Substring(startI, i - startI - 2), startPosition, new Position(row, column - 2), startI, i - startI - 2));
                                tokenList.Add(new Token(TokenType.SYMBOL, LexicalErrorType.None, "<%", new Position(row, column - 2), new Position(row, column), i - 2, 2));
                            }
                            else
                            {
                                i += 2;
                                column += 2;
                                tokenList.Add(new Token(TokenType.SYMBOL, LexicalErrorType.None, "<%", new Position(row, column - 2), new Position(row, column), i - 2, 2));
                            }
                            contextStack.Push("<%");
                        }
                        else if (i + 1 < sourceLength && source[i] == '>' && source[i+1] == '>')
                        {
                            i+=2;
                            column+=2;
                            if (contextStack.Count > 0)
                            {
                                tokenList.Add(new Token(TokenType.TEXT, LexicalErrorType.None, source.Substring(startI, i - startI - 2), startPosition, new Position(row, column - 2), startI, i - startI - 2));
                                tokenList.Add(new Token(TokenType.SYMBOL, LexicalErrorType.None, ">>", new Position(row, column - 2), new Position(row, column), i - 2, 2));
                                contextStack.Pop();
                            }
                            else
                            {
                                tokenList.Add(new Token(TokenType.TEXT, LexicalErrorType.Unknown, source.Substring(startI, i - startI), startPosition, new Position(row, column), startI, i - startI));
                                numberOfErrors++;
                            }
                        }
                        else
                        {
                            tokenList.Add(new Token(TokenType.TEXT, LexicalErrorType.Unknown, source.Substring(startI, i - startI), startPosition, new Position(row, column), startI, i - startI));
                            numberOfErrors++;
                            if (contextStack.Count > 0)
                                contextStack.Pop();
                        }
                    }
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
