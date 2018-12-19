using System;
using Xunit;
using McMaster.Extensions.CommandLineUtils;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    public class CreateTests
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
