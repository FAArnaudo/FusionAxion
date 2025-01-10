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
    public class ConnectorCemTest
    {
        private Mock<IConnections> connections;
        private ConnectorCem connectorCem;
        private Data data;

        [TestInitialize]
        public void TestInitialize()
        {
            connections = new Mock<IConnections>();

            connectorCem = new ConnectorCem(connections.Object);

            data = new Data
            {
                IP = "10.773.856",
                Protocol = "16",
                Modo = "NORMAL"
            };

            _ = Configuration.SaveConfiguration(data);

            _ = connections.Setup(a => a.GetConfiguration()).Returns(data);
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
        public void PoleoEnLinea_ReturnTrue()
        {
            // Arange
            byte[] command = new byte[] { 0x00 };

            _ = connections.Setup(a => a.EnviarComando(command)).Returns(new byte[] { 0x00 });

            _ = connections.Setup(a => a.GetConfiguration()).Returns(data);

            //Act
            bool actual = connectorCem.PoleoEnLinea(command);

            //Assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void PoleoEnLinea_ReturnFalse_InBuffer()
        {
            // Arange
            byte[] command = new byte[] { 0x00 };

            _ = connections.Setup(a => a.EnviarComando(command)).Returns(new byte[] { 0x01 });

            //Act
            bool actual = connectorCem.PoleoEnLinea(command);

            //Assert
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void PoleoEnLinea_ThrowException()
        {
            // Arange
            byte[] command = new byte[] { 0x00 };

            _ = connections.Setup(a => a.EnviarComando(command)).Returns(new byte[] { });

            // Act
            Exception ex = Assert.ThrowsException<Exception>(() => connectorCem.PoleoEnLinea(command));

            // Assert
        }

        [TestMethod]
        public void ComandoConfiguracionDeLaEstacion_ReturnNotNull()
        {
            // Arange
            byte[] reply = connectorCem.ReadAnswer("ConfiguracionDeLaEstacion");

            byte[] command = new byte[] { 0x65 };

            _ = connections.Setup(a => a.EnviarComando(command)).Returns(reply);

            //Act
            Station actual = connectorCem.ComandoConfiguracionDeLaEstacion(command);

            //Assert
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        public void ComandoConfiguracionDeLaEstacion_ThrowException()
        {
            // Arange
            byte[] command = new byte[] { 0x65 };

            _ = connections.Setup(a => a.EnviarComando(command)).Returns(new byte[] { });

            //Act
            Exception ex = Assert.ThrowsException<Exception>(() => connectorCem.ComandoConfiguracionDeLaEstacion(command));

            //Assert
        }

        [TestMethod]
        public void ComandoConfiguracionDeLaEstacion_ThrowException_NotConfirmation()
        {
            // Arange
            byte[] command = new byte[] { 0x65 };

            _ = connections.Setup(a => a.EnviarComando(command)).Returns(new byte[] { 0x01 });

            string expected = "Error al obtener la configuración de la estación. Excepción: No se recibió mensaje de confirmación al solicitar la configuración de la estación.";

            //Act
            Exception actual = Assert.ThrowsException<Exception>(() => connectorCem.ComandoConfiguracionDeLaEstacion(command));

            //Assert
            Assert.AreEqual(expected, actual.Message);
        }

        [TestMethod]
        public void ComandoStockDeTanques_ReturnNotNull()
        {
            // Arange
            byte[] reply = connectorCem.ReadAnswer("StockDeTanques");

            byte[] command = new byte[] { 0x68 };

            _ = connections.Setup(a => a.EnviarComando(command)).Returns(reply);

            //Act
            List<Tanque> actual = connectorCem.ComandoStockDeTanques(command);

            // Assert
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        public void ComandoStockDeTanques_ReturnNull_NotConfirmation()
        {
            // Arange
            byte[] reply = new byte[] { 0x01 };

            byte[] command = new byte[] { 0x68 };

            _ = connections.Setup(a => a.EnviarComando(command)).Returns(reply);

            //Act
            List<Tanque> actual = connectorCem.ComandoStockDeTanques(command);

            // Assert
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void GrabarDespachos_ReturnNotNull()
        {
            // Arange

            //Act

            // Assert
        }

        [TestMethod]
        public void GrabarDespachos_ReturnNotNull_WithoutSales()
        {
            // Arange

            //Act

            // Assert
        }

        [TestMethod]
        public void GrabarDespachos_ReturnNull_NotConfirmation()
        {
            // Arange

            //Act

            // Assert
        }
    }
}
