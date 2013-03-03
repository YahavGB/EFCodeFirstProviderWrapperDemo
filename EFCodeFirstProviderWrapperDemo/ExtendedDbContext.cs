// Copyright (c) Yahav Gindi Bar; Quartz Technologies, Ltd. All rights reserved.

using EFCachingProvider;
using EFCachingProvider.Caching;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFProviderWrapperToolkit;
using System.Data.Common;
using EFTracingProvider;
using System.Configuration;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using System.IO;

namespace EFCodeFirstProviderWrapperDemo
{
    public class ExtendedDbContext : DbContext
    {
        #region Extended ctors

        protected ExtendedDbContext()
            : base() { }

        protected ExtendedDbContext(DbCompiledModel model)
            : base(model) { }

        public ExtendedDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString) { }

        public ExtendedDbContext(DbConnection existingConnection, bool contextOwnsConnection)
            : base(existingConnection, contextOwnsConnection) { }

        public ExtendedDbContext(ObjectContext objectContext, bool dbContextOwnsObjectContext)
            : base(objectContext, dbContextOwnsObjectContext) { }

        public ExtendedDbContext(string nameOrConnectionString, DbCompiledModel model)
            : base(nameOrConnectionString, model) { }

        public ExtendedDbContext(DbConnection existingConnection, DbCompiledModel model, bool contextOwnsConnection)
            : base(existingConnection, model, contextOwnsConnection) { }

        #endregion


        public ExtendedDbContext(string nameOrConnectionString, bool contextOwnsConnection, bool enableTracing, bool enableCaching)
            : base(CreateWrappedConnection(nameOrConnectionString, enableTracing, enableCaching), contextOwnsConnection)
        {
            this.EnableTracing = enableTracing;
            this.EnableCaching = enableCaching;
        }

        /// <summary>
        /// Flag used to simboolize if we've initialized EFCachingProvider.
        /// </summary>
        protected static bool initializedCachingWrapper = false;

        /// <summary>
        /// Flag used to simboolize if we've initialized EFCachingProvider.
        /// </summary>
        protected static bool initializedTracingWrapper = false;

        /// <summary>
        /// Gets or sets the value that indicates if the DbContext should use EFTracingProvider to provide tracing abilities.
        /// </summary>
        public bool EnableTracing { get; set; }

        /// <summary>
        /// Gets or sets the value that indicates if the DbContext should use EFCachingProvider to provide caching abilities.
        /// </summary>
        public bool EnableCaching { get; set; }

        #region Caching Extensions

        /// <summary>
        /// The underlaying caching connection
        /// </summary>
        private EFCachingConnection CachingConnection
        {
            get
            {
                if (!this.EnableCaching)
                {
                    throw new NotSupportedException("The EnableCaching setting must be set to true in order to access this property.");
                }

                return this.UnwrapConnection<EFCachingConnection>();
            }
        }

        /// <summary>
        /// Gets or sets the ICache policy for database entities caching.
        /// </summary>
        public ICache Cache
        {
            get
            {
                if (!this.EnableCaching)
                {
                    throw new NotSupportedException("The EnableCaching setting must be set to true in order to access this property.");
                }

                return this.CachingConnection.Cache;
            }
            set
            {
                if (!this.EnableCaching)
                {
                    throw new NotSupportedException("The EnableCaching setting must be set to true in order to access this property.");
                }

                this.CachingConnection.Cache = value;
            }
        }

        /// <summary>
        /// Gets or sets the CacingPolicy data related to the database entities caching.
        /// </summary>
        public EFCachingProvider.Caching.CachingPolicy CachingPolicy
        {
            get
            {
                if (!this.EnableCaching)
                {
                    throw new NotSupportedException("The EnableCaching setting must be set to true in order to access this property.");
                }

                return this.CachingConnection.CachingPolicy;
            }
            set
            {
                if (!this.EnableCaching)
                {
                    throw new NotSupportedException("The EnableCaching setting must be set to true in order to access this property.");
                }

                this.CachingConnection.CachingPolicy = value;
            }
        }

        #endregion

        #region Tracing Extensions

        private EFTracingConnection TracingConnection
        {
            get { return this.UnwrapConnection<EFTracingConnection>(); }
        }

        public event EventHandler<CommandExecutionEventArgs> CommandExecuting
        {
            add { this.TracingConnection.CommandExecuting += value; }
            remove { this.TracingConnection.CommandExecuting -= value; }
        }

        public event EventHandler<CommandExecutionEventArgs> CommandFinished
        {
            add { this.TracingConnection.CommandFinished += value; }
            remove { this.TracingConnection.CommandFinished -= value; }
        }

        public event EventHandler<CommandExecutionEventArgs> CommandFailed
        {
            add { this.TracingConnection.CommandFailed += value; }
            remove { this.TracingConnection.CommandFailed -= value; }
        }

