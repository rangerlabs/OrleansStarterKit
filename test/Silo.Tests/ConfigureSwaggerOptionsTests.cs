﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Moq;
using Silo.Options;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using Xunit;

namespace Silo.Tests
{
    public class ConfigureSwaggerOptionsTests
    {
        [Fact]
        public void ConfigureSwaggerOptions_Configures_Expected_Options()
        {
            // arrange
            var group = "v1";
            var provider = new Mock<IApiVersionDescriptionProvider>();
            provider.Setup(_ => _.ApiVersionDescriptions)
                .Returns(new List<ApiVersionDescription>
                {
                    new ApiVersionDescription(new ApiVersion(1, 0), group, false)
                });
            var options = Mock.Of<IOptions<SupportApiOptions>>(_ => _.Value.Title == "SomeTitle");

            // act
            var config = new ConfigureSwaggerOptions(provider.Object, options);
            var target = new SwaggerGenOptions();
            config.Configure(target);

            // assert the swagger doc is there
            Assert.Single(target.SwaggerGeneratorOptions.SwaggerDocs);
            Assert.True(target.SwaggerGeneratorOptions.SwaggerDocs.ContainsKey(group));
            Assert.Equal(options.Value.Title, target.SwaggerGeneratorOptions.SwaggerDocs[group].Title);
            Assert.Equal("1.0", target.SwaggerGeneratorOptions.SwaggerDocs[group].Version);

            // assert the remove operation filter is there
            Assert.Contains(target.OperationFilterDescriptors, _ => _.Type == typeof(RemoveVersionFromParameterOperationFilter));

            // assert the replace document filter is there
            Assert.Contains(target.DocumentFilterDescriptors, _ => _.Type == typeof(ReplaceVersionWithExactValueInPathDocumentFilter));

            // assert the xml include filter is there
            Assert.Contains(target.SchemaFilterDescriptors, _ => _.Type == typeof(XmlCommentsSchemaFilter));

            // assert the annotation filter is there
            Assert.Contains(target.ParameterFilterDescriptors, _ => _.Type == typeof(AnnotationsParameterFilter));
        }

        [Fact]
        public void ConfigureSwaggerOptions_Refuses_Null_Provider()
        {
            var error = Assert.Throws<ArgumentNullException>(() =>
            {
                new ConfigureSwaggerOptions(null, Mock.Of<IOptions<SupportApiOptions>>());
            });
            Assert.Equal("provider", error.ParamName);
        }

        [Fact]
        public void ConfigureSwaggerOptions_Refuses_Null_Options()
        {
            var provider = Mock.Of<IApiVersionDescriptionProvider>();
            var error = Assert.Throws<ArgumentNullException>(() =>
            {
                new ConfigureSwaggerOptions(provider, null);
            });
            Assert.Equal("options", error.ParamName);
        }
    }
}
