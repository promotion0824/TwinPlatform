using EFCore.BulkExtensions;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Willow.Rules.DTO;
using Willow.Rules.Model;
using Willow.Rules.Repository;

namespace WillowRules.Test.Bugs.Mocks;

public abstract class RepositoryBaseMock<T> : IRepositoryBase<T> where T : IId
{
	public double CacheHitRatio => throw new NotImplementedException();
	private static object locker = new object();
	public List<T> Data { get; init; } = new List<T>();
	public BulkConfig? LastMergeConfig = null;

	public Task BulkDelete(System.Collections.Generic.IList<T> items, bool updateCache = true, CancellationToken cancellationToken = default)
	{
		Data.RemoveAll(v => items.Contains(v));

		return Task.CompletedTask;
	}

	public Task BulkMerge(System.Collections.Generic.IList<T> items, BulkConfig? config = null, bool updateOnly = false, CancellationToken cancellationToken = default)
	{
		LastMergeConfig = config;
		Data.AddRange(items);

		return Task.CompletedTask;
	}

	public Task<int> Count(Expression<Func<T, bool>> queryExpression)
	{
		return Task.FromResult<int>(Data.Count(queryExpression.Compile()));
	}

	public Task<bool> Any(Expression<Func<T, bool>> queryExpression)
	{
		return Task.FromResult<bool>(Data.Any(queryExpression.Compile()));
	}

	public Task DeleteOne(T item, bool updateCache = true)
	{
		Data.Remove(item);
		return Task.CompletedTask;
	}

	public Task FlushQueue(int batchSize = 4000, bool updateCache = true, BulkConfig? config = null, bool updateOnly = false)
	{
		return Task.CompletedTask;
	}

	public Task<System.Collections.Generic.IEnumerable<T>> Get(Expression<Func<T, bool>>? queryExpression)
	{
		if (queryExpression is not null)
		{
			return Task.FromResult((IEnumerable<T>)Data.Where(v => queryExpression.Compile()(v)).ToList());
		}

		return Task.FromResult((IEnumerable<T>)Data.ToList());
	}

	public Task<Batch<T>> GetAll(SortSpecificationDto[] sortSpecifications,
		FilterSpecificationDto[] filterSpecifications,
		Expression<Func<T, bool>>? whereExpression,
		int? page = null, int? take = null)
	{
		return Task.FromResult(new Batch<T>("", 0, 0, 0, whereExpression is null ? Data : Data.Where(whereExpression.Compile()), ""));
	}

	public System.Collections.Generic.IAsyncEnumerable<T> GetAll(Expression<Func<T, bool>>? whereExpression = null)
	{
		if(whereExpression is not null)
		{
			return Data.Where(v => whereExpression.Compile()(v)).ToList().ToAsyncEnumerable();
		}

		return Data.ToList().ToAsyncEnumerable();
	}

	public Task<T?> GetOne(string id, bool updateCache = true)
	{
		return Task.FromResult(Data.FirstOrDefault(v => v.Id == id));
	}

	public Task QueueWrite(T item, int queueSize = 4000, int batchSize = 4000, bool updateCache = true, BulkConfig? config = null, bool updateOnly = false)
	{
		//QueueWrite must be thread safe
		lock (locker)
		{
			item.Id.Should().NotBeNullOrEmpty();
			Data.RemoveAll(v => v.Id == item.Id);
			Data.Add(item);
		}
		return Task.CompletedTask;
	}

	public Task<T> UpsertOne(T value, bool updateCache = true, CancellationToken cancellationToken = default)
	{
		Data.RemoveAll(v => v.Id == value.Id);
		Data.Add(value);
		return Task.FromResult(value);
	}

	public Task UpsertOneUnique(T value, CancellationToken cancellationToken = default)
	{
		Data.RemoveAll(v => v.Id == value.Id);
		Data.Add(value);
		return Task.FromResult(value);
	}

	public Task<IEnumerable<T>> GetAscending<U>(Expression<Func<T, bool>> queryExpression, Expression<Func<T, U>> keySelector, int? limit = null)
	{
		return Task.FromResult(Data.Where(v => queryExpression.Compile()(v)));
	}

	public Task<IEnumerable<T>> GetDescending<U>(Expression<Func<T, bool>> queryExpression, Expression<Func<T, U>> keySelector, int? limit = null)
	{
		return Task.FromResult(Data.Where(v => queryExpression.Compile()(v)));
	}

	public IQueryable<T> GetQueryable()
	{
		return Data.AsQueryable();
	}
}
