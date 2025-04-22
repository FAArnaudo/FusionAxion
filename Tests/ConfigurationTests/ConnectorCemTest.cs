using CDS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;

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

            string expected = "Error al obtener la configuración de la estación. Excepción: No se recibió mensaje de confirmación al solicitar la configuración de la estación.\n";

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
            int surtidor = 2;

            byte[] reply = connectorCem.ReadAnswer("Despacho-2");

            byte[] command = new byte[] { (byte)(0x70 + Convert.ToByte(surtidor)) };

            _ = connections.Setup(a => a.EnviarComando(command)).Returns(reply);

            //Act
            DespachoCem despacho = connectorCem.ComandoInformacionDeDespacho(command);

            // Assert
            Assert.IsNotNull(despacho);
        }

        [TestMethod]
        public void GrabarDespachos_ReturnNotNull_MaxPump()
        {
            // Arange
            int numeroDeSurtidor = 0;                               // Representa el surtidor N° 16 si el protocolo es 16

            byte[] reply = connectorCem.ReadAnswer("Despacho-0");

            byte[] command = new byte[] { (byte)(0x70 + Convert.ToByte(numeroDeSurtidor)) };

            _ = connections.Setup(a => a.EnviarComando(command)).Returns(reply);

            //Act
            Despacho despacho = connectorCem.ComandoInformacionDeDespacho(command);

            // Assert
            Assert.IsNotNull(despacho);
        }

        [TestMethod]
        public void GrabarDespachos_ReturnNull_WithoutSales()
        {
            // Arange
            int numeroDeSurtidor = 0;                               // Representa el surtidor N° 16 si el protocolo es 16

            byte[] reply = new byte[] { 0x00, 0x03 };

            byte[] command = new byte[] { (byte)(0x70 + Convert.ToByte(numeroDeSurtidor)) };

            _ = connections.Setup(a => a.EnviarComando(command)).Returns(reply);

            //Act
            Despacho despacho = connectorCem.ComandoInformacionDeDespacho(command);

            // Assert
            Assert.IsNull(despacho);
        }

        [TestMethod]
        public void GrabarDespachos_ReturnNull_NotConfirmation()
        {
            // Arange
            int numeroDeSurtidor = 0;                               // Representa el surtidor N° 16 si el protocolo es 16

            byte[] reply = new byte[] { 0x01 };

            byte[] command = new byte[] { (byte)(0x70 + Convert.ToByte(numeroDeSurtidor)) };

            _ = connections.Setup(a => a.EnviarComando(command)).Returns(reply);

            //Act
            Despacho despacho = connectorCem.ComandoInformacionDeDespacho(command);

            // Assert
            Assert.IsNull(despacho);
        }

        [TestMethod]
        public void GrabarDespachos_ReturnNull_InException()
        {
            // Arange
            int numeroDeSurtidor = 0;                               // Representa el surtidor N° 16 si el protocolo es 16

            byte[] command = new byte[] { (byte)(0x70 + Convert.ToByte(numeroDeSurtidor)) };

            _ = connections.Setup(a => a.EnviarComando(command)).Throws(new Exception());

            //Act
            Despacho despacho = connectorCem.ComandoInformacionDeDespacho(command);

            // Assert
            Assert.IsNull(despacho);
        }

        [TestMethod]
        public void ComandoCierres_CierreDeTurno_SinVentas()
        {
            // Aragne
            byte[] reply = connectorCem.ReadAnswer("ConfiguracionDeLaEstacion");

            byte[] command = new byte[] { 0x65 };

            _ = connections.Setup(a => a.EnviarComando(command)).Returns(reply);

            _ = connectorCem.ComandoConfiguracionDeLaEstacion(command);

            reply = new byte[] { 0xFF };

            command = new byte[] { 0x07 };

            _ = connections.Setup(a => a.EnviarComando(command)).Returns(reply);

            string expected = "SIN VENTAS";

            // Act
            CierreCem cierreDeTurno = connectorCem.ComandoCierresDeTurno(command);

            string actual = cierreDeTurno.Estado;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ComandoCierres_CierreDeTurnoAnterior_ThorwException()
        {
            // Aragne
            byte[] command = new byte[] { 0x0B };

            byte[] reply = null;

            _ = connections.Setup(a => a.EnviarComando(command)).Returns(reply);

            string campos = "state";

            string rows = "ERROR";

            _ = connections.Setup(b => b.ExecuteNonQuery(string.Format("INSERT INTO Cierres ({0}) VALUES ({1})", campos, rows))).Returns(1);
            _ = connections.Setup(c => c.ExecuteNonQuery("UPDATE cierreBandera SET hacerCierre = 0")).Returns(1);

            string expected = "Error al pedir intormacion del CierreDeTurnoAnterior";

            // Act
            Exception actual = Assert.ThrowsException<Exception>(() => connectorCem.ComandoCierresDeTurno(command));

            // Assert
            Assert.AreEqual(expected, actual.Message.Substring(0, 52));
        }
    }
}
