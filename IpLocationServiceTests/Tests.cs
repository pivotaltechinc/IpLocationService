using System;
using System.Threading.Tasks;
using IpLocationService.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IpLocationServiceTests
{
    [TestClass]
    public class Tests
    {
        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            _Service = new IpLocationService.Service();
        }

        private static IpLocationService.Service _Service { get; set; }

        [TestMethod]
        [DataRow("13.82.28.61")]
        [DataRow("151.101.129.69")]
        public void Get_Test(string ip)
        {
            var response = _Service.Get(ip);
            Assert.IsTrue(response != null && !string.IsNullOrWhiteSpace(response.countryCode));
        }

        [TestMethod]
        [DataRow("13.82.28.61")]
        public void GetWithFields_Test(string ip)
        {
            var fields = new IpLocationService.Enums.FieldEnum[]
            {
                IpLocationService.Enums.FieldEnum.country,
                IpLocationService.Enums.FieldEnum.countryCode
            };
            var response = _Service.Get(ip, fields);
            Assert.IsTrue(response != null && !string.IsNullOrWhiteSpace(response.countryCode) && string.IsNullOrWhiteSpace(response.city));
        }

        [TestMethod]
        [DataRow("pivotaltechnology")]
        [DataRow("11.happy.hour.99")]
        public void Get_Fail_Test(string ip)
        {
            try
            {
                var response = _Service.Get(ip);
            }
            catch(Exception ex)
            {
                Assert.IsTrue(ex != null && ex.Message.Contains("valid"));
            }
        }

        [TestMethod]
        [DataRow("13.82.28.61")]
        public async Task Get_LimitExceeded_Test(string ip)
        {
            try
            {
                for (int i = 0; i < 46; i++)
                {
                    await _Service.GetAsync(ip);
                }
            }
            catch(Exception ex)
            {
                Assert.IsTrue(ex != null && !string.IsNullOrWhiteSpace(ex.Message));
            }
        }

        [TestMethod]
        public void GetBatch_Test()
        {
            string[] ips = new string[] { "13.82.28.61", "151.101.129.69" };
            var response = _Service.GetBatch(ips);
            Assert.IsTrue(response != null && response.Length > 0);
        }

        [TestMethod]
        public void GetBatchWithFields_Test()
        {
            string[] ips = new string[] { "13.82.28.61", "151.101.129.69" };
            var fields = new IpLocationService.Enums.FieldEnum[]
            {
                IpLocationService.Enums.FieldEnum.country,
                IpLocationService.Enums.FieldEnum.countryCode
            };
            var response = _Service.GetBatch(ips, fields);
            Assert.IsTrue(response != null && response.Length > 0 && !string.IsNullOrWhiteSpace(response[0].countryCode) && string.IsNullOrWhiteSpace(response[0].city));
        }
    }
}
