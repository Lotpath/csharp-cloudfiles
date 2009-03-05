using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace com.mosso.cloudfiles.unit.tests.StringHelperSpecs
{
    [TestFixture]
    public class when_capitalizing_a_string
    {
        [Test]
        public void should_capitalize_the_first_letter_and_lower_case_the_remainder_of_the_string()
        {
            var howdy = "howdy";
            Assert.That(howdy.Capitalize(), Is.EqualTo("Howdy"));
        }
    }

    [TestFixture]
    public class when_converting_true_to_string_for_x_cdn_enabled_header
    {
        [Test]
        public void should_result_in_True()
        {
            Assert.That(true.Capitalize(), Is.EqualTo("True"));
        }
    }

    [TestFixture]
    public class when_converting_false_to_string_for_x_cdn_enabled_header
    {
        [Test]
        public void should_result_in_False()
        {
            Assert.That(false.Capitalize(), Is.EqualTo("False"));
        }
    }
}