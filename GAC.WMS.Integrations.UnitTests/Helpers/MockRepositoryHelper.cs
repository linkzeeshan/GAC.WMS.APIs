using GAC.WMS.Integrations.Domain.Entities;
using GAC.WMS.Integrations.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace GAC.WMS.Integrations.UnitTests.Helpers
{
    public static class MockRepositoryHelper
    {
        /// <summary>
        /// Sets up a mock repository with the provided entities
        /// </summary>
        public static Mock<IRepository<T, TKey>> SetupMockRepository<T, TKey>(List<T> entities) where T : class, IEntity<TKey> where TKey : IEquatable<TKey>
        {
            var mockRepository = new Mock<IRepository<T, TKey>>();
            var queryable = entities.AsQueryable();

            mockRepository
                .Setup(repo => repo.GetAll())
                .Returns(queryable);

            mockRepository
                .Setup(repo => repo.GetByCondition(It.IsAny<Expression<Func<T, bool>>>()))
                .Returns((Expression<Func<T, bool>> expression) => 
                    entities.AsQueryable().Where(expression));

            mockRepository
                .Setup(repo => repo.CreateAsync(It.IsAny<T>()))
                .Callback<T>(entity => entities.Add(entity))
                .Returns(Task.CompletedTask);

            mockRepository
                .Setup(repo => repo.Update(It.IsAny<T>()))
                .Callback<T>(entity => 
                {
                    var existingEntity = entities.FirstOrDefault(e => e.Id.Equals(entity.Id));
                    if (existingEntity != null)
                    {
                        var index = entities.IndexOf(existingEntity);
                        entities[index] = entity;
                    }
                });

            mockRepository
                .Setup(repo => repo.Delete(It.IsAny<T>()))
                .Callback<T>(entity => entities.Remove(entity));

            return mockRepository;
        }

        /// <summary>
        /// Sets up a mock unit of work with repositories for the specified entity types
        /// </summary>
        public static Mock<IUnitOfWork> SetupMockUnitOfWork<T, TKey>(Mock<IRepository<T, TKey>> mockRepository) 
            where T : class, IEntity<TKey>
            where TKey : IEquatable<TKey>
        {
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            
            mockUnitOfWork
                .Setup(uow => uow.GetRepository<T, TKey>())
                .Returns(mockRepository.Object);

            mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync())
                .ReturnsAsync(1);

            mockUnitOfWork
                .Setup(uow => uow.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            mockUnitOfWork
                .Setup(uow => uow.CommitTransactionAsync())
                .Returns(Task.CompletedTask);

            mockUnitOfWork
                .Setup(uow => uow.RollbackTransactionAsync())
                .Returns(Task.CompletedTask);

            return mockUnitOfWork;
        }
    }
}
