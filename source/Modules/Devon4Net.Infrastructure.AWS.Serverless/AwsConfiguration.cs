﻿using Amazon.Runtime.CredentialManagement;
using Amazon.Runtime;
using Devon4Net.Application.WebAPI.Configuration.Application;
using Devon4Net.Infrastructure.AWS.Common.Options;
using Devon4Net.Infrastructure.AWS.ParameterStore;
using Devon4Net.Infrastructure.AWS.Secrets;
using Devon4Net.Infrastructure.Common.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Amazon;
using Devon4Net.Infrastructure.AWS.SQS.Handlers;
using Devon4Net.Infrastructure.Logger.Logging;

namespace Devon4Net.Infrastructure.AWS.Serverless
{
    public static class AwsConfiguration
    {
        private static AwsOptions AwsOptions { get; set; }

        public static void ConfigureDevonfwAWS(this IServiceCollection services, IConfiguration configuration, bool setupDevon = false)
        {
            if (setupDevon)
            {
                services.SetupDevonfw(configuration);
            }

            AwsOptions = services.GetTypedOptions<AwsOptions>(configuration, "AWS");
            if (AwsOptions.EnableAws)
            {
                var credentials = services.LoadAwsCredentials();

                if (credentials != null)
                {
                    var awsRegion = services.LoadAwsRegionEndpoint();

                    if (AwsOptions.UseSecrets)
                    {
                        (configuration as IConfigurationBuilder)?.AddSecretsHandler(credentials, awsRegion);
                    }

                    if (AwsOptions.UseParameterStore)
                    {
                        (configuration as IConfigurationBuilder)?.AddParameterStoreHandler(credentials, awsRegion);
                    }

                    if (AwsOptions.UseSqs)
                    {
                        services.AddSingleton<ISqsClientHandler, SqsClientHandler>();
                    }
                }
                else
                {
                    RaiseCredentialsException();
                }
            }
        }

        private static void RaiseCredentialsException()
        {
            Devon4NetLogger.Error("The AWS Credentials are null");
            throw new AWSCommonRuntimeException("The AWS Credentials can not be null. Please check the AWS options in the configuration file");
        }

        private static AWSCredentials LoadAwsCredentials(this IServiceCollection services)
        {
            AWSCredentials credentials = null;

            if (!string.IsNullOrEmpty(AwsOptions.Credentials.AccessKeyId) && !string.IsNullOrEmpty(AwsOptions.Credentials.SecretAccessKey))
            {
                credentials = new BasicAWSCredentials(AwsOptions.Credentials.AccessKeyId, AwsOptions.Credentials.SecretAccessKey);
            }
            else
            {
                if (!string.IsNullOrEmpty(AwsOptions.Credentials.Profile))
                {
                    var sharedFile = new SharedCredentialsFile();
                    sharedFile.TryGetProfile(AwsOptions.Credentials.Profile, out var profile);
                    AWSCredentialsFactory.TryGetAWSCredentials(profile, sharedFile, out credentials);
                }
            }

            if (credentials != null) services.AddSingleton(credentials);
            return credentials;
        }

        private static RegionEndpoint LoadAwsRegionEndpoint(this IServiceCollection services)
        {
            RegionEndpoint result = null;
            if (!string.IsNullOrEmpty(AwsOptions.Credentials.Region))
            {
                result = RegionEndpoint.GetBySystemName(AwsOptions.Credentials.Region);
            }

            if (result!=null) services.AddSingleton(result);
            return result;
        }
    }
}
