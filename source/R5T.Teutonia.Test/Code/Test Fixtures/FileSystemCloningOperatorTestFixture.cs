using System;
using System.IO;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using R5T.Gepidia;
using R5T.Lombardy;
using R5T.Salamis;


namespace R5T.Teutonia.Test
{
    /// <summary>
    /// Tests an <see cref="IFileSystemCloningOperator"/> instance under "normal" conditions.
    /// Note to implementers: call the text-fixture <see cref="FileSystemCloningOperatorTestFixture.TestFixtureClassInitialize"/> and <see cref="FileSystemCloningOperatorTestFixture.TestFixtureClassCleanup"/>.
    /// </summary>
    /// <remarks>
    /// Relies on working <see cref="IFileSystemOperator"/> source and destination instances to setup an example source directory structure, and then test the destination directory structure.
    /// </remarks>
    public abstract class FileSystemCloningOperatorTestFixture
    {
        #region Test-Fixture

        public static void TestFixtureClassInitialize(FileSystemSite source, FileSystemSite destination)
        {
            // Do nothing.
        }

        public static void TestFixtureClassCleanup(FileSystemSite source, FileSystemSite destination)
        {
            // Do nothing.
        }


        private FileSystemSite Source { get; }
        private FileSystemSite Destination { get; }
        private IFileSystemCloningOperator FileSystemCloningOperator { get; }
        private FileSystemCloningOptions Options { get; }
        private IStringlyTypedPathOperator StringlyTypedPathOperator { get; }
        private TextWriter Writer { get; }



        public FileSystemCloningOperatorTestFixture(FileSystemSite source, FileSystemSite destination,
            IFileSystemCloningOperator fileSystemCloningOperator, FileSystemCloningOptions options,
            IStringlyTypedPathOperator stringlyTypedPathOperator, TextWriter writer)
        {
            this.Source = source;
            this.Destination = destination;
            this.FileSystemCloningOperator = fileSystemCloningOperator;
            this.Options = options;
            this.StringlyTypedPathOperator = stringlyTypedPathOperator;
            this.Writer = writer;
        }

        public FileSystemCloningOperatorTestFixture(FileSystemSite source, FileSystemSite destination,
            IFileSystemCloningOperator fileSystemCloningOperator,
            IStringlyTypedPathOperator stringlyTypedPathOperator)
            : this(source, destination, fileSystemCloningOperator, FileSystemCloningOptions.Default, stringlyTypedPathOperator, Console.Out)
        {
        }

        /// <summary>
        /// Ensure the source and destination site direcctories do not exist.
        /// </summary>
        [TestInitialize]
        public void TestFixtureTestInitialize()
        {
            // Delete both source and destination directories.
            this.Source.FileSystemOperator.DeleteDirectoryOnlyIfExists(this.Source.DirectoryPath);
            this.Destination.FileSystemOperator.DeleteDirectoryOnlyIfExists(this.Destination.DirectoryPath);
        }

        /// <summary>
        /// Delete both the source and destination site directories.
        /// </summary>
        [TestCleanup]
        public void TestFixtureTestCleanup()
        {
            // Delete both source and destination directories.
            this.Source.FileSystemOperator.DeleteDirectoryOnlyIfExists(this.Source.DirectoryPath);
            this.Destination.FileSystemOperator.DeleteDirectoryOnlyIfExists(this.Destination.DirectoryPath);
        }

        #endregion

        #region Static

