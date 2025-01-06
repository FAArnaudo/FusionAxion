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
        [TestMethod]
        public void PoleoEnLinea_ReturnTrue()
        {
            // Arange
            Mock<IConnections> connections = new Mock<IConnections>();

            byte[] command = new byte[] { 0x00 };

            connections.Setup(a => a.EnviarComando(command)).Returns(new byte[] { 0x00 });

            ConnectorCem connectorCem = new ConnectorCem(connections.Object);

            Data data = new Data
            {
                IP = "10.773.856",
                Protocol = "16",
                Modo = MODO.NORMAL.ToString()
            };

            Configuration.SaveConfiguration(data);

            //Act
            bool actual = connectorCem.PoleoEnLinea(command);

            //Assert
            Assert.IsTrue(actual);
        }
    }
}
