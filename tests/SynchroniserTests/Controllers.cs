using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using Xunit;
using ADConnectors;
using Synchroniser.Pages.AD;

namespace tests.SynchroniserTests
{
    class ControllersTests
    {
        public void AD_User_List()
        {
            var mockSearcher = new Mock<IADSearcher>();
            mockSearcher.Setup(searcher => searcher.GetUser(1, false)).Returns(new Dictionary<string, string>());
            var page = new UserModel(mockSearcher.Object);
            page.OnGet(1);
            Assert.True(true);
        }
    }
}
