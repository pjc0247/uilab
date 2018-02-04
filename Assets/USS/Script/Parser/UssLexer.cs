using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UssLexer
{
    private class LexerState
    {
        public string src;
        public int offset, cur;
        public int line;
    }

    private static Dictionary<string, UssTokenType> seperators;

    private LexerState state;
    private List<UssToken> tokens;

    static UssLexer()
    {
        seperators = new Dictionary<string, UssTokenType>();

        seperators[" "] = UssTokenType.Whitespace;
        seperators["	"] = UssTokenType.Whitespace;
        seperators["\r"] = UssTokenType.CrLf;
        seperators["\n"] = UssTokenType.CrLf;
        seperators[":"] = UssTokenType.Colon;
        seperators[";"] = UssTokenType.SemiColon;
        seperators["{"] = UssTokenType.LeftBracket;
        seperators["}"] = UssTokenType.RightBracket;
        seperators[","] = UssTokenType.Comma;
        seperators["*"] = UssTokenType.Asterisk;
        seperators["//"] = UssTokenType.Comment;
        seperators["null"] = UssTokenType.Null;
        seperators[">"] = UssTokenType.RightArrow;
    }
    public static UssToken[] Parse(string src)
    {
        return new UssLexer().ParseAll(src);
    }
    public static UssToken ParseOne(string str)
    {
        return new UssLexer().ParseSingle(str);
    }

    private UssLexer()
    {
        tokens = new List<UssToken>();
        state = new LexerState();
        state.line = 1;
    }
    private UssToken[] ParseAll(string src)
    {
        var current = new UssToken();
        src += " "; // padding
        state.src = src;

        while (state.cur != src.Length)
        {
            var found = false;

            foreach (var pair in seperators)
            {
                if (state.cur + pair.Key.Length >= src.Length)
                    continue;
                var candidate = src.Substring(state.cur, pair.Key.Length);
                if (candidate == pair.Key)
                {
                    Flush();
                    current.body = candidate;
                    current.type = pair.Value;
                    AppendToken(current);
                    current = new UssToken();

                    state.cur += pair.Key.Length;
                    state.offset = state.cur;

                    found = true;
                    break;
                }
            }

            if (found == false)
                state.cur++;
        }

        return tokens.ToArray();
    }
    private UssToken ParseSingle(string str)
    {
        int trashInt;
        float trashFloat;

        // HEX-COLOR
        if (str[0] == '#')
        {
            // 용법이 확정되는 Parser레벨에서 검사
            //if (UssValidator.IsValidId(str) == false)
            //    throw new UssInvalidTokenException(str);

            return new UssToken()
            {
                body = str,
                type = UssTokenType.HexColor
            };
        }
        // VALUE REF
        else if (str[0] == '@')
        {
            if (UssValidator.IsValidValueRef(str) == false)
                throw new UssInvalidTokenException(str);

            return new UssToken()
            {
                body = str,
                type = UssTokenType.ValueRef
            };
        }
        // INT
        else if (int.TryParse(str, out trashInt))
        {
            if (UssValidator.IsValidInt(str) == false)
                throw new UssInvalidTokenException(str);

            return new UssToken()
            {
                body = str,
                type = UssTokenType.Int
            };
        }
        // FLOAT
        else if (float.TryParse(str, out trashFloat))
        {
            if (UssValidator.IsValidFloat(str) == false)
                throw new UssInvalidTokenException(str);

            return new UssToken()
            {
                body = str,
                type = UssTokenType.Float
            };
        }
        // ID
        else
        {
            if (UssValidator.IsValidId(str) == false)
                throw new UssInvalidTokenException(str);

            return new UssToken()
            {
                body = str,
                type = UssTokenType.Id
            };
        }
    }
    private bool Flush()
    {
        if (state.cur == state.offset)
            return false;

        var prev = ParseSingle(
            state.src.Substring(state.offset, (state.cur) - state.offset));
        if (prev != null)
        {
            AppendToken(prev);
            return true;
        }
        return false;
    }

    private void AppendToken(UssToken token)
    {
        tokens.Add(InjectState(token));
    }
    private UssToken InjectState(UssToken token)
    {
        token.line = state.line;
        return token;
    }
}