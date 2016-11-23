using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using FingerPrintLibrary;
using FingerprintTest;
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

            var storeTemplate = DataPackageUtilities.StoreTemplate(0x00, new byte[] { 0x00, 0x00 });

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

        [TestMethod]
        public void TestNextAvailablePosition()
        {
            short position;
            bool result;

            //Test sequential list starting at 0
            var positions = new List<int>() { 0, 1, 2 };
            FingerPrintSensor.DetermineNextAvailablePosition(out position, positions, 120);
            Assert.AreEqual(3, position);

            //Test list with gap
            positions.Add(5);
            FingerPrintSensor.DetermineNextAvailablePosition(out position, positions, 120);
            Assert.AreEqual(3, position);

            //Test list with no elements
            positions = new List<int>();
            FingerPrintSensor.DetermineNextAvailablePosition(out position, positions, 120);
            Assert.AreEqual(0, position);

            //Test list where the first element is missing.
            positions = new List<int>() { 1, 2, 4, 5 };
            FingerPrintSensor.DetermineNextAvailablePosition(out position, positions, 120);
            Assert.AreEqual(0, position);

            //Test list that's at capacity
            positions = new List<int>();
            for (int i = 0; i < 120; i++)
            {
                positions.Add(i);
            }
            result = FingerPrintSensor.DetermineNextAvailablePosition(out position, positions, 120);
            Assert.AreEqual(false, result);
            Assert.AreEqual(-1, position);

            //Test list one under capacity
            positions = new List<int>();
            for (int i = 0; i < 119; i++)
            {
                positions.Add(i);
            }
            FingerPrintSensor.DetermineNextAvailablePosition(out position, positions, 120);
            Assert.AreEqual(119, position);
        }
    }
}
