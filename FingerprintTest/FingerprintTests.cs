using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using FingerPrintLibrary;
using System.Collections.Generic;

namespace FingerprintTest
{
    [TestClass]
    public class FingerprintTests
    {
        [TestMethod]
        public void TestGetCheckSum()
        {
            //Arrange
            var bytes = DataPackageUtilities.DataPackageStart(0x01);

            //Act
            var sum = DataPackageUtilities.AddCheckSum(bytes);
            var expectedSum = new byte[9] { 0xEF, 0x1, 0xFF, 0xFF, 0xFF, 0xFF, 0x01, 0x00, 0x01 };

            //Assert            
            CollectionAssert.AreEqual(expectedSum, sum);
        }

        [TestMethod]
        public void TestHeaderGeneration()
        {
            var header = DataPackageUtilities.DataPackageStart(0x01);

            var expected = new byte[7] { 0xEF, 0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x01 };

            CollectionAssert.AreEqual(expected, header);
        }

        [TestMethod]
        public void TestHandshakeGeneration()
        {
            var command = DataPackageUtilities.Handshake();

            var expectedCommand = new byte[13] { 0xEF, 0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x01, 0x00, 0x04, 0x17, 0x00, 0x00, 0x1C };

            CollectionAssert.AreEqual(expectedCommand, command);
        }

        [TestMethod]
        public void TestParseReturn()
        {
            var commandToParse = new byte[13] { 0xEF, 0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x01, 0x00, 0x04, 0x17, 0x00, 0x00, 0x1C };

            var expectedByte = 0x01;

            Assert.AreEqual(expectedByte, DataPackageUtilities.ParsePackageIdentifier(commandToParse));
        }

        [TestMethod]
        public void TestStoreTemplate()
        {
            var expected = new byte[13] { 0xEF, 0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x01, 0x00, 0x06, 0x06, 0x00, 0x00, 0x00 };
            expected = DataPackageUtilities.AddCheckSum(new List<byte>(expected));

            var storeTemplate = DataPackageUtilities.StoreTemplate(0x00, new byte[] { 0x00 });

            CollectionAssert.AreEqual(expected, storeTemplate);
        }

        [TestMethod]
        public void TestGenerateTemplate()
        {
            var expected = new byte[12] { 0xEF, 0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x01, 0x00, 0x03, 0x05, 0x00, 0x09 };

            var generateTemplate = DataPackageUtilities.GenerateTemplate();

            CollectionAssert.AreEqual(expected, generateTemplate);
        }

        [TestMethod]
        public void TestValidateCheckSum()
        {
            var expectedCommand = new byte[13] { 0xEF, 0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x01, 0x00, 0x04, 0x17, 0x00, 0x00, 0x1C };

            var result = DataPackageUtilities.ValidateCheckSum(expectedCommand);

            Assert.IsTrue(result);
        }
    }
}
