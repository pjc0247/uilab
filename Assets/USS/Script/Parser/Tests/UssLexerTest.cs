using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NUnit.Framework;

public class UssLexerTest : MonoBehaviour
{
    [Test]
    public void ParseOne()
    {
        var parsed = UssLexer.ParseOne("#ABABAB");
        Assert.AreEqual(UssTokenType.HexColor, parsed.type);
    }

    [Test]
    public void ParseOne_Number()
    {
        var parsed = UssLexer.ParseOne("1234");
        Assert.AreEqual(UssTokenType.Int, parsed.type);

        parsed = UssLexer.ParseOne("123.4");
        Assert.AreEqual(UssTokenType.Float, parsed.type);
    }

    [Test]
    public void ParseOne_ValueRef()
    {
        var parsed = UssLexer.ParseOne("@asdf");
        Assert.AreEqual(UssTokenType.ValueRef, parsed.type);
    }

    [Test]
    public void ParseAll()
    {
        var tokens = UssLexer.Parse("corgi{color:#ABABAB;}");

        Assert.AreEqual(UssTokenType.Id, tokens[0].type);
        Assert.AreEqual(UssTokenType.LeftBracket, tokens[1].type);
        Assert.AreEqual(UssTokenType.Colon, tokens[3].type);
        Assert.AreEqual(UssTokenType.HexColor, tokens[4].type);
        Assert.AreEqual(UssTokenType.SemiColon, tokens[5].type);
        Assert.AreEqual(UssTokenType.RightBracket, tokens[6].type);
    }

    [Test]
    public void InvalidToken_WrongHexColor()
    {
        Assert.Throws<UssInvalidTokenException>(() => {
            UssLexer.ParseOne("#..zuzu..");
        });
        Assert.DoesNotThrow(() => {
            UssLexer.ParseOne("#ABABAB");
        });
    }
    
    [Test]
    public void InvalidToken_LongNumber()
    {
        // INTEGER
        Assert.Throws<UssInvalidTokenException>(() => {
            UssLexer.ParseOne("123412341234213124123214123124213");
        });
        Assert.DoesNotThrow(() => {
            UssLexer.ParseOne("1234");
        });

        // FLOAT
        Assert.Throws<UssInvalidTokenException>(() => {
            UssLexer.ParseOne("12341234123.4213124123214123124213");
        });
        Assert.DoesNotThrow(() => {
            UssLexer.ParseOne("123.4");
        });
    }
}
