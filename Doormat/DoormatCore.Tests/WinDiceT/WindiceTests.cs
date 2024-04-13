using DoormatCore.Sites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DoormatCore.Tests.WinDiceT
{
    public class WindiceTests : BaseSiteTests, IClassFixture<WindiceFixture>
    {
        public WindiceTests(WindiceFixture fixure) : base(fixure.site)
        {

        }

        [Fact]
        public override void a2_LogInWithout2faWhenRequiredShouldNotLogIn()
        {
            //test s not applicable
            Assert.True(true);
        }
        [Fact]
        public override void a3_LogInWith2faWhenNotRequiredShouldLogIn()
        {
            //test s not applicable
            Assert.True(true);
        }
        [Fact]
        public override void a4_LogInWit2faWhenRequiredShouldLogIn()
        {
            //test s not applicable
            Assert.True(true);
        }
    }
}
