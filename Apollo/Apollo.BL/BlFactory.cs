using Apollo.BLInterface;
using Apollo.Core.Dal.Common;
using Apollo.Core.Dal.Dao;
using Apollo.Core.Dal.Interface;
using Apollo.Pay;
using Apollo.Print;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

using System.Collections.Generic;
using System.Text;

namespace Apollo.BL
{
    public abstract class BlFactory
    {
        public static IServiceCollection RegisterServices(IServiceCollection services, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            return services.AddSingleton<IConnectionFactory, DefaultConnectionFactory>((arg) => connectionFactory(arg, configuration))
                .AddSingleton<IQueryBuilderFactory, DefaultQueryBuilderFactory>((arg) => queryBuilderFactory(arg, configuration))
                .AddSingleton<IMovieBl, MovieBl>()
                .AddSingleton<ICinemaBl, CinemaBl>()
                .AddSingleton<IReservationBl, ReservationBl>()
                .AddSingleton<IScheduleBl, ScheduleBl>()
                .AddSingleton<IConstraintBl, ConstraintBl>()
                .AddSingleton<IPersonBl, PersonBl>()
                .AddSingleton<BLInterface.IConfiguration, ConfigurationBl>()

                .AddSingleton<IPersonDao, PersonDao>()
                .AddSingleton<IUserDao, UserDao>()
                .AddSingleton<IMovieDao, MovieDao>()
                .AddSingleton<IGenreDao, GenreDao>()
                .AddSingleton<IRoleDao, RoleDao>()
                .AddSingleton<IScheduleDao, ScheduleDao>()
                .AddSingleton<IReservationDao, ReservationDao>()
                .AddSingleton<ICinemaDao, CinemaDao>()
                .AddSingleton<ICinemaHallDao, CinemaHallDao>()
                .AddSingleton<IConfigurationDao, ConfigurationDao>()
                
                .AddSingleton<IPrint, ApolloPrint>()
                .AddSingleton<Pay.IPayment>(provider => PaymentFactory.GetApolloPay(
                    configuration.GetSection("PaymentProvider.ApiKey").Value, 
                    configuration.GetSection("PaymentProvider").Value))
                .AddSingleton<ISearchBl, SearchBl>()

                .AddSingleton<IImageService>((arg) => ImageServiceFactory(arg, configuration))
                ;
        }

        private static IImageService ImageServiceFactory(IServiceProvider arg, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            return new ImageService(configuration.GetSection("ImageRootPath").Value);
        }

        private static DefaultQueryBuilderFactory queryBuilderFactory(IServiceProvider arg, Microsoft.Extensions.Configuration.IConfiguration config)
        {
            var dbConfig = config.GetSection("ConnectionStrings")
                .GetSection("ApolloDbConnection");

            return new DefaultQueryBuilderFactory(dbConfig.GetSection("QueryBuilder").Value);
        }

        private static DefaultConnectionFactory connectionFactory(IServiceProvider arg, Microsoft.Extensions.Configuration.IConfiguration config)
        {
            var dbConfig = config.GetSection("ConnectionStrings")
                .GetSection("ApolloDbConnection");

            var connectionString = dbConfig.GetSection("ConnectionString").Value;
            var providerName = dbConfig.GetSection("ProviderName").Value;

            return new DefaultConnectionFactory(connectionString, providerName);
        }
    }
}
