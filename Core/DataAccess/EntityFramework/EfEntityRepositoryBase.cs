﻿using Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Core.DataAccess.EntityFramework
{
    public class EfEntityRepositoryBase<TEntity, TContext> : IEntityRepository<TEntity>
        where TEntity : class, IEntity, new()
        where TContext : DbContext, new()
    {
        public void Add(TEntity entity)
        {
            using (TContext context = new TContext())//using içerisine yazdığımız nesneler using bitince anında gb gelip beni bellekten at diyor
            {
                var addedEntity = context.Entry(entity);//git veri kaynağından benim bu gönderdiğim producttan belirli bir eşleşme yap(referansı yakala)
                addedEntity.State = EntityState.Added;
                context.SaveChanges();//burdaki bütün işlemleri gerçekleştirir.
            }
        }

        public void Delete(TEntity entity)
        {
            using (TContext context = new TContext())//using içerisine yazdığımız nesneler using bitince anında gb gelip beni bellekten at diyor
            {
                var deleteEntity = context.Entry(entity);//git veri kaynağından benim bu gönderdiğim producttan belirli bir eşleşme yap(referansı yakala)
                deleteEntity.State = EntityState.Deleted;
                context.SaveChanges();//burdaki bütün işlemleri gerçekleştirir.
            }
        }

        public TEntity Get(Expression<Func<TEntity, bool>> filter)
        {
            using (TContext context = new TContext())
            {
                return context.Set<TEntity>().SingleOrDefault(filter);
            }
        }

        public List<TEntity> GetAll(Expression<Func<TEntity, bool>> filter = null)
        {
            using (TContext context = new TContext())
            {
                //veri tabanındaki bütün tabloyu listeye çevir onu bana ver- select * from products döndürüyor(sqlde)
                return filter == null ? context.Set<TEntity>().ToList()
                    : context.Set<TEntity>().Where(filter).ToList();

            }
        }

        public void Update(TEntity entity)
        {
            using (TContext context = new TContext())//using içerisine yazdığımız nesneler using bitince anında gb gelip beni bellekten at diyor
            {
                var updateEntity = context.Entry(entity);//git veri kaynağından benim bu gönderdiğim producttan belirli bir eşleşme yap(referansı yakala)
                updateEntity.State = EntityState.Modified;
                context.SaveChanges();//burdaki bütün işlemleri gerçekleştirir.
            }
        }
        public Expression<Func<TEntity, bool>> GetFilterExpression(IFilterDto filterDto)
        {
            Expression propertyExp, someValue, containsMethodExp, combinedExp;
            Expression<Func<TEntity, bool>> exp = c => true, oldExp;
            MethodInfo method;
            var parameterExp = Expression.Parameter(typeof(TEntity), "type");
            foreach (PropertyInfo propertyInfo in filterDto.GetType().GetProperties())
            {
                if (propertyInfo.GetValue(filterDto, null) != null)
                {
                    oldExp = exp;
                    propertyExp = Expression.Property(parameterExp, propertyInfo.Name);
                    method = typeof(object).GetMethod("Equals", new[] { typeof(object) });
                    someValue = Expression.Constant(filterDto.GetType().GetProperty(propertyInfo.Name).GetValue(filterDto, null), typeof(object));
                    containsMethodExp = Expression.Call(propertyExp, method, someValue);
                    exp = Expression.Lambda<Func<TEntity, bool>>(containsMethodExp, parameterExp);
                    combinedExp = Expression.AndAlso(exp.Body, oldExp.Body);
                    exp = Expression.Lambda<Func<TEntity, bool>>(combinedExp, exp.Parameters[0]);
                }
            }
            return exp;
        }
    }
}
