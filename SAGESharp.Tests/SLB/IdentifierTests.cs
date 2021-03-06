/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */
using FluentAssertions;
using NUnit.Framework;
using NUtils.Extensions;
using SAGESharp.SLB;
using System;
using System.Reflection;

namespace SAGESharp.Tests.SLB
{
    class IdentifierTests
    {
        [Test]
        public void Test_Cast_Integer_To_Identifier()
        {
            int value = 0x11223344;
            Identifier identifier = value;
            // For the nature of the "Identifier" type
            // we make an exception here testing private members
            // to ensure the correct behavior of the casting operator
            var field = identifier
                .GetType()
                .GetField("value", BindingFlags.NonPublic | BindingFlags.Instance);

            field.GetValue(identifier).Should().Be(value);
        }

        [Test]
        public void Test_Case_Unsigned_Integer_To_Identifier()
        {
            uint value = 0x44434241;
            Identifier identifier = value;
            // For the nature of the "Identifier" type
            // we make an exception here testing private members
            // to ensure the correct behavior of the casting operator
            var field = identifier
                .GetType()
                .GetField("value", BindingFlags.NonPublic | BindingFlags.Instance);

            field.GetValue(identifier).Should().Be(value);
        }

        [Test]
        public void Test_Create_Identifier_From_Byte_Array()
        {
            byte[] input = new byte[] { 0x11, 0x22, 0x33, 0x44 };

            Identifier result = Identifier.From(input);

            result.Should().Be((Identifier)0x44332211);
        }

        [TestCaseSource(nameof(IncorrectByteArrayTestCases))]
        public void Test_Creating_An_Identifier_From_An_incorrect_Byte_Array(IncorrectByteArrayTestCase testCase)
        {
            Action action = () => Identifier.From(testCase.Values);

            action.Should()
                .ThrowExactly<ArgumentException>()
                .WithMessage("Input is not 4 bytes long.");
        }

        static IncorrectByteArrayTestCase[] IncorrectByteArrayTestCases() => new IncorrectByteArrayTestCase[]
        {
            new IncorrectByteArrayTestCase(
                values: new byte[0],
                description: "Test creating an Identifier from an empty array."
            ),
            new IncorrectByteArrayTestCase(
                values: new byte[1],
                description: "Test creating an Identifier from an array with only one element."
            ),
            new IncorrectByteArrayTestCase(
                values: new byte[5],
                description: "Test creating an Identifier from an array with more than four elements."
            )
        };

        public class IncorrectByteArrayTestCase : AbstractTestCase
        {
            public IncorrectByteArrayTestCase(byte[] values, string description) : base(description)
            {
                Values = values;
            }

            public byte[] Values { get; }
        }

        [Test]
        public void Test_Create_Identifier_From_Null_Byte_Array_Should_Throw_ArgumentNullException()
        {
            Action action = () => Identifier.From((byte[])null);

            action.Should()
                .ThrowArgumentNullException("values");
        }

        [TestCaseSource(nameof(IdentifierFromStingTestCases))]
        public void Test_Create_Identifier_From_String(IdentifierFromStringTestCase testCase)
        {
            Identifier result = Identifier.From(testCase.Value);

            result.Should().Be(testCase.Expected);
        }

        static IdentifierFromStringTestCase[] IdentifierFromStingTestCases() => new IdentifierFromStringTestCase[]
        {
            new IdentifierFromStringTestCase(
                value: "Id01",
                expected: 0x49643031,
                description: "Creating an identifier form a string with a digit character"
            ),
            new IdentifierFromStringTestCase(
                value: "val|0xab|",
                expected: 0x76616CAB,
                description: "Creating an identifier form a string with an escaped byte as a lowercase hexadecimal"
            ),
            new IdentifierFromStringTestCase(
                value: "val|0xEF|",
                expected: 0x76616CEF,
                description: "Creating an identifier form a string with an escaped byte as an uppercase hexadecimal"
            ),
            new IdentifierFromStringTestCase(
                value: "val|1|",
                expected: 0x76616C01,
                description: "Creating an identifier form a string with an escaped byte as decimal"
            ),
            new IdentifierFromStringTestCase(
                value: "|0x10||0x23||0x7B||0xB6|",
                expected: 0x10237BB6,
                description: "Creating an identifier form a string with only escaped characters"
            )
        };

        public class IdentifierFromStringTestCase : AbstractTestCase
        {
            public IdentifierFromStringTestCase(string value, Identifier expected, string description) : base(description)
            {
                Value = value;
                Expected = expected;
            }

            public string Value { get; }

            public Identifier Expected { get; }
        }

