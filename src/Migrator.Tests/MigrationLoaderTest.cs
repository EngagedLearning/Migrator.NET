using System;
using System.Collections.Generic;
using System.Reflection;
using Migrator.Framework;
using Migrator.Framework.Loggers;
using NUnit.Framework;
using NUnit.Mocks;

namespace Migrator.Tests
{
    [TestFixture]
    public class MigrationLoaderTest
    {

        private MigrationLoader _migrationLoader;

        [SetUp]
        public void SetUp()
        {
            SetUpCurrentVersion(0, false);
        }

        [Test]
        public void LastVersion()
        {
            Assert.AreEqual(7, _migrationLoader.LastVersion);
        }

        [Test]
        public void ZeroIfNoMigrations()
        {
            _migrationLoader.MigrationsTypes.Clear();
            Assert.AreEqual(0, _migrationLoader.LastVersion);
        }

        [Test]
        public void NullIfNoMigrationForVersion()
        {
            Assert.IsNull(_migrationLoader.GetMigration(99999999));
        }

        [Test]
        [ExpectedException(typeof(DuplicatedVersionException))]
        public void CheckForDuplicatedVersion()
        {
            _migrationLoader.MigrationsTypes.Add(typeof(MigratorTest.FirstMigration));
            _migrationLoader.CheckForDuplicatedVersion();
        }

        [Test]
        public void AddingASecondSetOfTypesSortsAppropriately()
        {
            DynamicMock providerMock = new DynamicMock(typeof(ITransformationProvider));

            providerMock.SetReturnValue("get_CurrentVersion", 0);
            providerMock.SetReturnValue("get_Logger", new Logger(false));
            providerMock.ExpectNoCall("Rollback");

            _migrationLoader = new MigrationLoader((ITransformationProvider)providerMock.MockInstance, new List<Assembly>(), true);
            var later = typeof(LaterMigration);
            var earlier = typeof(EarlierMigration);
            _migrationLoader.AddMigrationTypes(new List<Type> { later });
            _migrationLoader.AddMigrationTypes(new List<Type> { earlier });
            CollectionAssert.AreEqual(new[] { earlier, later }, _migrationLoader.MigrationsTypes);
        }

        [Migration(1)]
        private class EarlierMigration
        {
        }

        [Migration(2)]
        private class LaterMigration
        {
        }

        private void SetUpCurrentVersion(int version, bool assertRollbackIsCalled)
        {
            DynamicMock providerMock = new DynamicMock(typeof(ITransformationProvider));

            providerMock.SetReturnValue("get_CurrentVersion", version);
            providerMock.SetReturnValue("get_Logger", new Logger(false));
            if (assertRollbackIsCalled)
                providerMock.Expect("Rollback");
            else
                providerMock.ExpectNoCall("Rollback");

            _migrationLoader = new MigrationLoader((ITransformationProvider)providerMock.MockInstance, new List<Assembly> { Assembly.GetExecutingAssembly() }, true);
            _migrationLoader.MigrationsTypes.Add(typeof(MigratorTest.FirstMigration));
            _migrationLoader.MigrationsTypes.Add(typeof(MigratorTest.SecondMigration));
            _migrationLoader.MigrationsTypes.Add(typeof(MigratorTest.ThirdMigration));
            _migrationLoader.MigrationsTypes.Add(typeof(MigratorTest.ForthMigration));
            _migrationLoader.MigrationsTypes.Add(typeof(MigratorTest.BadMigration));
            _migrationLoader.MigrationsTypes.Add(typeof(MigratorTest.SixthMigration));
            _migrationLoader.MigrationsTypes.Add(typeof(MigratorTest.NonIgnoredMigration));
        }
    }
}