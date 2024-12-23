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
        public void UpdateControllerType_ReturnTrue()
        {
            // Arrange
            PumpController controller = PumpController.Instance;

            Data data = new Data
            {
                Controller = "CEM-44",
                Timer = "8",
                RutaProyNuevo = @"C:\Sistema\PROY_NUEVO"
            };

            // Act
            bool actual = controller.CheckNewController(data.Controller);

            // Assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void UpdateControllerType_SameController_ReturnTrue()
        {
            // Arrange
            PumpController controller = PumpController.Instance;

            Data data = new Data
            {
                Controller = "CEM-44",
                Timer = "8",
                RutaProyNuevo = @"C:\Sistema\PROY_NUEVO"
            };

            _ = controller.CheckNewController(data.Controller);

            // Act
            bool actual = controller.CheckNewController(data.Controller);

            // Assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void UpdateControllerType_NoController_ReturnFalse()
        {
            // Arrange
            PumpController controller = PumpController.Instance;

            Data data = new Data
            {
                Controller = "CEM-44",
                Timer = "8",
                RutaProyNuevo = @"C:\Sistema\PROY_NUEVO"
            };

            _ = controller.CheckNewController(data.Controller);

            // Act
            bool actual = controller.CheckNewController("MIDEX");

            // Assert
            Assert.IsFalse(actual);
        }
    }
}
