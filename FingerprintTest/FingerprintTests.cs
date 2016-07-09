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
            var bytes = new List<byte> { 0x1, 0xe };

            //Act
            var sensor = new FingerPrintSensor("COM1");
            var sum = sensor.AddCheckSum(bytes);

            //Assert
            //var expectedSum = new List<byte> { 0x1, 0xe, 0x0, 0xf };
            Assert.AreSame(new byte[] { 0x1, 0xe, 0x0, 0xf }, sum);
        }
    }
}