        //private void AppendToLog(object sender, CommandExecutionEventArgs e)
        //{
        //    if (this.logOutput != null)
        //    {
        //        this.logOutput.WriteLine(e.ToTraceString().TrimEnd());
        //        this.logOutput.WriteLine();
        //    }
        //}

        //public TextWriter Log
        //{
        //    get { return this.logOutput; }
        //    set
        //    {
        //        if ((this.logOutput != null) != (value != null))
        //        {
        //            if (value == null)
        //            {
        //                CommandExecuting -= AppendToLog;
        //            }
        //            else
        //            {
        //                CommandExecuting += AppendToLog;
        //            }
        //        }

        //        this.logOutput = value;
        //    }
        //}


        #endregion

        /// <summary>
        /// Create warpped connection.
        /// </summary>
        /// <param name="nameOrConnectionString">The connection string name in the config file or actual value.</param>
        /// <param name="enableTracing">Enable entities tracing.</param>
        /// <param name="enableCaching">Enable entities caching.</param>
        /// <returns>The wrapped connection.</returns>
        protected static DbConnection CreateWrappedConnection(string nameOrConnectionString, bool enableTracing, bool enableCaching)
        {
            //-------------------------------------------------------
            //  Init
            //-------------------------------------------------------

            /* Setup variables */
            DbConnection connection = null;
            string providerInvariantName = "System.Data.SqlClient";
            string connectionString = nameOrConnectionString;
            
            if (enableTracing && !initializedTracingWrapper)
            {
                EFTracingProviderConfiguration.RegisterProvider();
#if DEBUG
                EFTracingProviderConfiguration.LogToConsole = true;
#endif

                initializedTracingWrapper = true;
            }

            if (enableCaching && !initializedCachingWrapper)
            {
                EFCachingProviderConfiguration.RegisterProvider();
                initializedCachingWrapper = true;
            }

            //-------------------------------------------------------
            //  Do we got a "name={DbConnectionNameInConfigFile} format?
            //-------------------------------------------------------
            int index = nameOrConnectionString.IndexOf('=');
            if (index > -1
                 && nameOrConnectionString.Substring(0, index).Trim().Equals("name", StringComparison.OrdinalIgnoreCase))
            {
                /* Get the actual connection name */
                nameOrConnectionString = nameOrConnectionString.Substring(index + 1).Trim();
            }

            //-------------------------------------------------------
            //  Parse the connection string and provider invariant name
            //-------------------------------------------------------
            var connectionStringSetting = ConfigurationManager.ConnectionStrings[nameOrConnectionString];
            if (connectionStringSetting != null)
            {
                providerInvariantName = connectionStringSetting.ProviderName;
                connectionString = connectionStringSetting.ConnectionString;
            }

            //-------------------------------------------------------
            //  Construct the basic underlaying connection
            //-------------------------------------------------------
            DbProviderFactory factory = DbProviderFactories.GetFactory(providerInvariantName);
            connection = factory.CreateConnection();
            try
            {
                connection.ConnectionString = connectionString;
            }
            catch { }

            //-------------------------------------------------------
            //  Should we use tracking
            //-------------------------------------------------------
            if (enableTracing)
            {
                connection = new EFTracingConnection(connection, "System.Data.SqlClient");
            }

            //-------------------------------------------------------
            //  Should we use caching
            //  NOTE: Caching test and wrapping MUST COME AFTER Tracing OTHERWISE IT'LL WON'T WORK!
            //-------------------------------------------------------

            if (enableCaching)
            {
                if (enableTracing)
                {
                    connection = new EFCachingConnection(connection, "EFTracingProvider");
                }
                else
                {
                    connection = new EFCachingConnection(connection, "System.Data.SqlClient");
                }
            }

            return connection;
        }
    }

    public static class DbContextExtensions
    {
        /// <summary>
        /// Gets the underlying wrapper connection from the <see cref="DbContext"/>.
        /// </summary>
        /// <typeparam name="TConnection">Connection type.</typeparam>
        /// <param name="context">The db context.</param>
        /// <returns>Wrapper connection of a given type.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Type parameter must be specified explicitly.")]
        public static TConnection UnwrapConnection<TConnection>(this DbContext context)
            where TConnection : DbConnection
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            return ((System.Data.Entity.Infrastructure.IObjectContextAdapter)context).ObjectContext.Connection.UnwrapConnection<TConnection>();
        }

        /// <summary>
        /// Tries to get the underlying wrapper connection from the <see cref="DbContext"/>.
        /// </summary>
        /// <typeparam name="TConnection">Connection type.</typeparam>
        /// <param name="context">The db context.</param>
        /// <param name="result">The result connection.</param>
        /// <returns>A value of true if the given connection type was found in the provider chain, false otherwise.</returns>
        public static bool TryUnwrapConnection<TConnection>(this DbContext context, out TConnection result) where TConnection : DbConnection
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            return ((System.Data.Entity.Infrastructure.IObjectContextAdapter)context).ObjectContext.Connection.TryUnwrapConnection<TConnection>(out result);
        }
    }
}
