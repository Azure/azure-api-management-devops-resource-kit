using System;
using Xunit;
using APIManagement.Template;
using McMaster.Extensions.CommandLineUtils;

namespace apimtemplate.test
{
    public class Create
    {

        [Fact]
        public void ShouldFailWithUnknownCommand()
        {

            var createCommand = new CreateCommand();

            var ex = Assert.ThrowsAny<CommandParsingException>(() => createCommand.Execute("test"));
            //Console.WriteLine(ex.Message);
            Assert.Contains("Unrecognized command or argument 'test'", ex.Message);
        }
    }
}
