﻿using FaunaDB.Collections;
using FaunaDB.Types;
using NUnit.Framework;
using System;

namespace Test
{
    [TestFixture] public class CodecTest
    {
        [Test] public void TestRef()
        {
            Assert.AreEqual(Result.Success(new Ref("databases")), new Ref("databases").To(Codec.REF));
            Assert.AreEqual(Result.Fail<Ref>("Cannot convert StringV to Ref"), StringV.Of("a string").To(Codec.REF));
        }

        [Test] public void TestSetRef()
        {
            var dic = ImmutableDictionary.Of<string, Value>("@ref", "databases");
            Assert.AreEqual(Result.Success(new SetRef(dic)), new SetRef(dic).To(Codec.SETREF));
            Assert.AreEqual(Result.Fail<SetRef>("Cannot convert StringV to SetRef"), StringV.Of("a string").To(Codec.SETREF));
        }

        [Test] public void TestLong()
        {
            Assert.AreEqual(Result.Success(1L), LongV.Of(1).To(Codec.LONG));
            Assert.AreEqual(Result.Fail<long>("Cannot convert StringV to LongV"), StringV.Of("a string").To(Codec.LONG));
        }

        [Test] public void TestString()
        {
            Assert.AreEqual(Result.Success("a string"), StringV.Of("a string").To(Codec.STRING));
            Assert.AreEqual(Result.Fail<string>("Cannot convert ObjectV to StringV"), ObjectV.Empty.To(Codec.STRING));
        }

        [Test] public void TestBoolean()
        {
            Assert.AreEqual(Result.Success(true), BooleanV.True.To(Codec.BOOLEAN));
            Assert.AreEqual(Result.Success(false), BooleanV.False.To(Codec.BOOLEAN));
            Assert.AreEqual(Result.Fail<bool>("Cannot convert ObjectV to BooleanV"), ObjectV.Empty.To(Codec.BOOLEAN));
        }

        [Test] public void TestDouble()
        {
            Assert.AreEqual(Result.Success(3.14), DoubleV.Of(3.14).To(Codec.DOUBLE));
            Assert.AreEqual(Result.Fail<double>("Cannot convert ObjectV to DoubleV"), ObjectV.Empty.To(Codec.DOUBLE));
        }

        [Test] public void TestTimestamp()
        {
            Assert.AreEqual(Result.Success(new DateTime(2000, 1, 1, 0, 0, 0, 123)), new TsV("2000-01-01T00:00:00.123Z").To(Codec.TS));
            Assert.AreEqual(Result.Fail<DateTime>("Cannot convert ObjectV to TsV"), ObjectV.Empty.To(Codec.TS));
        }

        [Test] public void TestDate()
        {
            Assert.AreEqual(Result.Success(new DateTime(2000, 1, 1)), new DateV("2000-01-01").To(Codec.DATE));
            Assert.AreEqual(Result.Fail<DateTime>("Cannot convert ObjectV to DateV"), ObjectV.Empty.To(Codec.DATE));
        }

        [Test] public void TestArray()
        {
            var array = ImmutableList.Of<Value>("a string", true, 10);

            Assert.AreEqual(Result.Success(array), ArrayV.Of("a string", true, 10).To(Codec.ARRAY));
            Assert.AreEqual(Result.Fail<ArrayList<Value>>("Cannot convert ObjectV to ArrayV"), ObjectV.Empty.To(Codec.ARRAY));
        }

        [Test] public void TestObject()
        {
            var obj = ImmutableDictionary.Of<string, Value>("foo", StringV.Of("bar"));

            Assert.AreEqual(Result.Success(obj), ObjectV.With("foo", "bar").To(Codec.OBJECT));
            Assert.AreEqual(Result.Fail<OrderedDictionary<string, Value>>("Cannot convert StringV to ObjectV"), StringV.Of("a string").To(Codec.OBJECT));
        }
    }
}
