using CDS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationTests
{
    [TestClass]
    public class PumpControllerTest
    {
        [TestMethod]
        public void StartProcess_True()
        {
            // Arrange
            PumpController pumpController = new PumpController();

            Data data = new Data
            {
                Controller = "CEM-44",
                Timer = "8",
                RutaProyNuevo = @"C:\Sistema\PROY_NUEVO"
            };

            _ = Configuration.SaveConfiguration(data);

            // Act
            bool actual = pumpController.StartProcess(data);

            // Assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void StartProcess_NullReferenceFalse()
        {
            // Arrange
            PumpController pumpController = new PumpController();

            Data data = new Data();

            // Act
            bool actual = pumpController.StartProcess(data);

            // Assert
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void StartProcess_NullArgumentFalse()
        {
            // Arrange
            PumpController pumpController = new PumpController();

            Data data = null;

            // Act
            bool actual = pumpController.StartProcess(data);

            // Assert
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void UpdateProcess_True()
        {
            // Arrange
            PumpController pumpController = new PumpController();

            Data data = new Data
            {
                Controller = "FUSION",
                Timer = "8",
                RutaProyNuevo = @"C:\Sistema\PROY_NUEVO"
            };

            _ = pumpController.StartProcess(data);

            data = new Data
            {
                Controller = "CEM-44",
                Timer = "8",
                RutaProyNuevo = @"C:\Sistema\PROY_NUEVO"
            };

            // Act
            bool actual = pumpController.UpdateProcess(data);

            // Assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void UpdateProcess_NullReferenceFalse()
        {
            // Arrange
            PumpController pumpController = new PumpController();

            Data data = new Data();

            // Act
            bool actual = pumpController.UpdateProcess(data);

            // Assert
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void UpdateProcess_NullArgumentFalse()
        {
            // Arrange
            PumpController pumpController = new PumpController();

            Data data = null;

            // Act
            bool actual = pumpController.UpdateProcess(data);

            // Assert
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void SetNewData_True()
        {
            // Arrange
            PumpController pumpController = new PumpController();

            Data data = new Data
            {
                Controller = "CEM-44",
                Timer = "4",
                RutaProyNuevo = @"C:\Sistema\PROY_NUEVO"
            };

            _ = pumpController.StartProcess(data);

            data = new Data
            {
                Controller = "CEM-44",
                Timer = "10",
                RutaProyNuevo = @"C:\Sistema\PROY_NUEVO"
            };

            // Act
            bool actual = pumpController.UpdateProcess(data);

            // Assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void SetNewData_NullReferenceFalse()
        {
            // Arrange
            PumpController pumpController = new PumpController();

            Data data = new Data();

            // Act
            bool actual = pumpController.SetNewData(data);

            // Assert
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void SetNewData_NullArgumentFalse()
        {
            // Arrange
            PumpController pumpController = new PumpController();

            Data data = null;

            // Act
            bool actual = pumpController.SetNewData(data);

            // Assert
            Assert.IsFalse(actual);
        }
    }
}
