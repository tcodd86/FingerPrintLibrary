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
            var sensor = new FingerPrintSensor("COM1");
            var bytes = sensor.GenerateDataPackageStart(0x01);

            //Act
            var sum = sensor.AddCheckSum(bytes);
            var expectedSum = new byte[9] { 0xEF, 0x1, 0xFF, 0xFF, 0xFF, 0xFF, 0x01, 0x00, 0x01 };

            //Assert            
            Assert.AreEqual(expectedSum.Length, sum.Length);
            for (int i = 0; i < sum.Length; i++)
            {
                Assert.AreEqual(expectedSum[i], sum[i]);
            }

            sensor.Disposeserial();
        }

        [TestMethod]
        public void TestHeaderGeneration()
        {
            var sensor = new FingerPrintSensor("COM1");
            var header = sensor.GenerateDataPackageStart(0x01);

            var expected = new byte[7] { 0xEF, 0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x01 };

            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], header[i]);
            }

            sensor.Disposeserial();
        }

        [TestMethod]
        public void TestHandshakeGeneration()
        {
            var sensor = new FingerPrintSensor("COM1");

            var command = sensor.GenerateHandshakeInstruction();

            var expectedCommand = new byte[13] { 0xEF, 0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x01, 0x00, 0x04, 0x17, 0x00, 0x00, 0x1C };

            for (int i = 0; i < expectedCommand.Length; i++)
            {
                Assert.AreEqual(expectedCommand[i], command[i]);
            }

            sensor.Disposeserial();
        }
    }
}