        [TestCaseSource(nameof(IdentifierFromInvalidStringTestCases))]
        public void Test_Create_Identifier_From_Invalid_String(IdentifierFromInvalidStringTestCase testCase)
        {
            Action action = () => Identifier.From(testCase.Value);

            action.Should()
                .ThrowExactly<ArgumentException>()
                .WithMessage($"\"{testCase.Value}\" is not a valid Identifier.");
        }

        static IdentifierFromInvalidStringTestCase[] IdentifierFromInvalidStringTestCases() => new IdentifierFromInvalidStringTestCase[]
        {
            new IdentifierFromInvalidStringTestCase(
                value: string.Empty,
                description: "Creating an identifier form a string with no characters"
            ),
            new IdentifierFromInvalidStringTestCase(
                value: "A",
                description: "Creating an identifier from a string with a single character"
            ),
            new IdentifierFromInvalidStringTestCase(
                value: "FEDCBA",
                description: "Creating an identifier from a string with more than four characters"
            ),
            new IdentifierFromInvalidStringTestCase(
                value: "FED|0",
                description: "Creating an identifier from a string with with bad escaping"
            )
        };

        public class IdentifierFromInvalidStringTestCase : AbstractTestCase
        {
            public IdentifierFromInvalidStringTestCase(string value, string description) : base(description)
            {
                Value = value;
            }

            public string Value { get; }
        }

        [Test]
        public void Test_Create_Identifier_From_Null_String_Should_Throw_ArgumentNullException()
        {
            Action action = () => Identifier.From((string)null);

            action.Should()
                .ThrowArgumentNullException("value");
        }

        [Test]
        public void Test_Getting_Identifier_Individual_Bytes()
        {
            Identifier value = 0x11223344;

            value.B0.Should().Be(0x11);
            value.B1.Should().Be(0x22);
            value.B2.Should().Be(0x33);
            value.B3.Should().Be(0x44);
        }

        [Test]
        public void Test_Getting_Identifier_Individual_Bytes_As_Chars()
        {
            Identifier value = Identifier.From("Id01");

            value.C0.Should().Be('I');
            value.C1.Should().Be('d');
            value.C2.Should().Be('0');
            value.C3.Should().Be('1');
        }

        [Test]
        public void Test_Cast_Identifier_To_Integer()
            => ((Identifier)0x11223344).Let(i => (int)i).Should().Be(0x11223344);

        [Test]
        public void Test_Cast_Identifier_To_Unsigned_Integer()
            => ((Identifier)0x44434241).Let(i => (uint)i).Should().Be(0x44434241);

        [TestCaseSource(nameof(ToStringTestCases))]
        public void Test_Identifier_ToString(ToStringTestCase testCase)
        {
            string result = testCase.Value.ToString();

            result.Should().Be(testCase.Expected);
        }

        static ToStringTestCase[] ToStringTestCases() => new ToStringTestCase[]
        {
            new ToStringTestCase(
                value: 0x546F6130,
                expected: "Toa0",
                description: "Test converting an identifier with only alphanumerical characters to a string"
            ),
            new ToStringTestCase(
                value: 0x546F617B,
                expected: "Toa|0x7B|",
                description: "Test converting an identifier with alphanumerical and symbols to a string"
            ),
            new ToStringTestCase(
                value: 0x101F9AEF,
                expected: "|0x10||0x1F||0x9A||0xEF|",
                description: "Test converting an identifier with only symbols to a string"
            ),
            new ToStringTestCase(
                value: Identifier.Zero,
                expected: "|0x00||0x00||0x00||0x00|",
                description: "Test converting an identifier with value 0 to a string"
            )
        };

        public class ToStringTestCase : AbstractTestCase
        {
            public ToStringTestCase(Identifier value, string expected, string description) : base(description)
            {
                Value = value;
                Expected = expected;
            }

            public Identifier Value { get; }

            public string Expected { get; }
        }

        [TestCaseSource(nameof(ModifyCopyIdentifierTestCases))]
        public void Test_Modify_An_Identifier_Copy(ModifyIdentifierCopyTestCase testCase)
            => testCase.Execute();

