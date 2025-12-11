using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using OBase.Pazaryeri.Api.Attributes;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;

namespace OBase.Pazaryeri.Api.Filters
{
    /// <summary>
    /// Swagger dokümantasyonunda endpoint'leri ve schema'ları konfigürasyona göre filtreleyen document filter.
    /// Bu filter, SwaggerSettings konfigürasyonuna ve SwaggerEndpointGroupAttribute'a göre
    /// endpoint'leri ve kullanılmayan schema'ları Swagger UI'da gösterir veya gizler.
    /// </summary>
    public class SwaggerEndpointFilter : IDocumentFilter
    {
        private readonly SwaggerSettings _swaggerSettings;

        public SwaggerEndpointFilter(IOptions<SwaggerSettings> swaggerSettings)
        {
            _swaggerSettings = swaggerSettings.Value;
        }

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            // Eğer filtreleme aktif değilse, hiçbir şey yapma
            if (!_swaggerSettings.EnableFiltering)
                return;

            // Eğer ShowUngroupedEndpoints = false ise, tüm endpoint'leri göster (filtreleme yapma)
            if (!_swaggerSettings.ShowUngroupedEndpoints)
                return;

            // Eğer enabled groups listesi boşsa, tüm endpoint'leri göster
            if (_swaggerSettings.EnabledEndpointGroups == null || !_swaggerSettings.EnabledEndpointGroups.Any())
                return;

            // Silinecek path'leri bul
            var pathsToRemove = swaggerDoc.Paths
                .Where(path => ShouldRemovePath(path, context))
                .Select(x => x.Key)
                .ToList();

            // Path'leri sil
            foreach (var path in pathsToRemove)
            {
                swaggerDoc.Paths.Remove(path);
            }

            // Kullanılan schema'ları topla
            var usedSchemas = CollectUsedSchemas(swaggerDoc);

            // Kullanılmayan schema'ları sil
            RemoveUnusedSchemas(swaggerDoc, usedSchemas);
        }

        private bool ShouldRemovePath(System.Collections.Generic.KeyValuePair<string, OpenApiPathItem> path, DocumentFilterContext context)
        {
            // Path içindeki tüm operasyonları kontrol et
            var operations = path.Value.Operations;

            foreach (var operation in operations)
            {
                // Operation'a karşılık gelen method'u bul
                var apiDescription = context.ApiDescriptions
                    .FirstOrDefault(desc =>
                        desc.RelativePath?.TrimStart('/') == path.Key.TrimStart('/') &&
                        desc.HttpMethod?.ToUpper() == operation.Key.ToString().ToUpper());

                if (apiDescription != null)
                {
                    // Method'un attribute'larını kontrol et
                    var methodInfo = apiDescription.ActionDescriptor.EndpointMetadata
                        .OfType<MethodInfo>()
                        .FirstOrDefault();

                    if (methodInfo == null)
                    {
                        // Eğer MethodInfo bulunamazsa, alternative yöntemle dene
                        var actionDescriptor = apiDescription.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
                        methodInfo = actionDescriptor?.MethodInfo;
                    }

                    if (methodInfo != null)
                    {
                        // Method veya Controller üzerindeki SwaggerEndpointGroupAttribute'ları al
                        var methodAttributes = methodInfo.GetCustomAttributes<SwaggerEndpointGroupAttribute>(false);
                        var controllerAttributes = methodInfo.DeclaringType?.GetCustomAttributes<SwaggerEndpointGroupAttribute>(false);

                        var allAttributes = methodAttributes.Concat(controllerAttributes ?? Enumerable.Empty<SwaggerEndpointGroupAttribute>());

                        // Eğer hiç attribute yoksa, sil (grup atanmamış endpoint'ler gösterilmez)
                        if (!allAttributes.Any())
                        {
                            return true;
                        }

                        // Attribute'ların herhangi biri enabled groups içinde var mı?
                        var hasEnabledGroup = allAttributes.Any(attr =>
                            _swaggerSettings.EnabledEndpointGroups.Contains(attr.GroupName));

                        // Liste içindeyse göster, değilse sil
                        return !hasEnabledGroup;
                    }
                }
            }

            // Default olarak kaldır
            return true;
        }

