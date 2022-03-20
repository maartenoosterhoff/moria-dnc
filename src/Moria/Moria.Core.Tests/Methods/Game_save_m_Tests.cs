using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using Moq;
using Moria.Core.Methods;
using Moria.Core.States;
using Moria.Core.Utils;
using NUnit.Framework;

namespace Moria.Core.Tests.Methods
{
    [TestFixture]
    public class Game_save_m_Tests
    {
        private Game_save_m gameSave;

        private Mock<IBinaryReaderWriterFactory> binaryReaderWriterFactoryMock;
        private Mock<IFileSystem> fileSystemMock;
        private Mock<IRnd> rndMock;

        private Mock<IFile> fileMock;

        private byte[] saveGameContents;

        [SetUp]
        public void SetUp()
        {
            this.binaryReaderWriterFactoryMock = new Mock<IBinaryReaderWriterFactory>();
            this.fileSystemMock = new Mock<IFileSystem>();
            this.rndMock = new Mock<IRnd>();
            this.gameSave = new Game_save_m(
                Mock.Of<IEventPublisher>(),
                this.fileSystemMock.Object,
                Mock.Of<IGame>(),
                this.rndMock.Object,
                Mock.Of<IStoreInventory>(),
                Mock.Of<ITerminal>(),
                this.binaryReaderWriterFactoryMock.Object
            );

            this.fileMock = new Mock<IFile>();
            this.fileSystemMock
                .Setup(x => x.File)
                .Returns(this.fileMock.Object);

            this.fileMock
                .Setup(x => x.Exists(It.IsAny<string>()))
                .Returns(true);
            this.fileMock
                .Setup(x => x.Open(It.IsAny<string>(), It.IsAny<FileMode>()))
                .Returns((Stream)null);

            State.Instance = new State();

            this.saveGameContents = new EmbeddedResourceReader().ReadResourceAsBytes(
                typeof(Game_save_m_Tests).Assembly,
                "Moria.Core.Tests.Methods.game.sav"
            );

            Player_m.SetDependencies(
                Mock.Of<IDice>(),
                Mock.Of<IDungeon>(),
                Mock.Of<IGame>(),
                Mock.Of<IHelpers>(),
                Mock.Of<IIdentification>(),
                Mock.Of<IInventoryManager>(),
                Mock.Of<IMonster>(),
                Mock.Of<IPlayerMagic>(),
                Mock.Of<IPlayerTraps>(),
                Mock.Of<IRnd>(),
                Mock.Of<ITerminal>(),
                Mock.Of<ITerminalEx>(),
                Mock.Of<IEventPublisher>()
            );
        }

        [Test]
        public void LoadAndSave_ResultsInIdenticalSaveFile()
        {
            // Arrange
            this.rndMock
                .Setup(x => x.randomNumber(256))
                .Returns(this.saveGameContents[3] + 1);
            this.binaryReaderWriterFactoryMock
                .Setup(x => x.CreateBinaryWriter(It.IsAny<Stream>()))
                .Returns((Stream stream) => new VerifyingBinaryWriter(this.saveGameContents));
            this.binaryReaderWriterFactoryMock
                .Setup(x => x.CreateBinaryReader(It.IsAny<Stream>()))
                .Returns(new BinaryReaderWrapper(new MemoryStream(this.saveGameContents)));

            // Load
            this.gameSave.loadGame(out _);

            // Save
            this.gameSave.saveGame();
        }
    }

    public class VerifyingBinaryWriter : IBinaryWriter
    {
        private readonly byte[] expected;

        private int position;

        public VerifyingBinaryWriter(byte[] expected)
        {
            this.expected = expected;
        }

        public void Write(byte value)
        {
            if (this.position > this.expected.Length)
            {
                Assert.Fail("Written beyond the expected size.");
            }

            if (value != this.expected[this.position])
            {
                var message = $"Incorrect byte value found when writing to stream. Expected: {this.expected[this.position]}, Actual: {value}";
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                else
                {
                    Assert.Fail(message);
                }
            }

            this.position++;
        }
    }
}
