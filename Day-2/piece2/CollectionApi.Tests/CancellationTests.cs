using System.Net;
using CollectionApi.Models;
using CollectionApi.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CollectionApi.Tests;

public sealed class CancellationTests
{
    [Fact]
    public async Task Get_collections_cancels_the_repository_operation_when_request_is_aborted()
    {
        var repository = new BlockingCollectionRepository();
        await using var factory = new CollectionApiFactory(repository);
        using var client = factory.CreateClient();
        using var cancellation = new CancellationTokenSource();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/collections");
        var responseTask = client.SendAsync(request, cancellation.Token);

        await repository.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => responseTask);
        await repository.Cancelled.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.False(repository.Completed);
    }

    private sealed class CollectionApiFactory(BlockingCollectionRepository repository) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.ConfigureServices(services =>
        {
            services.RemoveAll<ICollectionRepository>();
            services.AddSingleton(repository);
            services.AddScoped<ICollectionRepository>(provider => provider.GetRequiredService<BlockingCollectionRepository>());
        });
    }

    private sealed class BlockingCollectionRepository : ICollectionRepository
    {
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Cancelled { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public bool Completed { get; private set; }

        public async Task<IReadOnlyList<CollectionItem>> GetAllAsync(CancellationToken cancellationToken)
        {
            Started.TrySetResult();
            try { await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken); }
            catch (OperationCanceledException) { Cancelled.TrySetResult(); throw; }
            Completed = true;
            return [];
        }

        public Task<CollectionItem?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
            Task.FromResult<CollectionItem?>(null);

        public Task<CollectionItem> AddAsync(CollectionItem item, CancellationToken cancellationToken) =>
            Task.FromResult(item);
    }
}