        /// <summary>
        /// Swagger dokümantasyonunda kullanılan tüm schema'ları toplar
        /// </summary>
        private HashSet<string> CollectUsedSchemas(OpenApiDocument swaggerDoc)
        {
            var usedSchemas = new HashSet<string>();

            // Tüm path'lerdeki operasyonları gez
            foreach (var path in swaggerDoc.Paths.Values)
            {
                foreach (var operation in path.Operations.Values)
                {
                    // Request body schema'larını topla
                    if (operation.RequestBody?.Content != null)
                    {
                        foreach (var content in operation.RequestBody.Content.Values)
                        {
                            CollectSchemaReferences(content.Schema, usedSchemas);
                        }
                    }

                    // Response schema'larını topla
                    if (operation.Responses != null)
                    {
                        foreach (var response in operation.Responses.Values)
                        {
                            if (response.Content != null)
                            {
                                foreach (var content in response.Content.Values)
                                {
                                    CollectSchemaReferences(content.Schema, usedSchemas);
                                }
                            }
                        }
                    }

                    // Parameter schema'larını topla
                    if (operation.Parameters != null)
                    {
                        foreach (var parameter in operation.Parameters)
                        {
                            CollectSchemaReferences(parameter.Schema, usedSchemas);
                        }
                    }
                }

                // Path-level parameters
                if (path.Parameters != null)
                {
                    foreach (var parameter in path.Parameters)
                    {
                        CollectSchemaReferences(parameter.Schema, usedSchemas);
                    }
                }
            }

            return usedSchemas;
        }

        /// <summary>
        /// Bir schema'dan referans edilen tüm schema'ları recursively toplar
        /// </summary>
        private void CollectSchemaReferences(OpenApiSchema schema, HashSet<string> usedSchemas)
        {
            if (schema == null)
                return;

            // Referans varsa ekle
            if (!string.IsNullOrEmpty(schema.Reference?.Id))
            {
                var schemaName = schema.Reference.Id;
                if (usedSchemas.Add(schemaName))
                {
                    // Yeni eklenen schema'nın içindeki referansları da topla
                    // Bu nested schema'ları yakalamak için önemli
                }
            }

            // AllOf, OneOf, AnyOf içindeki schema'ları kontrol et
            if (schema.AllOf != null)
            {
                foreach (var subSchema in schema.AllOf)
                {
                    CollectSchemaReferences(subSchema, usedSchemas);
                }
            }

            if (schema.OneOf != null)
            {
                foreach (var subSchema in schema.OneOf)
                {
                    CollectSchemaReferences(subSchema, usedSchemas);
                }
            }

            if (schema.AnyOf != null)
            {
                foreach (var subSchema in schema.AnyOf)
                {
                    CollectSchemaReferences(subSchema, usedSchemas);
                }
            }

            // Properties içindeki schema'ları kontrol et
            if (schema.Properties != null)
            {
                foreach (var property in schema.Properties.Values)
                {
                    CollectSchemaReferences(property, usedSchemas);
                }
            }

            // Array items
            if (schema.Items != null)
            {
                CollectSchemaReferences(schema.Items, usedSchemas);
            }

            // AdditionalProperties
            if (schema.AdditionalProperties != null)
            {
                CollectSchemaReferences(schema.AdditionalProperties, usedSchemas);
            }
        }

        /// <summary>
        /// Kullanılmayan schema'ları dokümantasyondan siler
        /// </summary>
        private void RemoveUnusedSchemas(OpenApiDocument swaggerDoc, HashSet<string> usedSchemas)
        {
            if (swaggerDoc.Components?.Schemas == null)
                return;

            // Nested schema'ları da kontrol et
            // Kullanılan schema'ların içinde referans edilen schema'ları bul
            var allUsedSchemas = new HashSet<string>(usedSchemas);
            var previousCount = 0;

            // Tüm nested referanslar bulunana kadar devam et
            while (allUsedSchemas.Count != previousCount)
            {
                previousCount = allUsedSchemas.Count;

                foreach (var schemaName in allUsedSchemas.ToList())
                {
                    if (swaggerDoc.Components.Schemas.TryGetValue(schemaName, out var schema))
                    {
                        CollectSchemaReferences(schema, allUsedSchemas);
                    }
                }
            }

            // Kullanılmayan schema'ları sil
            var schemasToRemove = swaggerDoc.Components.Schemas
                .Where(s => !allUsedSchemas.Contains(s.Key))
                .Select(s => s.Key)
                .ToList();

            foreach (var schemaName in schemasToRemove)
            {
                swaggerDoc.Components.Schemas.Remove(schemaName);
            }
        }
    }
}
