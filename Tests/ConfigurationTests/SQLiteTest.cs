using CDS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

        [TestInitialize]
        public void TestInitialize()
        {
            Data data = new Data
            {
                RutaProyNuevo = testPath
            };

            _ = Configuration.SaveConfiguration(data);

            string databasePath = Path.Combine(testFolderPath, testDatabaseName);

            // Elimina la base de datos si existe
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }

        

        [TestMethod]
        public void CreateDatabase_ReturnTrue_WhenItDoesNotExist()
        {
            // Arrange
            ConnectorSQLite connector = ConnectorSQLite.Instance;

            bool expected = true;

            // Act
            bool actual = connector.CreateDatabase();

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

            _ = connector.CreateDatabase();

            // Act
            bool actual = connector.CreateDatabase();

            // Assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void ExecuteNonQuery_CreateTable()
        {
            // Arrange
            ConnectorSQLite connector = ConnectorSQLite.Instance;

            _ = connector.CreateDatabase();

            int expected = 0;

            // Act
            string createTableQuery = "CREATE TABLE IF NOT EXISTS Usuarios " +
                                      "(Nombre TEXT, Edad  INTEGER)";

            int actual = connector.ExecuteNonQuery(createTableQuery);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ExecuteInsertOrStateQuery_InsertOneRowModify()
        {
            // Arrange
            ConnectorSQLite connector = ConnectorSQLite.Instance;

            _ = connector.CreateDatabase();

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

            _ = connector.CreateDatabase();

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
        public void ExecuteInsertOrStateQuery_UpdateException()
        {
            // Arrange
            ConnectorSQLite connector = ConnectorSQLite.Instance;

            _ = connector.CreateDatabase();

            // Crear las tablas si no existen
            string createTableQuery = "CREATE TABLE IF NOT EXISTS Usuarios " +
                                      "(id_usuario INTEGER, nombre TEXT, edad  INTEGER, " +
                                      "PRIMARY KEY(ID_Usuario))";

            _ = connector.ExecuteNonQuery(createTableQuery);

            int expected = -1;

            // Act
            int actual = connector.ExecuteNonQuery($"UPDATE Usuarios SET name = 'Charls', age = 29");

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ExecuteSelectQuery_NotNull()
        {
            // Arrange
            ConnectorSQLite connector = ConnectorSQLite.Instance;

            _ = connector.CreateDatabase();

            // Crear las tablas si no existen
            string createTableQuery = "CREATE TABLE IF NOT EXISTS Usuarios " +
                                      "(id_usuario INTEGER, nombre TEXT, edad  INTEGER, " +
                                      "PRIMARY KEY(ID_Usuario));" +
                                      "INSERT INTO Usuarios (Nombre, Edad) VALUES ('Carlos', 25)";

            _ = connector.ExecuteNonQuery(createTableQuery);

            int expected = 0;

            // Act
            DataTable actual = connector.ExecuteSelectQuery("SELECT * FROM Usuarios WHERE id_usuario = (3)");
            int count = actual.Rows.Count;

            // Assert
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected, count);
        }

        [TestMethod]
        public void ExecuteSelectQuery_NotNullException()
        {
            // Arrange
            ConnectorSQLite connector = ConnectorSQLite.Instance;

            _ = connector.CreateDatabase();

            // Crear las tablas si no existen
            string createTableQuery = "CREATE TABLE IF NOT EXISTS Usuarios " +
                                      "(id_usuario INTEGER, nombre TEXT, edad  INTEGER, " +
                                      "PRIMARY KEY(ID_Usuario))";

            _ = connector.ExecuteNonQuery(createTableQuery);

            _ = connector.ExecuteNonQuery($"INSERT INTO Usuarios (Nombre, Edad) VALUES ('Carlos', 25)");

            // Act
            DataTable actual = connector.ExecuteSelectQuery("SELECT * FROM Users");

            // Assert
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void ExecuteStateQuery_False()
        {
            // Arrange
            ConnectorSQLite connector = ConnectorSQLite.Instance;

            _ = connector.CreateDatabase();

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
            ConnectorSQLite connector = ConnectorSQLite.Instance;

            _ = connector.CreateDatabase();

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
