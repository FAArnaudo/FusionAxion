using CDS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Data;
using System.IO;

namespace ConfigurationTests
{
    [TestClass]
    public class SQLiteTest
    {
        // El nombre del archivo de base de datos que se usará en las pruebas

        private readonly string testPath = @"C:\Sistema\PROY_NUEVO";
        private readonly string testFolderPath = @"C:\Sistema\PROY_NUEVO" + @"\CDS";
        private readonly string testDatabaseName = "cds.db";
        private Data data;
        private Mock<IGetConfiguration> configuration;
        private ConnectorSQLite connector;


        [TestInitialize]
        public void TestInitialize()
        {
            configuration = new Mock<IGetConfiguration>();

            connector = ConnectorSQLite.Instance;

            data = new Data
            {
                RutaProyNuevo = testPath
            };

            _ = configuration.Setup(a => a.GetConfiguration()).Returns(data);

            Configuration.SaveConfiguration(data);

            string databasePath = Path.Combine(testFolderPath, testDatabaseName);

            // Elimina la base de datos si existe
            //if (File.Exists(databasePath))
            //{
            //    File.Delete(databasePath);
            //}
            // Ensure the database file is not in use before attempting to delete it
            if (File.Exists(databasePath))
            {
                if (!IsFileLocked(databasePath))
                {
                    File.Delete(databasePath);
                }
                else
                {
                    // Log a warning or handle the locked file appropriately
                    throw new IOException($"The database file '{databasePath}' is in use by another process.");
                }
            }
        }

        // Helper method to check if a file is locked
        private bool IsFileLocked(string filePath)
        {
            try
            {
                using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                return true;
            }
            return false;
        }

        [TestMethod]
        public void CreateDatabase_ReturnTrue_WhenItDoesNotExist()
        {
            // Arrange
            ConnectorSQLite connector = ConnectorSQLite.Instance;

            bool expected = true;

            // Act
            bool actual = connector.CreateDatabase(configuration.Object);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CreateDatabase_ReturnTrue_WhenItAlreadyExist()
        {
            // Arrange
            ConnectorSQLite connector = ConnectorSQLite.Instance;

            // Crear el directorio
            _ = Directory.CreateDirectory(testPath);

            _ = connector.CreateDatabase(configuration.Object);

            // Act
            bool actual = connector.CreateDatabase(configuration.Object);

            // Assert
            Assert.IsTrue(actual);
        }


        [TestMethod]
        public void ExecuteInsertOrStateQuery_InsertOneRowModify()
        {
            // Arrange
            ConnectorSQLite connector = ConnectorSQLite.Instance;

            _ = connector.CreateDatabase(configuration.Object);

            // Crear las tablas si no existen
            string createTableQuery = "CREATE TABLE IF NOT EXISTS Usuarios " +
                                      "(Nombre TEXT, Edad  INTEGER)";

            _ = connector.ExecuteNonQuery(createTableQuery);

            int expected = 1;

            // Act
            int actual = connector.ExecuteNonQuery($"INSERT INTO Usuarios (Nombre, Edad) VALUES ('Carlos', 25)");

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ExecuteInsertOrStateQuery_DeleteNoModify()
        {
            // Arrange
            ConnectorSQLite connector = ConnectorSQLite.Instance;

            _ = connector.CreateDatabase(configuration.Object);

            // Crear las tablas si no existen
            string createTableQuery = "CREATE TABLE IF NOT EXISTS Usuarios " +
                                      "(id_usuario INTEGER, nombre TEXT, edad  INTEGER, " +
                                      "PRIMARY KEY(ID_Usuario))";

            _ = connector.ExecuteNonQuery(createTableQuery);

            int expected = 0;

            // Act
            int actual = connector.ExecuteNonQuery($"DELETE FROM Usuarios WHERE id_usuario = (1)");

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ExecuteStateQuery_False()
        {
            // Arrange
            ConnectorSQLite connector = ConnectorSQLite.Instance;

            _ = connector.CreateDatabase(configuration.Object);

            // Crear la tabla si no existe
            string createTableQuery = "CREATE TABLE IF NOT EXISTS CheckConnection (" +
                                      "idConnection INTEGER PRIMARY KEY, " +
                                      "isConnected INTEGER, " +
                                      "fecha DATE DEFAULT(datetime('now', 'localtime')));" +
                                      "\nINSERT OR IGNORE INTO CheckConnection (idConnection, isConnected) " +
                                      "\nVALUES (1, 0);";

            _ = connector.ExecuteNonQuery(createTableQuery);

            // Act
            bool actual = connector.ExecuteStateQuery("SELECT isConnected FROM CheckConnection WHERE idConnection = 1;");

            // Assert
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void ExecuteStateQuery_True()
        {
            // Arrange
            _ = connector.CreateDatabase(configuration.Object);

            // Crear la tabla si no existe
            string createTableQuery = "UPDATE CheckConnection " +
                                      "SET isConnected = 1 " +
                                      "WHERE idConnection = 1";

            _ = connector.ExecuteNonQuery(createTableQuery);

            // Act
            bool actual = connector.ExecuteStateQuery("SELECT isConnected FROM CheckConnection WHERE idConnection = 1");

            // Assert
            Assert.IsTrue(actual);
        }
    }
}
