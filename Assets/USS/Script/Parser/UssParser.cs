using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UssParser
{
    private enum ParsingState
    {
        Root,

        Values,
        Conditions,
        Properties
    }
    private enum ValueParsingState
    {
        Key, Colon, Value, End
    }
    private enum PropertyParsingState
    {
        Key, Colon, Value
    }
    private enum CurrentNodeType
    {
        Bundle,
        Style
    }

    private ParsingState state;
    private ValueParsingState valueState;
    private PropertyParsingState propertyState;

    private List<UssToken> tokens;
    private List<UssStyleDefinition> styles;
    private Dictionary<string, UssValue> values;

    private UssStyleDefinition current;
    private List<string> bundles;
    private List<UssStyleCondition> conditions;
    private List<UssStyleProperty> properties;
    private List<UssValue> propertyValues;

    private int cur = 0;
    private string valueKey;
    private string propertyKey;

    private CurrentNodeType nodeType;
    private UssStyleConditionType nextConditionType;

    public static UssParsingResult Parse(string src)
    {
        return new UssParser().ParseAll(UssLexer.Parse(src));
    }
    public static UssStyleCondition[] ParseConditions(string src)
    {
        src += " "; // padding
        var p = new UssParser();
        p.ParseAll(UssLexer.Parse(src));
        return p.conditions.ToArray();
    }

    private UssParser()
    {
        state = ParsingState.Root;
        propertyState = PropertyParsingState.Key;
        valueState = ValueParsingState.Key;
    }
    public UssParsingResult ParseAll(UssToken[] _tokens)
    {
        tokens = new List<UssToken>(_tokens);
        styles = new List<UssStyleDefinition>();
        values = new Dictionary<string, UssValue>();

        FlushCurrentDifinition();
        for (cur = 0; cur < tokens.Count; cur++)
        {
            var token = tokens[cur];

            if (state == ParsingState.Root)
            {
                if (token.IsIgnoreable) continue;

                if (token.type == UssTokenType.ValueRef)
                {
                    state = ParsingState.Values;
                    valueState = ValueParsingState.Key;
                }
                else
                    state = ParsingState.Conditions;
            }

            if (token.type == UssTokenType.Comment)
            {
                while (GetNextToken() != null && GetNextToken().type != UssTokenType.CrLf)
                    WasteNextToken();
                continue;
            }

            if (state == ParsingState.Values)
                ParseValues(token);
            else if (state == ParsingState.Conditions)
                ParseConditions(token);
            else if (state == ParsingState.Properties)
            {
                if (ParseProperties(token))
                    FlushCurrentDifinition();
            }
        }

        return new UssParsingResult()
        {
            styles = styles.ToArray(),
            values = values
        };
    }

    private void FlushCurrentDifinition()
    {
        if (current != null)
        {
            if (nodeType == CurrentNodeType.Bundle)
            {
                values[valueKey] = new UssBundleValue() {
                    value = properties.ToArray()
                };
            }
            else
            {
                current.conditions = conditions.ToArray();
                current.properties = properties.ToArray();
                current.bundles = bundles.ToArray();
                styles.Add(current);
            }
        }

        current = new UssStyleDefinition();
        bundles = new List<string>();
        conditions = new List<UssStyleCondition>();
        properties = new List<UssStyleProperty>();
    }
    private void WasteNextToken()
    {
        cur++;
    }
    private UssToken GetNextToken(bool shouldBeNotNull = false)
    {
        if (cur + 1 == tokens.Count)
        {
            if (shouldBeNotNull)
                throw new UssParsingException("Unexpected end");
            return null;
        }
        return tokens[cur + 1];
    }

    private void ParseValues(UssToken token)
    {
        if (token.IsIgnoreable) return;

        if (token.type == UssTokenType.SemiColon)
        {
            if (valueState != ValueParsingState.End)
                throw new UssUnexpectedTokenException(token);

            state = ParsingState.Root;
            return;
        }

        if (valueState == ValueParsingState.Key)
        {
            if (token.type != UssTokenType.ValueRef)
                throw new UssUnexpectedTokenException(token, UssTokenType.ValueRef);

            valueKey = token.body.Substring(1);
            valueState = ValueParsingState.Colon;
        }
        else if (valueState == ValueParsingState.Colon)
        {
            if (token.type == UssTokenType.LeftBracket)
            {
                state = ParsingState.Properties;
                nodeType = CurrentNodeType.Bundle;
            }
            else if (token.type == UssTokenType.Colon) 
                valueState = ValueParsingState.Value;
            else
                throw new UssUnexpectedTokenException(token);
        }
        else if (valueState == ValueParsingState.Value)
        {
            if (token.IsValue == false)
                throw new UssUnexpectedTokenException(token);

            values.Add(valueKey, UssValue.Create(token));
            valueState = ValueParsingState.End;
        }
    }
    private void ParseConditions(UssToken token)
    {
        if (token.IsIgnoreable) return;

        if (token.type == UssTokenType.LeftBracket)
        {
            state = ParsingState.Properties;
            nodeType = CurrentNodeType.Style;
            return;
        }

        if (token.type == UssTokenType.RightArrow)
        {
            if (conditions.Count == 0)
                throw new UssUnexpectedTokenException(token);

            nextConditionType = UssStyleConditionType.DirectDescendant;
            return;
        }
        if (token.type == UssTokenType.Colon)
        {
            if (conditions.Count == 0)
                throw new UssUnexpectedTokenException(token);
            if (GetNextToken(true).type != UssTokenType.Id)
                throw new UssUnexpectedTokenException(GetNextToken(), UssTokenType.Id);

            conditions.Last().targetState = GetNextToken().body;
            WasteNextToken();
            return;
        }

        // Every types of token can be accepted here
        // since name of the Unity's gameobject can contain almost characters.
        // (ex: Zu!ZU##!)
        var rawCondition = token.body;
        var styleCondition = new UssStyleCondition();

        if (rawCondition[0] == '*')
        {
            styleCondition.target = UssSelectorType.All;
            styleCondition.name = "*";
        }
        // CLASS
        else if (rawCondition[0] == '.')
        {
            styleCondition.target = UssSelectorType.Class;
            styleCondition.name = rawCondition.Substring(1);
        }
        // NAME
        else if (rawCondition[0] == '#')
        {
            styleCondition.target = UssSelectorType.Name;
            styleCondition.name = rawCondition.Substring(1);
        }
        else
        {
            styleCondition.target = UssSelectorType.Component;
            styleCondition.name = rawCondition;
        }

        styleCondition.type = nextConditionType;
        conditions.Add(styleCondition);
        nextConditionType = UssStyleConditionType.None;
    }
    private bool ParseProperties(UssToken token)
    {
        if (token.IsIgnoreable) return false;

        if (propertyState == PropertyParsingState.Key &&
            token.type == UssTokenType.RightBracket)
        {
            state = ParsingState.Root;
            return true;
        }

        if (propertyState == PropertyParsingState.Key)
        {
            if (token.type == UssTokenType.ValueRef)
            {
                bundles.Add(token.body.Substring(1));

                if (GetNextToken().type == UssTokenType.SemiColon)
                    WasteNextToken();
                else
                    throw new UssUnexpectedTokenException(GetNextToken(), UssTokenType.SemiColon);
            }
            else if (token.type == UssTokenType.Id)
            {
                propertyValues = new List<UssValue>();

                propertyKey = token.body;
                propertyState = PropertyParsingState.Colon;
            }
            else
                throw new InvalidOperationException("Invalid token `" + token.body + "`. (expected Id)");
        }
        else if (propertyState == PropertyParsingState.Colon)
        {
            if (token.type != UssTokenType.Colon)
                throw new InvalidOperationException("Invalid token `" + token.body + "`. (expected Colon)");

            propertyState = PropertyParsingState.Value;
        }
        else
        {
            if (token.IsValue)
            {
                propertyValues.Add(UssValue.Create(token));
            }
            else if (token.type == UssTokenType.SemiColon)
            {
                properties.Add(new UssStyleProperty()
                {
                    key = propertyKey,
                    values = propertyValues.ToArray()
                });

                propertyState = PropertyParsingState.Key;
            }
            else
                throw new InvalidOperationException("Invalid token `" + token.body + "`. (expected Value)");
        }

        return false;
    }
}