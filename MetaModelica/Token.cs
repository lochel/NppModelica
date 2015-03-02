using System;

namespace MetaModelica
{
    namespace Parser
    {
        public struct Position
        {
            public int column;
            public int row;

            public Position(int row, int column)
            {
                this.column = column;
                this.row = row;
            }

            public override string ToString()
            {
                return Convert.ToString(row) + ":" + Convert.ToString(column);
            }
        }

        public enum TokenType
        {
            UNKNOWN,
            IDENT,
            STRING,
            UNSIGNED_NUMBER,
            COMMENT,
            WHITESPACE,
            SYMBOL
        }

        public enum LexicalErrorType
        {
            None,
            Unknown
        }

        public struct Token
        {
            public TokenType type;
            public LexicalErrorType error;
            public string value;
            public Position startPosition;
            public Position endPosition;

            public int pos;
            public int length;

            public Token(TokenType type, LexicalErrorType error, string value, Position startPosition, Position endPosition, int pos, int length)
            {
                this.type = type;
                this.error = error;
                this.value = value;
                this.startPosition = startPosition;
                this.endPosition = endPosition;
                this.pos = pos;
                this.length = length;
            }

            public override string ToString()
            {
                if (error == LexicalErrorType.None)
                {
                    if (type == TokenType.WHITESPACE)
                        return type.ToString() + ": [" + startPosition + " - " + endPosition + "]";
                    else
                        return type.ToString() + ": [" + startPosition + " - " + endPosition + "] " + value;
                }
                else
                {
                    return "Error [" + error.ToString() + "] in token: " + type.ToString() + ": [" + startPosition + " - " + endPosition + "] " + value;
                }
            }

            public bool isIDENT()
            {
                return type == TokenType.IDENT;
            }

            public bool isIDENT(string value)
            {
                return (type == TokenType.IDENT) && (this.value == value);
            }

            public bool isSYMBOL()
            {
                return type == TokenType.SYMBOL;
            }

            public bool isSYMBOL(string value)
            {
                return (type == TokenType.SYMBOL) && (this.value == value);
            }

            public bool isSTRING()
            {
                return type == TokenType.STRING;
            }

            public bool isUNSIGNED_NUMBER()
            {
                return type == TokenType.UNSIGNED_NUMBER;
            }

            public bool isUNKNOWN()
            {
                return type == TokenType.UNKNOWN;
            }
        }
    }
}