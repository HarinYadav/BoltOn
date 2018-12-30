using System;
using BoltOn.Data;
using BoltOn.Data.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using BoltOn.Bootstrapping;

namespace BoltOn.Tests.Data
{
	public class EFRepositoryTests : IDisposable
	{
		[Fact]
		public void GetById_WhenRecordExists_ReturnsRecord()
		{
			// arrange
			var services = new ServiceCollection();
			services.AddDbContext<TestDbContext>(options => options.UseInMemoryDatabase("InMemoryDbForTesting"));
			services.BoltOn();
			var serviceProvider = services.BuildServiceProvider();
			var testRepository = serviceProvider.GetService<ITestRepository>();
			var testDbContext = serviceProvider.GetService<TestDbContext>();
			testDbContext.Set<TestEntity>().Add(new TestEntity
			{
				Id = 1,
				FirstName = "a",
				LastName = "b"
			});
			serviceProvider.BoltOn();

			// act
			var result = testRepository.GetById<TestEntity>(1);

		}

		public void Dispose()
		{
		}
	}

	public interface ITestRepository : IRepository
	{

	}

	public class TestRepository : BaseEFRepository<TestDbContext>, ITestRepository
	{
		public TestRepository(IDbContextFactory dbContextFactory) : base(dbContextFactory)
		{
		}
	}

	public class TestDbContext : BaseDbContext<TestDbContext>
	{
		public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
		{

		}
	}

	public class TestEntity : BaseEntity<int>
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
	}

	public class TestEntityMapping : IEntityTypeConfiguration<TestEntity>
	{
		public void Configure(EntityTypeBuilder<TestEntity> builder)
		{
			builder
				.ToTable("TestEntity")
				.HasKey(k => k.Id);
			builder
				.Property(p => p.Id)
				.HasColumnName("TestEntityId");
		}
	}
}
