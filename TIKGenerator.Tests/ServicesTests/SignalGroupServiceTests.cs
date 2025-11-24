using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TIKGenerator.Data;
using TIKGenerator.Models;
using TIKGenerator.Services;
using Microsoft.EntityFrameworkCore;

namespace TIKGenerator.Tests.ServicesTests
{
    public class SignalGroupServiceTests
    {
        private DbContextOptions<AppDbContext> CreateInMemoryOptions()
        {
            return new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        private Mock<ILogger<SignalGroupService>> CreateLoggerMock()
        {
            return new Mock<ILogger<SignalGroupService>>();
        }

        [Fact]
        public async Task SaveGroupAsync_ShouldReturnTrue_WhenGroupIsSaved()
        {
            var options = CreateInMemoryOptions();
            var loggerMock = CreateLoggerMock();

            var service = new SignalGroupService(options, loggerMock.Object);

            var group = new SignalGroup
            {
                Id = "g1",
                Name = "Test Group",
                CreatedAt = DateTime.Now,
                Signals = new List<Signal>()
            };

            var result = await service.SaveGroupAsync(group);

            Assert.True(result);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(group.Id)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAllGroupsAsync_ShouldReturnSavedGroups()
        {
            var options = CreateInMemoryOptions();
            var loggerMock = CreateLoggerMock();

            await using (var db = new AppDbContext(options))
            {
                db.SignalGroups.Add(new SignalGroup { Id = "g1", Name = "Group1", CreatedAt = DateTime.Now });
                db.SignalGroups.Add(new SignalGroup { Id = "g2", Name = "Group2", CreatedAt = DateTime.Now.AddMinutes(-10) });
                await db.SaveChangesAsync();
            }

            var service = new SignalGroupService(options, loggerMock.Object);

            var groups = await service.GetAllGroupsAsync();

            Assert.Equal(2, groups.Count);
            Assert.Equal("g1", groups[0].Id);
        }

        [Fact]
        public async Task UpdateGroupAsync_ShouldReturnFalse_WhenGroupNotFound()
        {
            var options = CreateInMemoryOptions();
            var loggerMock = CreateLoggerMock();
            var service = new SignalGroupService(options, loggerMock.Object);

            var group = new SignalGroup { Id = "nonexistent", Name = "Group", CreatedAt = DateTime.Now };

            var result = await service.UpdateGroupAsync(group);

            Assert.False(result);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(group.Id)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteGroupAsync_ShouldRemoveGroup()
        {
            var options = CreateInMemoryOptions();
            var loggerMock = CreateLoggerMock();

            await using (var db = new AppDbContext(options))
            {
                db.SignalGroups.Add(new SignalGroup { Id = "g1", Name = "Group1", CreatedAt = DateTime.Now });
                await db.SaveChangesAsync();
            }

            var service = new SignalGroupService(options, loggerMock.Object);

            await service.DeleteGroupAsync("g1");

            await using (var db = new AppDbContext(options))
            {
                var group = await db.SignalGroups.FirstOrDefaultAsync(g => g.Id == "g1");
                Assert.Null(group);
            }
        }
    }
}
