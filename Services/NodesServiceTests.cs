
namespace NodeManagementApp.UnitTests.Services{
    // The wrapper interface for MongoDB collections
    public interface IMongoCollectionWrapper<T>
    {
        Task InsertOneAsync(T document);
        Task<T> FindOneAsync(Expression<Func<T, bool>> filter);
        Task ReplaceOneAsync(Expression<Func<T, bool>> filter, T replacement);
        Task DeleteOneAsync(Expression<Func<T, bool>> filter);
        // Add other methods as needed later
    }

    // Mock Node class for testing
    public class Node
    {
        public string Id { get; set; }

        public Node()
        {
            Id = Guid.NewGuid().ToString();
        }
        // Add other properties as needed later
    }

    // Mocking NodeService with MongoDB connection
    public class NodesService
    {
        private readonly IMongoCollectionWrapper<Node> _nodes;

        public NodesService(IMongoCollectionWrapper<Node> nodes)
        {
            _nodes = nodes;
        }

        public async Task CreateAsync(Node node)
        {
            await _nodes.InsertOneAsync(node);
        }

        public async Task<Node> ReadAsync(string id)
        {
            return await _nodes.FindOneAsync(n => n.Id == id);
        }

        public async Task UpdateAsync(string id, Node node)
        {
            await _nodes.ReplaceOneAsync(n => n.Id == id, node);
        }

        public async Task DeleteAsync(string id)
        {
            await _nodes.DeleteOneAsync(n => n.Id == id);
        }

    }

    public class NodesServiceTests
    {
        private readonly Mock<IMongoCollectionWrapper<Node>> _mockCollection;
        private readonly NodesService _service;
        private readonly Node _node;

        public NodesServiceTests()
        {
            _mockCollection = new Mock<IMongoCollectionWrapper<Node>>();
            _service = new NodesService(_mockCollection.Object);
            _node = new Node();
        }

        [Fact]
        public async Task CreateAsync_CreatesNode()
        {
            _mockCollection.Setup(c => c.InsertOneAsync(_node))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await _service.CreateAsync(_node);

            _mockCollection.Verify();
        }

        [Fact]
        public async Task ReadAsync_ReturnsNode()
        {
            // Arrange
            var mockCollection = new Mock<IMongoCollectionWrapper<Node>>();
            var node = new Node();

            mockCollection.Setup(c => c.FindOneAsync(It.IsAny<Expression<Func<Node, bool>>>()))
                .ReturnsAsync(node)
                .Verifiable();

            var service = new NodesService(mockCollection.Object);

            // Act
            var result = await service.ReadAsync(node.Id);

            // Assert
            mockCollection.Verify();
            Assert.Equal(node, result);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesNode()
        {
            // Arrange
            var mockCollection = new Mock<IMongoCollectionWrapper<Node>>();
            var node = new Node();

            mockCollection.Setup(c => c.ReplaceOneAsync(It.IsAny<Expression<Func<Node, bool>>>(), node))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var service = new NodesService(mockCollection.Object);

            // Act
            await service.UpdateAsync(node.Id, node);

            // Assert
            mockCollection.Verify();
        }

        [Fact]
        public async Task DeleteAsync_DeletesNode()
        {
            _mockCollection.Setup(c => c.DeleteOneAsync(It.IsAny<Expression<Func<Node, bool>>>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await _service.DeleteAsync(_node.Id);

            _mockCollection.Verify();
        }
    }
}
