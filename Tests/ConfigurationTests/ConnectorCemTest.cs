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
        [TestInitialize]
        public void TestInitialize()
        {
            Data data = new Data
            {
                IP = "10.773.856",
                Protocol = "16",
                Modo = "NORMAL"
            };

            _ = Configuration.SaveConfiguration(data);
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
            Mock<IConnections> connections = new Mock<IConnections>();

            byte[] command = new byte[] { 0x00 };

            _ = connections.Setup(a => a.EnviarComando(command)).Returns(new byte[] { 0x00 });

            ConnectorCem connectorCem = new ConnectorCem(connections.Object);

            //Act
            bool actual = connectorCem.PoleoEnLinea(command);

            //Assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void PoleoEnLinea_ReturnFalse_InBuffer()
        {
            // Arange
            Mock<IConnections> connections = new Mock<IConnections>();

            byte[] command = new byte[] { 0x00 };

            _ = connections.Setup(a => a.EnviarComando(command)).Returns(new byte[] { 0x01 });

            ConnectorCem connectorCem = new ConnectorCem(connections.Object);

            //Act
            bool actual = connectorCem.PoleoEnLinea(command);

            //Assert
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void PoleoEnLinea_ThrowException()
        {
            // Arange
            Mock<IConnections> connections = new Mock<IConnections>();

            byte[] command = new byte[] { 0x00 };

            _ = connections.Setup(a => a.EnviarComando(command)).Returns(new byte[] { });

            ConnectorCem connectorCem = new ConnectorCem(connections.Object);

            // Act
            Exception ex = Assert.ThrowsException<Exception>(() => connectorCem.PoleoEnLinea(command));

            // Assert
        }

        [TestMethod]
        public void ComandoConfiguracionDeLaEstacion_ReturnNotNull()
        {
            // Arange
            Mock<IConnections> connections = new Mock<IConnections>();

            ConnectorCem connectorCem = new ConnectorCem(connections.Object);

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
            Mock<IConnections> connections = new Mock<IConnections>();

            ConnectorCem connectorCem = new ConnectorCem(connections.Object);

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
            Mock<IConnections> connections = new Mock<IConnections>();

            ConnectorCem connectorCem = new ConnectorCem(connections.Object);

            byte[] command = new byte[] { 0x65 };

            _ = connections.Setup(a => a.EnviarComando(command)).Returns(new byte[] { 0x01 });

            string expected = "Error al obtener la configuración de la estación. Excepción: No se recibió mensaje de confirmación al solicitar la configuración de la estación.";

            //Act
            Exception actual = Assert.ThrowsException<Exception>(() => connectorCem.ComandoConfiguracionDeLaEstacion(command));

            //Assert
            Assert.AreEqual(expected, actual.Message);
        }
    }
}