        static ModifyIdentifierCopyTestCase[] ModifyCopyIdentifierTestCases() => new ModifyIdentifierCopyTestCase[]
        {
            new ModifyIdentifierCopyTestCase(
                value: 0x12345678,
                getModifiedIdentifierCopy: identifier => identifier.WithB0(0x00),
                expected: 0x00345678,
                description: "Test changing the byte 0 of an identifier"
            ),
            new ModifyIdentifierCopyTestCase(
                value: 0x12345678,
                getModifiedIdentifierCopy: identifier => identifier.WithB1(0x00),
                expected: 0x12005678,
                description: "Test changing the byte 1 of an identifier"
            ),
            new ModifyIdentifierCopyTestCase(
                value: 0x12345678,
                getModifiedIdentifierCopy: identifier => identifier.WithB2(0x00),
                expected: 0x12340078,
                description: "Test changing the byte 2 of an identifier"
            ),
            new ModifyIdentifierCopyTestCase(
                value: 0x12345678,
                getModifiedIdentifierCopy: identifier => identifier.WithB3(0x00),
                expected: 0x12345600,
                description: "Test changing the byte 3 of an identifier"
            ),
            new ModifyIdentifierCopyTestCase(
                value: Identifier.From("ABCD"),
                getModifiedIdentifierCopy: identifier => identifier.WithC0(' '),
                expected: Identifier.From(" BCD"),
                description: "Test changing the char 0 of an identifier"
            ),
            new ModifyIdentifierCopyTestCase(
                value: Identifier.From("ABCD"),
                getModifiedIdentifierCopy: identifier => identifier.WithC1(' '),
                expected: Identifier.From("A CD"),
                description: "Test changing the char 1 of an identifier"
            ),
            new ModifyIdentifierCopyTestCase(
                value: Identifier.From("ABCD"),
                getModifiedIdentifierCopy: identifier => identifier.WithC2(' '),
                expected: Identifier.From("AB D"),
                description: "Test changing the char 2 of an identifier"
            ),
            new ModifyIdentifierCopyTestCase(
                value: Identifier.From("ABCD"),
                getModifiedIdentifierCopy: identifier => identifier.WithC3(' '),
                expected: Identifier.From("ABC "),
                description: "Test changing the char 3 of an identifier"
            )
        };

        public class ModifyIdentifierCopyTestCase : AbstractTestCase
        {
            private readonly Identifier value;

            private readonly Func<Identifier, Identifier> getModifiedIdentifierCopy;

            private readonly Identifier expected;

            public ModifyIdentifierCopyTestCase(
                Identifier value,
                Func<Identifier, Identifier> getModifiedIdentifierCopy,
                Identifier expected,
                string description
            ) : base(description) {
                this.value = value;
                this.getModifiedIdentifierCopy = getModifiedIdentifierCopy;
                this.expected = expected;
            }

            public void Execute()
            {
                Identifier result = getModifiedIdentifierCopy(value);

                result.Should().Be(expected);
            }
        }

        [TestCaseSource(nameof(EqualObjectsTestCases))]
        public void Test_Comparing_Equal_Objects(IComparisionTestCase<Identifier> testCase) => testCase.Execute();

        public static IComparisionTestCase<Identifier>[] EqualObjectsTestCases() => new IComparisionTestCase<Identifier>[]
        {
            ComparisionTestCase.CompareObjectAgainstItself(SampleIdentifier()),
            ComparisionTestCase.CompareTwoEqualObjects(SampleIdentifier)
        };

        [TestCaseSource(nameof(NotEqualObjectsTestCases))]
        public void Test_Comparing_NotEqual_Objects(IComparisionTestCase<Identifier> testCase) => testCase.Execute();

        public static IComparisionTestCase<Identifier>[] NotEqualObjectsTestCases() => new IComparisionTestCase<Identifier>[]
        {
            ComparisionTestCase.CompareTwoNotEqualObjects(SampleIdentifier(), SampleIdentifier().WithB0(0x01)),
            ComparisionTestCase.CompareTwoNotEqualObjects(SampleIdentifier(), SampleIdentifier().WithB1(0x01)),
            ComparisionTestCase.CompareTwoNotEqualObjects(SampleIdentifier(), SampleIdentifier().WithB2(0x01)),
            ComparisionTestCase.CompareTwoNotEqualObjects(SampleIdentifier(), SampleIdentifier().WithB3(0x01)),
            ComparisionTestCase.CompareTwoNotEqualObjects(SampleIdentifier(), SampleIdentifier().WithC0('A')),
            ComparisionTestCase.CompareTwoNotEqualObjects(SampleIdentifier(), SampleIdentifier().WithC1('B')),
            ComparisionTestCase.CompareTwoNotEqualObjects(SampleIdentifier(), SampleIdentifier().WithC2('C')),
            ComparisionTestCase.CompareTwoNotEqualObjects(SampleIdentifier(), SampleIdentifier().WithC3('D')),
            ComparisionTestCase.CompareTwoNotEqualObjects(SampleIdentifier(), (Identifier)1),
            ComparisionTestCase.CompareTwoNotEqualObjects(SampleIdentifier(), Identifier.From(new byte[] { 0x01, 0x02, 0x03, 0x04 })),
            ComparisionTestCase.CompareTwoNotEqualObjects(SampleIdentifier(), Identifier.From("BCDA")),
            ComparisionTestCase.CompareTwoNotEqualObjects((Identifier)0x11223344, (Identifier)0x11121314)
        };

        public static Identifier SampleIdentifier() => 0xAABBCCDD;
    }
}
