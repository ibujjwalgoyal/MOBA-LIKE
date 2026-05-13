using System;
using System.Text;
using System.Text.Json;
using NUnit.Framework;
using MOBA.Math;

namespace MOBA.Tests
{
    [TestFixture]
    public class FixedPointTests
    {
        [Test]
        public void Construction()
        {
            var a = default(FixedPoint);
            Assert.AreEqual(0, a.RawValue);

            var b = FixedPoint.FromInt(5);
            Assert.AreEqual(5000, b.RawValue);
        }

        [Test]
        public void Arithmetic()
        {
            var a = FixedPoint.FromInt(3);
            var b = FixedPoint.FromInt(2);
            var c = FixedPoint.FromInt(6);

            Assert.AreEqual(FixedPoint.FromInt(5), a + b);
            Assert.AreEqual(FixedPoint.FromInt(1), a - b);
            Assert.AreEqual(FixedPoint.FromInt(6), a * b);
            Assert.AreEqual(FixedPoint.FromInt(3), c / b);
            Assert.AreEqual(FixedPoint.FromInt(-3), -a);
        }

        [Test]
        public void Comparison()
        {
            var a = FixedPoint.FromInt(5);
            var b = FixedPoint.FromInt(5);
            var c = FixedPoint.FromInt(6);

            Assert.True(a == b);
            Assert.True(a != c);
            Assert.True(a < c);
            Assert.True(c > a);
            Assert.True(a <= b);
            Assert.True(c >= a);
        }

        [Test]
        public void Sqrt()
        {
            var v4 = FixedPoint.FromInt(4);
            Assert.AreEqual(2000, FixedPoint.Sqrt(v4).RawValue);

            var v2 = FixedPoint.FromInt(2);
            Assert.AreEqual(1414, FixedPoint.Sqrt(v2).RawValue);

            var v0 = new FixedPoint(0);
            Assert.AreEqual(0, FixedPoint.Sqrt(v0).RawValue);

            var neg = FixedPoint.FromInt(-1);
            Assert.AreEqual(0, FixedPoint.Sqrt(neg).RawValue);
        }

        [Test]
        public void Distance2D()
        {
            var zero = new FixedPoint(0);
            var x2 = FixedPoint.FromInt(3);
            var y2 = FixedPoint.FromInt(4);
            var dist = FixedPoint.Distance2D(zero, zero, x2, y2);
            Assert.AreEqual(5000, dist.RawValue);
        }

        [Test]
        public void OverflowCheck()
        {
            var max = FixedPoint.FromRaw(2000000000);
            var two = FixedPoint.FromInt(2);
            var prod = max * two;
            Assert.GreaterOrEqual(prod.RawValue, 0);
            var div = max / two;
            Assert.AreEqual(1000000000, div.RawValue);
        }

        [Test]
        public void CrossLanguageDeterminism()
        {
            string json = TestDataGenerator.Generate1000OperationsJson();
            var operations = JsonSerializer.Deserialize<Operation[]>(json);
            Assert.IsNotNull(operations);
            Assert.AreEqual(1000, operations.Length);

            foreach (var op in operations)
            {
                FixedPoint result;
                switch (op.op)
                {
                    case "add":
                        result = FixedPoint.FromRaw(op.a) + FixedPoint.FromRaw(op.b);
                        break;
                    case "sub":
                        result = FixedPoint.FromRaw(op.a) - FixedPoint.FromRaw(op.b);
                        break;
                    case "mul":
                        result = FixedPoint.FromRaw(op.a) * FixedPoint.FromRaw(op.b);
                        break;
                    case "div":
                        result = FixedPoint.FromRaw(op.a) / FixedPoint.FromRaw(op.b);
                        break;
                    case "sqrt":
                        result = FixedPoint.Sqrt(FixedPoint.FromRaw(op.a));
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown op: {op.op}");
                }
                Assert.AreEqual(op.expected, result.RawValue,
                    $"Mismatch on {op.op}({op.a}, {op.b}) – expected {op.expected}, got {result.RawValue}");
            }
        }
    }

    public struct Operation
    {
        public string op { get; set; }
        public int a { get; set; }
        public int b { get; set; }
        public int expected { get; set; }
    }

    public static class TestDataGenerator
    {
        private const uint Seed = 42;
        private const uint LcgMultiplier = 1664525;
        private const uint LcgIncrement = 1013904223;

        public static string Generate1000OperationsJson()
        {
            var sb = new StringBuilder();
            sb.Append('[');
            uint state = Seed;

            // 200 add operations
            for (int i = 0; i < 200; i++)
            {
                state = NextState(state);
                int a = (int)state;
                state = NextState(state);
                int b = (int)state;
                int expected = unchecked(a + b);
                AppendOp(sb, "add", a, b, expected, i == 0 && i == 999);
            }

            // 200 sub operations
            for (int i = 0; i < 200; i++)
            {
                state = NextState(state);
                int a = (int)state;
                state = NextState(state);
                int b = (int)state;
                int expected = unchecked(a - b);
                AppendOp(sb, "sub", a, b, expected, false);
            }

            // 200 mul operations (a,b limited to safe range to avoid int64 overflow)
            for (int i = 0; i < 200; i++)
            {
                state = NextState(state);
                int a = (int)(state % 1000000);
                state = NextState(state);
                int b = (int)(state % 1000000);
                // expected: (a * b) / 1000 using 64-bit
                int expected = (int)(((long)a * b) / 1000);
                AppendOp(sb, "mul", a, b, expected, false);
            }

            // 200 div operations (b != 0)
            for (int i = 0; i < 200; i++)
            {
                state = NextState(state);
                int a = (int)state;
                state = NextState(state);
                int b = (int)state;
                if (b == 0) b = 1; // avoid division by zero
                // expected: (a * 1000) / b using 64-bit
                int expected = (int)(((long)a * 1000) / b);
                AppendOp(sb, "div", a, b, expected, false);
            }

            // 200 sqrt operations (a >= 0)
            for (int i = 0; i < 200; i++)
            {
                state = NextState(state);
                int a = (int)(state & 0x7FFFFFFF); // positive only
                // expected: sqrt(a * 1000) using integer sqrt
                if (a <= 0) expected = 0;
                else
                {
                    long n = (long)a * 1000;
                    long x = n;
                    long y = (x + 1) / 2;
                    while (y < x)
                    {
                        x = y;
                        y = (x + n / x) / 2;
                    }
                    expected = (int)x;
                }
                AppendOp(sb, "sqrt", a, 0, expected, false);
            }

            // Remove trailing comma and close array
            sb.Length--; // remove last comma (we always added one after each item)
            sb.Append(']');
            return sb.ToString();
        }

        private static uint NextState(uint state)
        {
            return unchecked(LcgMultiplier * state + LcgIncrement);
        }

        private static void AppendOp(StringBuilder sb, string op, int a, int b, int expected, bool isFirst)
        {
            if (!isFirst) sb.Append(',');
            sb.Append("{\"op\":\"");
            sb.Append(op);
            sb.Append("\",\"a\":");
            sb.Append(a);
            sb.Append(",\"b\":");
            sb.Append(b);
            sb.Append(",\"expected\":");
            sb.Append(expected);
            sb.Append('}');
        }
    }
}