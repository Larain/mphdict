﻿// Copyright © 2016 uSofTrod. Contacts: <uSofTrod@outlook.com>
// License: http://opensource.org/licenses/MIT
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using mphdict.Models.Etym.Mapping;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using uSofTrod.generalTypes.Models;

namespace mphdict.Models.Etym
{
    public class etymContext: DbContext
    {
        private string _schema = string.Empty; // "dbo" 

        public DbSet<bibl> bibls { get; set; }
        public DbSet<e_classes> e_classes { get; set; }
        public DbSet<etymons> etymons { get; set; }
        public DbSet<lang_all> lang_all { get; set; }
        public DbSet<links> links { get; set; }
        public DbSet<root> roots { get; set; }

        public etymContext(DbContextOptions<etymContext> options) 
            : base(options)
        {
            //Configuration.LazyLoadingEnabled = false;
            //Configuration.ProxyCreationEnabled = false;
            //Configuration.AutoDetectChangesEnabled = false;
        }
        public etymContext(DbContextOptions<etymContext> options, string schema)
            : base(options)
        {
            _schema = schema;
        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    //optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=MyDatabase;Trusted_Connection=True;");
        //}
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.RemovePluralizingTableNameConvention();
            modelBuilder.biblMap();
            modelBuilder.e_classesMap();
            modelBuilder.etymonsMap();
            modelBuilder.lang_allMap();
            modelBuilder.linksMap();
            modelBuilder.rootMap();

            if (!string.IsNullOrEmpty(_schema))
            {
                modelBuilder.HasDefaultSchema(_schema);
            }


            modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.Snapshot);


            //modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();//не змінювати імена під час сворення БД
            ////повне відключення угоди по замовчанню (якщо зовнішній ключ не може містити null, для таких колонок Code First вказує флаг каскадного видалення записів):
            //modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            //modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
            //modelBuilder.Conventions.Remove<StoreGeneratedIdentityKeyConvention>();//не генерувати нові ключові поля (з тих, які закінчуються на ID)
        }

        #region for working with multiple schemas
        private static ConcurrentDictionary<string, IModel> modelCache = new ConcurrentDictionary<string, IModel>();
        public static etymContext Create(DbContextOptionsBuilder<etymContext> optionsBuilder, string schema)
        {

            IModel compiledModel;
            if (schema == null || schema.Trim().Length == 0) schema = "/";
            compiledModel = modelCache.GetOrAdd(schema, (t) => GetCompiled(optionsBuilder, t));

            optionsBuilder.UseModel(compiledModel);

            return new etymContext(optionsBuilder.Options, schema);
        }
        private static IModel GetCompiled(DbContextOptionsBuilder<etymContext> optionsBuilder, string schema)
        {
            //https://github.com/aspnet/EntityFramework/issues/3909
            //var serviceCollection = new ServiceCollection();
            //serviceCollection.AddEntityFramework().AddSqlServer();
            //var serviceProvider = serviceCollection.BuildServiceProvider();
            var coreConventionSetBuilder = new CoreConventionSetBuilder(null);
            //var sqlConventionSetBuilder = new SqlServerConventionSetBuilder(new SqlServerTypeMapper());
            //var conventionSet = sqlConventionSetBuilder.AddConventions(coreConventionSetBuilder.CreateConventionSet());

            var conventionSet = coreConventionSetBuilder.CreateConventionSet();
            var builder = new ModelBuilder(conventionSet);
            builder.Entity<bibl>();
            builder.Entity<e_classes>();
            builder.Entity<etymons>();
            builder.Entity<lang_all>();
            builder.Entity<links>();
            builder.Entity<root>();
            //Cal Manually OnModelCreating from base class to create model objects relatet di Asp.Net identity (not nice)
            (new etymContext(optionsBuilder.Options, schema)).OnModelCreating(builder);
            //builder.Entity<Invoice>().ToTable("REGION", schema);

            return builder.Model;
        }
        #endregion
        public IEnumerable<T> Local<T>(DbSet<T> set)
          where T : class
        {
            return this.ChangeTracker.Entries<T>()
              .Where(e => e.State == EntityState.Added || e.State == EntityState.Unchanged || e.State == EntityState.Modified)
              .Select(e => e.Entity);
        }

    }
}
