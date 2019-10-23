using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace R5T.Teutonia.Test
{
    public abstract class FileSystemCloningOperatorTestFixture
    {
        #region Test-Fixture

        public IStringlyTypedPathOperator StringlyTypedPathOperator { get; }


        public StringlyTypedPathOperatorCombineTestFixture(IStringlyTypedPathOperator stringlyTypedPathOperator)
        {
            this.StringlyTypedPathOperator = stringlyTypedPathOperator;
        }

        #endregion
        

        /// <summary>
        /// Tests cloning the example file-system state from one file-system to another.
        /// </summary>
        [TestMethod]
        public void TestCloneOperation()
        {

        }
    }
}