        /// <summary>
        /// Create a directory structure for use in testing.
        /// </summary>
        /// <param name="site"></param>
        public static void CreateExampleDirectoryStructure(FileSystemSite site, IStringlyTypedPathOperator stringlyTypedPathOperator)
        {
            var fileSystemOperator = site.FileSystemOperator;

            var baseDirectoryPath = site.DirectoryPath;

            // Create directories.
            var directory01Path = stringlyTypedPathOperator.GetDirectoryPath(baseDirectoryPath, ExampleDirectoryNames.Directory01);
            var directory02Path = stringlyTypedPathOperator.GetDirectoryPath(baseDirectoryPath, ExampleDirectoryNames.Directory02);
            var directory03Path = stringlyTypedPathOperator.GetDirectoryPath(directory02Path, ExampleDirectoryNames.Directory03);
            var directory04Path = stringlyTypedPathOperator.GetDirectoryPath(directory03Path, ExampleDirectoryNames.Directory04);

            foreach (var directoryPath in new string[] { directory01Path, directory02Path, directory03Path, directory04Path })
            {
                fileSystemOperator.CreateDirectoryOnlyIfNotExists(directoryPath);
            }

            // Create files.
            var file01Path = stringlyTypedPathOperator.GetFilePath(baseDirectoryPath, ExampleFileNames.File01Name);
            var file02Path = stringlyTypedPathOperator.GetFilePath(directory01Path, ExampleFileNames.File02Name);
            var file03Path = stringlyTypedPathOperator.GetFilePath(directory02Path, ExampleFileNames.File03Name);
            var file04Path = stringlyTypedPathOperator.GetFilePath(directory02Path, ExampleFileNames.File04Name);
            var file05Path = stringlyTypedPathOperator.GetFilePath(directory03Path, ExampleFileNames.File05Name);
            var file06Path = stringlyTypedPathOperator.GetFilePath(directory04Path, ExampleFileNames.File06Name);

            foreach (var filePath in new string[] { file01Path, file02Path, file03Path, file04Path, file05Path, file06Path })
            {
                using (var writer = fileSystemOperator.CreateFileText(filePath))
                {
                    writer.WriteLine("CONTENT!");
                }
            }
        }

        public static bool VerifyClonedFileSystemStructure(FileSystemSite source, FileSystemSite destination, IStringlyTypedPathOperator stringlyTypedPathOperator, TextWriter writer)
        {
            var sourcePaths = source.FileSystemOperator.EnumerateFileSystemEntryPaths(source.DirectoryPath, true);
            var sourceRelativePaths = sourcePaths.Select(path => stringlyTypedPathOperator.GetRelativePath(source.DirectoryPath, path));
            var expectedDestinationPaths = sourceRelativePaths.Select(path => stringlyTypedPathOperator.Combine(destination.DirectoryPath, path));

            var actualDestinationPaths = destination.FileSystemOperator.EnumerateFileSystemEntryPaths(destination.DirectoryPath, true);

            var output = true;

            var missingDestinationPaths = expectedDestinationPaths.Except(actualDestinationPaths);
            if(missingDestinationPaths.Count() > 0)
            {
                output = false;

                writer.WriteLine($"Paths missing (count: {missingDestinationPaths.Count()}):");
                foreach (var missingDestinationPath in missingDestinationPaths)
                {
                    writer.WriteLine(missingDestinationPath);
                }
            }

            return output;
        }

        #endregion


        /// <summary>
        /// Tests cloning the example file-system state from one file-system to another.
        /// </summary>
        [TestMethod]
        public void TestCloneOperation()
        {
            // Create the example directory structure in the source.
            FileSystemCloningOperatorTestFixture.CreateExampleDirectoryStructure(this.Source, this.StringlyTypedPathOperator);

            // Perform file-system cloning action.
            this.FileSystemCloningOperator.Clone(this.Source, this.Destination, this.Options);

            // Verify.
            var success = FileSystemCloningOperatorTestFixture.VerifyClonedFileSystemStructure(this.Source, this.Destination, this.StringlyTypedPathOperator, this.Writer);

            Assert.IsTrue(success, "Failed to verify cloned file system.");
        }

        ///// <summary>
        ///// Tests what happens if the source site directory path does not exist.
        ///// </summary>
        //[TestMethod]
        //public void TestSourceSiteDirectoryDoesNotExist()
        //{

        //}
    }
}
