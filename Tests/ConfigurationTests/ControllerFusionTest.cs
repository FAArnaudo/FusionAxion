using CDS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationTests
{
    [TestClass]
    public class ControllerFusionTest
    {
        private Mock<ICommunication> communication;
        private ControllerFusion controllerFusion;
        private Data data;

        [TestInitialize]
        public void TestInitialize()
        {
            communication = new Mock<ICommunication>();

            controllerFusion = new ControllerFusion(communication.Object);

            data = new Data
            {
                IP = "200.85.107.15",
                Protocol = "16",
                Modo = "NORMAL",
                StationFlag = "PUMA"
            };

            _ = communication.Setup(a => a.GetConfiguration()).Returns(data);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            // Elimina la base de datos si existe
            if (File.Exists(Environment.CurrentDirectory + "/Config.ini"))
            {
                File.Delete(Environment.CurrentDirectory + "/Config.ini");
            }
        }

        [TestMethod]
        public void VerificarConexion_ReturnTrue()
        {
            // Arange
            string query = $"UPDATE CheckConnection " +
                           $"SET isConnected = 1, fecha = '{DateTime.Now:dd-MM-yyyy HH:mm:ss}' " +
                           $"WHERE idConnection = 1";

            _ = communication.Setup(a => a.ExecuteNonQuery(query)).Returns(1);

            bool expected = true;

            // Act
            bool actual = controllerFusion.VerificarConexión();

            // Assert
            Assert.AreEqual(expected, actual);
        }
    }
}
