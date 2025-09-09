using AutoMapper;
using GAC.WMS.Integrations.Application.Mappings;
using Microsoft.Extensions.Logging;
using Moq;
using System;

namespace GAC.WMS.Integrations.UnitTests.Helpers
{
    public static class TestHelper
    {
        /// <summary>
        /// Creates a real mapper instance with the application's mapping profile
        /// </summary>
        public static IMapper CreateMapper()
        {
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            
            return mapperConfig.CreateMapper();
        }

        /// <summary>
        /// Creates a mock logger for the specified type
        /// </summary>
        public static Mock<ILogger<T>> CreateMockLogger<T>()
        {
            return new Mock<ILogger<T>>();
        }
    }
}
