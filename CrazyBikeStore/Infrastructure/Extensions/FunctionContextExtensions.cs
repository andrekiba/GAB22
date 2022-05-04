using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace CrazyBikeStore.Infrastructure.Extensions
{
    public static class FunctionContextExtensions
    {
        //Thanks to juunas11
        //https://github.com/juunas11/IsolatedFunctionsAuthentication/blob/main/IsolatedFunctionAuth/Middleware/FunctionContextExtensions.cs
        public static void SetHttpResponseStatusCode(this FunctionContext context, HttpStatusCode statusCode)
        {
            var coreAssembly = Assembly.Load("Microsoft.Azure.Functions.Worker.Core");
            const string featureInterfaceName = "Microsoft.Azure.Functions.Worker.Context.Features.IFunctionBindingsFeature";
            var featureInterfaceType = coreAssembly.GetType(featureInterfaceName);
            var bindingsFeature = context.Features.Single(f => f.Key.FullName == featureInterfaceType?.FullName).Value;
            var invocationResultProp = featureInterfaceType?.GetProperty("InvocationResult");

            var grpcAssembly = Assembly.Load("Microsoft.Azure.Functions.Worker.Grpc");
            var responseDataType = grpcAssembly.GetType("Microsoft.Azure.Functions.Worker.GrpcHttpResponseData");
            var responseData = Activator.CreateInstance(responseDataType, context, statusCode);

            invocationResultProp?.SetMethod?.Invoke(bindingsFeature, new[] { responseData });
        }
        public static MethodInfo GetTargetFunctionMethod(this FunctionContext context)
        {
            var entryPoint = context.FunctionDefinition.EntryPoint;

            var assemblyPath = context.FunctionDefinition.PathToAssembly;
            var assembly = Assembly.LoadFrom(assemblyPath);
            var typeName = entryPoint.Substring(0, entryPoint.LastIndexOf('.'));
            var type = assembly.GetType(typeName);
            var methodName = entryPoint.Substring(entryPoint.LastIndexOf('.') + 1);
            var method = type?.GetMethod(methodName);
            return method;
        }
        
        //Thanks https://github.com/Azure/azure-functions-dotnet-worker/issues/414#issuecomment-872818004
        public static HttpRequestData GetHttpRequestData(this FunctionContext functionContext)
        {
            try
            {
                var keyValuePair = functionContext.Features.SingleOrDefault(f => f.Key.Name == "IFunctionBindingsFeature");
                if (keyValuePair.Equals(default(KeyValuePair<Type, object>))) 
                    return null;
                var functionBindingsFeature = keyValuePair.Value;
                var type = functionBindingsFeature.GetType();
                var inputData = type.GetProperties().Single(p => p.Name == "InputData")
                    .GetValue(functionBindingsFeature) as IReadOnlyDictionary<string, object>;
                return inputData?.Values.SingleOrDefault(o => o is HttpRequestData) as HttpRequestData;
            }
            catch
            {
                return null;
            }
        }
        public static HttpResponseData GetHttpResponseData(this FunctionContext functionContext)
        {
            try
            {
                var request = functionContext.GetHttpRequestData();
                if (request is null)
                    return null;
                var response = HttpResponseData.CreateResponse(request);
                var keyValuePair = functionContext.Features.FirstOrDefault(f => f.Key.Name == "IFunctionBindingsFeature");
                if (keyValuePair.Equals(default(KeyValuePair<Type, object>))) 
                    return null;
                var functionBindingsFeature = keyValuePair.Value;
                var invocationResult = functionBindingsFeature.GetType().GetProperty("InvocationResult");
                invocationResult?.SetValue(functionBindingsFeature, response);
                return response;
            }
            catch
            {
                return null;
            }
        }
    }
}